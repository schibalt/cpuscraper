using HtmlAgilityPack;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;

namespace cpuScraper
{
    static class Utils
    {
        public static Regex MCSeriesRegex = new Regex(@"(i[357]|FX|A(6|10)[ |-]|Opteron|Athlon|Sempron|Pentium 4|Celeron)?", RegexOptions.Compiled);

        public static Regex MCModelRegex = new Regex(@"E[35]( |-)?\d{4} ?[vV]\d", RegexOptions.Compiled);
        public static Regex MCModelRegex2 = new Regex(@"[A-Z]?\d{4}([A-Za-z]){0,2}", RegexOptions.Compiled);
        public static Regex MCModelRegex3 = new Regex(@"\d\.\dGHz", RegexOptions.Compiled);

        public static Regex DiscountRegex = new Regex(@"\d{2}", RegexOptions.Compiled);

        public static Regex CpuBenchBrandRegex = new Regex(@"(Intel|AMD|HP)?", RegexOptions.Compiled);

        public static Regex CpuBenchSeriesRegex = new Regex(@"i[357]|[[mM]\d?]|[FR]X[ |-]|Phenom II X?\d\d? |Opteron|Athlon (X4|II X3 |64 )?|A(6|8|10|12(PRO )?[ |-])|Turion (II Neo|X2)|Sempron|Pentium [45M]|Celeron", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex CpuBenchModelRegex = new Regex(@"E[35]-\d{4} v[1-5]", RegexOptions.Compiled);
        public static Regex CpuBenchModelRegex2 = new Regex(@"([a-z]\d{2}[a-z]?|[A-Z]?\d{3,4}[A-Z]?|\d[A-Z]\d\d|TWKR|Hexa-Core)[a-z]{0,2}", RegexOptions.Compiled);
        public static Regex CpuBenchModelRegex3 = new Regex(@"[A-Z]{1,2}-\d\d", RegexOptions.Compiled);
        public static Regex CpuBenchModelRegex4 = new Regex(@"[A-Z]-[A-Z]\d\d[A-Z]", RegexOptions.Compiled);
        public static Regex CpuBenchModelRegex5 = new Regex(@"\d\.\d[1-9]?GHz", RegexOptions.Compiled);
        public static Regex CpuBenchModelRegex6 = new Regex(@"(\d\.\d)0?(GHz)", RegexOptions.Compiled);

        public enum BenchTypes { PassMark, FutureMark, PassMarkRanged };

        public static ushort PassMarkCeiling = 25000;
        public static ushort FutureMarkCeiling = 12000;

        public static string[] NameNodeXPath = new string[]
        {
            "td[1]/a[2]",
            "td[3]/a[1]",

            "td[1]"
        };

        public static string passmarkRangedPageNameNodePath = "descendant::*[starts-with(@id,'rk')]/a";

        public static string[] ValueNodeXPath = new string[]
        {
            "td[3]",
            "td[5]/div/div",

            "td[2]"
        };

        public static string passmarkRangedPageValueNodePath = "descendant::*[starts-with(@id,'rt')]/div";

        public static void PrintConsole<T>(this T argument)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(argument))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(argument);
                Console.WriteLine("{0}\t{1}", name, value);
            }
            //text += string.Format("\n");
        }

        public static string Print<T>(this T argument)
        {
            var text = "";
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(argument))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(argument);
                text += string.Format("{0}\t{1}\r\n", name, value);
            }
            //text += string.Format("\n");
            return text;
        }

        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }


    class Cpu
    {
        public uint MCId { get; set; }

        public decimal Price { get; set; }

        public string Brand { get; set; }

        public string Series { get; set; }

        public string Model { get; set; }

        public string Name { get; set; }

        public byte Discount { get; set; }

        public Benchmark PassMark;

        public Benchmark FutureMark;

        public Benchmark Geekbench;

        public Cpu(HtmlNode node)
        {
            var anchor = node.SelectSingleNode("descendant::a[starts-with(@id,'hypProductH2_')]");
            MCId = Convert.ToUInt32(anchor.Attributes.Where(a => a.Name.Equals("data-id")).First().Value);
            Name = anchor.Attributes.Where(a => a.Name.Equals("data-name")).First().Value;
            Price = Convert.ToDecimal(anchor.Attributes.Where(a => a.Name.Equals("data-price")).First().Value);
            Brand = anchor.Attributes.Where(a => a.Name.Equals("data-brand")).First().Value;

            MatchCollection matches = Utils.MCSeriesRegex.Matches(Name);

            Series = "";

            for (var matchIdx = 0; matchIdx < matches.Count; ++matchIdx)
                if (matches[matchIdx].Value.Length > Series.Length)
                    Series = matches[matchIdx].Value.Replace(" ", "").Replace("-", "").ToUpper();

            if (MCId == 430516)
            {
                Model = "6376";
                return;
            }

            Model = "";

            var modelRegexes = new List<Regex>
            {
                Utils.MCModelRegex,
                Utils.MCModelRegex2,
                Utils.MCModelRegex3
            };

            var match = false;

            for (var regex = 0; regex < modelRegexes.Count && !match; ++regex)
            {
                matches = modelRegexes[regex].Matches(Name);
                match = matches.Count > 0;

                if (match)
                    Model = matches[0].Value.Replace(" ", "").Replace("-", "").ToUpper();
            }

            var discountNode = node.SelectNodes("descendant::div[@class='highlight clear']").First();

            matches = Utils.DiscountRegex.Matches(discountNode.InnerText);

            if (matches.Count > 0)
                Discount = Convert.ToByte(matches[0].Groups[0].Value);

            if (string.IsNullOrWhiteSpace(Series) && string.IsNullOrWhiteSpace(Model))
                throw new ArgumentException();

        }
    }

    class Benchmark
    {

        public string Brand { get; set; }

        public string Series { get; set; }

        public string Model { get; set; }

        public string Name { get; set; }

        public ushort Value { get; set; }

        public Benchmark(HtmlNode node, List<Benchmark> passMarks, int type)
        {
            var nameAnchor = node.SelectSingleNode(Utils.NameNodeXPath[type]);

            if (nameAnchor != null)
                Name = nameAnchor.InnerText;
            else
                return;

            MatchCollection matches = Utils.CpuBenchBrandRegex.Matches(nameAnchor.InnerText);

            Brand = matches[0].Value;

            matches = Utils.CpuBenchSeriesRegex.Matches(nameAnchor.InnerText);

            if (matches.Count > 0)
                Series = matches[0].Value.Replace(" ", "").Replace("-", "").ToUpper();

            var modelRegexes = new List<Regex>
            {
                Utils.CpuBenchModelRegex,
                Utils.CpuBenchModelRegex2,
                Utils.CpuBenchModelRegex3,
                Utils.CpuBenchModelRegex4,
                Utils.CpuBenchModelRegex5
            };

            var match = false;

            for (var regex = 0; regex < modelRegexes.Count && !match; ++regex)
            {
                matches = modelRegexes[regex].Matches(nameAnchor.InnerText);
                match = matches.Count > 0;

                if (match)
                {
                    Model = matches[0].Value.Replace(" ", "").Replace("-", "").ToUpper();
                }
            }

            if (!match)
            {
                matches = Utils.CpuBenchModelRegex6.Matches(nameAnchor.InnerText);
                match = matches.Count > 0;

                if (match)
                    Model = (matches[0].Groups[1].Value + matches[0].Groups[2].Value).Replace(" ", "").Replace("-", "").ToUpper();
            }

            nameAnchor = node.SelectSingleNode(Utils.ValueNodeXPath[(int)type]);

            Value = Convert.ToUInt16(nameAnchor.InnerText.Trim().Replace(",", ""));

            if (string.IsNullOrWhiteSpace(Series) && string.IsNullOrWhiteSpace(Model))
            {
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(node.OuterHtml);
                this.PrintConsole();
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                throw new ArgumentNullException();
            }

            passMarks.Add(this);
        }
    }

    class Program
    {
        static void SetBenchmark(Benchmark benchmark, ref Benchmark toSet)
        {
            if (toSet == null)
            {
                toSet = benchmark;
            }
        }

        static void Main(string[] args)
        {
            var htmlDoc = new HtmlDocument();
            var mcCpus = new List<Cpu>();
            var passMarks = new List<Benchmark>();
            var futureMarks = new List<Benchmark>();

            var pages = new List<string>
            {
                        "Processors_CPUs   Computer Parts   Micro Center.htm",
                        "Processors_CPUs   Computer Parts   Micro Center2.htm",
                        "Processors_CPUs   Computer Parts   Micro Center3.htm"
            };

            var webpages = new List<string>
            {
                        "http://www.microcenter.com/search/search_results.aspx?N=4294966995&page=1",
                        "http://www.microcenter.com/search/search_results.aspx?N=4294966995&page=2",
                        "http://www.microcenter.com/search/search_results.aspx?N=4294966995&page=3"
            };

            #region read benchmarks

            var paths = new List<string>
                    {
                        "benchmarks.txt",
                        "futuremarks.txt",
                        "geekbench.txt"
                    };

            HtmlNodeCollection nodes = null;
            string path = @"mccpus.txt";

            File.WriteAllText(path, string.Empty);

            var useWeb = args[0].Equals("web");

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                var cookies = new CookieContainer();
                var uri = new Uri("http://www.microcenter.com");
                cookies.Add(uri, new Cookie("storeSelected", "051"));
                cookies.Add(uri, new Cookie("ipp", "25"));

                for (var microCtrPgNo = 0; microCtrPgNo < 3; microCtrPgNo++)
                {
                    if (useWeb)
                    {

                        using (var httpClient = new HttpClient(new HttpClientHandler
                        {
                            UseDefaultCredentials = true,
                            CookieContainer = cookies
                        }))
                        {
                            httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

                            var stream = httpClient.GetStreamAsync(webpages[microCtrPgNo]).Result;

                            // filePath is a path to a file containing the html
                            htmlDoc.Load(stream);


                        } // httpclient
                    }
                    else
                    {
                        htmlDoc.Load(pages[microCtrPgNo]);
                    }

                    nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='detail_wrapper']");

                    foreach (var node in nodes)
                    {
                        var cpu = new Cpu(node);
                        mcCpus.Add(cpu);
                        sw.WriteLine(cpu.Print());
                    }
                } // iterate pages
            }

            pages = new List<string>
                    {
                        "PassMark - CPU Benchmarks - CPU Mega Page - Detailed List of Benchmarked CPUs.htm",
                        "Best Processors June - 2016.htm",
                        "Processor Benchmarks - Geekbench Browser.htm"
                    };

            webpages = new List<string>
                    {
                        "https://www.cpubenchmark.net/CPU_mega_page.html",
                        "http://www.futuremark.com/hwc/hwcenter/page-main.php?type=cpu&filters=desktop,mobile,server",
                        "https://browser.primatelabs.com/processor-benchmarks"
                    };

            var benchContainerXPaths = new List<string>
                    {
                        "//descendant::table[@id='cputable']/tbody/tr",
                        "//descendant::table[@id='productTable']/tbody/tr",
                        "//descendant::div[@id='4']/table[@id='pc64']/tbody/tr"

                    };

            var benchContainerWebXPaths = new List<string>
                    {
                        "//descendant::table[@id='cputable']/tbody/tr",
                        "//*[@id=\"productTable\"]/tr",
                        "//descendant::div[@id='4']/table[@id='pc64']/tbody/tr"
                    };

            for (var page = 0; page < pages.Count; ++page)
            {
                File.WriteAllText(paths[page], string.Empty);

                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(paths[page]))
                {
                    if (useWeb)
                    {
                        using (var httpClient = new HttpClient(new HttpClientHandler
                        {
                            UseDefaultCredentials = true
                        }))
                        {
                            var stream = httpClient.GetStreamAsync(webpages[page]).Result;

                            // filePath is a path to a file containing the html
                            htmlDoc.Load(stream);

                        } // httpclient
                    }
                    else
                    {
                        htmlDoc.Load(pages[page]);
                    }
                    nodes = htmlDoc.DocumentNode.SelectNodes(useWeb ? benchContainerWebXPaths[page] : benchContainerXPaths[page]);

                    foreach (var node in nodes)
                    {
                        try
                        {
                            var benchmark = new Benchmark(node, passMarks, page);

                            sw.WriteLine(benchmark.Print());

                            var matchingCpus = mcCpus.Where(c => (c.Series + c.Model).Equals((benchmark.Series + benchmark.Model)));
                            //var cpu = matchingCpus.FirstOrDefault();

                            //if (cpu == null) continue;

                            foreach (var cpu in matchingCpus)
                            {
                                switch (page)
                                {
                                    case 0:
                                        SetBenchmark(benchmark, ref cpu.PassMark);
                                        break;
                                    case 1:
                                        SetBenchmark(benchmark, ref cpu.FutureMark);
                                        break;
                                    case 2:
                                        SetBenchmark(benchmark, ref cpu.Geekbench);
                                        break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(node.OuterHtml);
                            Console.WriteLine(e.ToString() + "\n");
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    } // page nodes
                } // bench file writer
            }
            #endregion

            #region excel

            var sheetFile = new FileInfo(@"cpubench.xlsx");
            try
            {
                sheetFile.Delete();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }

            using (var package = new ExcelPackage(sheetFile))
            {

                //Add the Content sheet
                var sheet = package.Workbook.Worksheets.Add("Content");

                sheet.Cells["A1"].Value = "Brand";
                sheet.Cells["B1"].Value = "Name";
                sheet.Cells["C1"].Value = "Price";
                sheet.Cells["D1"].Value = "Discount";
                sheet.Cells["E1"].Value = "Discounted Rounded Price";
                sheet.Cells["F1"].Value = "PassMark";
                sheet.Cells["G1"].Value = "FutureMark";
                sheet.Cells["H1"].Value = "Geekbench";
                sheet.Cells["I1"].Value = "PassMarks/$";
                sheet.Cells["J1"].Value = "FutureMarks/$";
                sheet.Cells["K1"].Value = "Geekbench/$";

                var row = 2;

                var priceMin = mcCpus.Min(c => c.Price);
                var priceMax = mcCpus.Max(c => c.Price);
                //var priceMin = mcCpus.Where(c => c.Price < 800).Min(c => c.Price);
                //var priceMax = mcCpus.Where(c => c.Price < 800).Max(c => c.Price);
                var priceRange = priceMax - priceMin;

                foreach (var cpu in mcCpus)
                {
                    sheet.Cells[row, 1].Value = cpu.Brand;
                    sheet.Cells[row, 2].Value = cpu.Name;
                    sheet.Cells[row, 3].Value = cpu.Price;
                    sheet.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    var coefficient = (cpu.Price - priceMin) / priceRange;
                    coefficient = Math.Max(coefficient, 0);
                    coefficient = Math.Min(coefficient, 1);
                    int red, green;

                    if (coefficient < .25m)
                    {
                        red = 0;
                        green = (int)(510 * coefficient);
                        green += 128;
                    }
                    else if (coefficient < .5m)
                    {
                        red = (int)(coefficient * 510);
                        green = 255;
                    }
                    else
                    {
                        red = 255;
                        green = 255 - (int)(255 * coefficient);
                    }
                    var color = Color.FromArgb(red, green, 0);

                    sheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(color);
                    sheet.Cells[row, 4].Value = cpu.Discount;
                    sheet.Cells[row, 5].Formula = "=MAX(ROUND(C" + row + ", 1)-D" + row + ",1e-304)";
                    if (cpu.PassMark != null)
                    {
                        sheet.Cells[row, 6].Value = cpu.PassMark.Value;
                    }
                    if (cpu.FutureMark != null)
                    {
                        sheet.Cells[row, 7].Value = cpu.FutureMark.Value;
                    }
                    if (cpu.Geekbench != null)
                    {
                        sheet.Cells[row, 8].Value = cpu.Geekbench.Value;
                    }
                    sheet.Cells[row, 9].Formula = "=ROUND(F" + row + "/E" + row + ",1)";
                    sheet.Cells[row, 10].Formula = "=ROUND(G" + row + "/E" + row + ",1)";
                    sheet.Cells[row, 11].Formula = "=ROUND(H" + row + "/E" + row + ",1)";

                    ++row;
                }

                for (var col = 1; col < 8; ++col)
                    sheet.Column(col).AutoFit();

                package.Save();
            }
            #endregion
        }
    }
}
