using HtmlAgilityPack;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        public static string[] NameNodeXPath = new string[] { "td[1]/a[2]", "td[3]/a[1]", "descendant::*[starts-with(@id,'rk')]/a" };
        public static string[] ValueNodeXPath = new string[] { "td[3]", "td[5]/div/div", "descendant::*[starts-with(@id,'rt')]/div" };

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

        public Benchmark PassMark { get; set; }

        public Benchmark FutureMark { get; set; }

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

        public Benchmark(HtmlNode node, List<Benchmark> passMarks, Utils.BenchTypes type)
        {
            var nameAnchor = node.SelectSingleNode(Utils.NameNodeXPath[(int)type]);

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
        static void Main(string[] args)
        {
            var htmlDoc = new HtmlDocument();
            var mcCpus = new List<Cpu>();
            var passMarks = new List<Benchmark>();
            var futureMarks = new List<Benchmark>();

            switch (args[0])
            {
                #region web
                case "web":
                    var cookies = new CookieContainer();
                    using (var httpClient = new HttpClient(new HttpClientHandler
                    {
                        UseDefaultCredentials = true,
                        CookieContainer = cookies
                    }))
                    {
                        var uri = new Uri("http://www.microcenter.com");
                        cookies.Add(uri, new Cookie("storeSelected", "051"));
                        cookies.Add(uri, new Cookie("ipp", "25"));
                        httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                        var requestUri = "http://www.microcenter.com/search/search_results.aspx?N=4294966995+4294964566+4294965455&page={0}";

                        for (var microCtrPgNo = 1; microCtrPgNo < 4; microCtrPgNo++)
                        {
                            var stream = httpClient.GetStreamAsync(string.Format(requestUri, microCtrPgNo)).Result;

                            // filePath is a path to a file containing the html
                            htmlDoc.Load(stream);

                            // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

                            // ParseErrors is an ArrayList containing any errors from the Load statement
                            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                            {
                                // Handle any parse errors as required

                            }

                            if (htmlDoc.DocumentNode != null)
                            {
                                mcCpus = htmlDoc.DocumentNode.SelectNodes("//div[@class='detail_wrapper']").ToList().ConvertAll(n => new Cpu(n));

                                mcCpus.AddRange(mcCpus);

                            }
                        } // iterate pages

                    } // httpclient
                    break;
                #endregion
                case "file":

                    #region microcenter
                    var pages = new List<string> {
                        "Intel _ AMD _ Processors_CPUs _ Computer Parts _ Micro Center.html",
                        "Intel _ AMD _ Processors_CPUs _ Computer Parts _ Micro Center2.html",
                        "Intel _ AMD _ Processors_CPUs _ Computer Parts _ Micro Center3.html" };

                    HtmlNodeCollection nodes = null;

                    string path = @"mccpus.txt";

                    File.WriteAllText(path, string.Empty);

                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        foreach (var microCenterPage in pages)
                        {
                            htmlDoc.Load(microCenterPage);

                            nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='detail_wrapper']");

                            foreach (var node in nodes)
                            {
                                var cpu = new Cpu(node);
                                mcCpus.Add(cpu);
                                sw.WriteLine(cpu.Print());
                            }
                        }
                    }
                    #endregion

                    #region passmark
                    var page = "PassMark - CPU Benchmarks - CPU Mega Page - Detailed List of Benchmarked CPUs.htm";

                    path = @"benchmarks.txt";

                    File.WriteAllText(path, string.Empty);

                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        htmlDoc.Load(page);
                        nodes = htmlDoc.DocumentNode.SelectNodes("//descendant::table[@id='cputable']/tbody/tr");

                        foreach (var node in nodes)
                        {
                            try
                            {
                                var benchmark = new Benchmark(node, passMarks, Utils.BenchTypes.PassMark);

                                sw.WriteLine(benchmark.Print());

                                var matchingCpus = mcCpus.Where(c => (c.Series + c.Model).Equals((benchmark.Series + benchmark.Model)));
                                //var cpu = matchingCpus.FirstOrDefault();

                                //if (cpu == null) continue;

                                foreach (var cpu in matchingCpus)
                                {
                                    if (cpu.PassMark == null)
                                    {
                                        cpu.PassMark = benchmark;

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
                    #endregion

                    #region futuremark
                    page = "Best Processors June - 2016.htm";

                    path = @"futuremarks.txt";

                    File.WriteAllText(path, string.Empty);

                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        htmlDoc.Load(page);
                        nodes = htmlDoc.DocumentNode.SelectNodes("//descendant::table[@id='productTable']/tbody/tr");

                        foreach (var node in nodes)
                        {
                            try
                            {
                                var benchmark = new Benchmark(node, futureMarks, Utils.BenchTypes.FutureMark);

                                sw.WriteLine(benchmark.Print());

                                var matchingCpus = mcCpus.Where(c => (c.Series + c.Model).Equals((benchmark.Series + benchmark.Model)));
                                //var cpu = matchingCpus.FirstOrDefault();

                                //if (cpu == null) continue;

                                foreach (var cpu in matchingCpus)
                                {
                                    if (cpu.FutureMark == null)
                                    {
                                        cpu.FutureMark = benchmark;

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
                        sheet.Cells["H1"].Value = "Pts/$";

                        var row = 2;

                        foreach (var cpu in mcCpus)
                        {
                            sheet.Cells[row, 1].Value = cpu.Brand;
                            sheet.Cells[row, 2].Value = cpu.Name;
                            sheet.Cells[row, 3].Value = cpu.Price;
                            sheet.Cells[row, 4].Value = cpu.Discount;
                            sheet.Cells[row, 5].Formula = "=MAX(ROUND(C" + row + ", 1)-D" + row + ",1e-304)";
                            sheet.Cells[row, 6].Value = cpu.PassMark.Value;
                            if (cpu.FutureMark != null) sheet.Cells[row, 7].Value = cpu.FutureMark.Value;
                            sheet.Cells[row, 8].Formula = "=ROUND(C" + row + "/F" + row + ",1)";

                            ++row;
                        }

                        for (var col = 1; col < 8; ++col)
                            sheet.Column(col).AutoFit();

                        package.Save();
                    }
                    #endregion

                    break;
            }
        }
    }
}
