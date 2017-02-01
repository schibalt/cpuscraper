using HtmlAgilityPack;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;

namespace gpuScraper
{
    static class Utils
    {
        public static Regex MCSeriesRegex = new Regex(@"Geforce|Quadro|Radeon|FirePro", RegexOptions.IgnoreCase);

        public static Regex MCModelRegex = new Regex(@"GTX? \d?\d\d0( Ti(tan( X)?)?)?", RegexOptions.Compiled);
        public static Regex MCModelRegex2 = new Regex(@"(M|K|W)\d000", RegexOptions.Compiled);
        public static Regex MCModelRegex3 = new Regex(@"R(X|9) \d\d0X?2?", RegexOptions.Compiled);
        public static Regex MCModelRegex4 = new Regex(@"\d?\d\d0( Ti(tan( X)?)?)?", RegexOptions.Compiled);
        //public static Regex MCModelRegex2 = new Regex(@"[A-Z]?\d{4}([A-Za-z]){0,2}", RegexOptions.Compiled);
        //public static Regex MCModelRegex3 = new Regex(@"\d\.\dGHz", RegexOptions.Compiled);

        public static Regex DiscountRegex = new Regex(@"\d{2}", RegexOptions.Compiled);

        public static Regex GigsMemRegex = new Regex(@"\dgb", RegexOptions.IgnoreCase);

        public static Regex GpuBenchBrandRegex = new Regex(@"ASUS|Zotac|Gigabyte|EVGA|MSI|nvidia", RegexOptions.IgnoreCase);

        public static Regex GpuBenchSeriesRegex = new Regex(@"Geforce|Radeon( (pro|hd|R(X|9|7)))?|quadro|grid", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex GpuBenchModelRegex = new Regex(@"GTX? \d?\d\d0( Ti)?", RegexOptions.IgnoreCase);
        public static Regex GpuBenchModelRegex2 = new Regex(@"\w?\d\d\d(\d|\w)?2?", RegexOptions.Compiled);
        public static Regex GpuBenchModelRegex3 = new Regex(@"K\d(\d\d\w?)?", RegexOptions.Compiled);
        public static Regex GpuBenchModelRegex4 = new Regex(@"(fx )?\w?\d\d\d0", RegexOptions.Compiled);
        public static Regex GpuBenchModelRegex5 = new Regex(@"Titan( X)?|duo|fury|device|gpu|cx|nano", RegexOptions.IgnoreCase);
        public static Regex GpuBenchModelRegex6 = new Regex(@"duo?", RegexOptions.IgnoreCase);

        public enum BenchTypes
        {
            PassMark,
            //FutureMark, PassMarkRanged
        };

        //public static ushort PassMarkCeiling = 25000;
        //public static ushort FutureMarkCeiling = 12000;

        public static string[] NameNodeXPath = new string[]
        {
            "td[1]/a[1]",
            "td[3]/a[1]",

            "td[1]"
        };

        public static string passmarkRangedPageNameNodePath = "descendant::*[starts-with(@id,'rk')]/a";

        public static string[] ValueNodeXPath = new string[]
        {
            "td[2]",
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

    }


    class Gpu
    {
        public uint MCId { get; set; }

        public decimal Price { get; set; }

        public decimal RebatePrice { get; set; }

        public string Brand { get; set; }

        public string Series { get; set; }

        public string Model { get; set; }

        public byte GigsMem { get; set; }

        public string Name { get; set; }

        public byte Discount { get; set; }

        public Benchmark PassMark;

        public Benchmark FutureMark;

        public Benchmark UserBenchmark;

        public bool FlaggedForPurchase { get; set; }
        
        public Gpu(HtmlNode node)
        {
            var anchor = node.SelectSingleNode("descendant::a[starts-with(@id,'hypProductH2_')]");
            MCId = Convert.ToUInt32(anchor.Attributes.Where(a => a.Name.Equals("data-id")).First().Value);
            Name = anchor.Attributes.Where(a => a.Name.Equals("data-name")).First().Value;
            var priceNode = anchor.Attributes.Where(a => a.Name.Equals("data-price")).First();
            if (string.IsNullOrWhiteSpace(priceNode.Value))
                throw new InvalidOperationException("price node is empty");
            Price = Convert.ToDecimal(priceNode.Value);
            Brand = anchor.Attributes.Where(a => a.Name.Equals("data-brand")).First().Value;

            var matches = Utils.MCSeriesRegex.Matches(Name);

            Series = "";

            for (var matchIdx = 0; matchIdx < matches.Count; ++matchIdx)
                if (matches[matchIdx].Value.Length > Series.Length)
                    Series = matches[matchIdx].Value.Replace(" ", "").Replace("-", "").ToUpper();
            
            Model = "";

            var modelRegexes = new List<Regex>
            {
                Utils.MCModelRegex,
                Utils.MCModelRegex2,
                Utils.MCModelRegex3,
                Utils.MCModelRegex4
            };

            var hasMatch = false;

            for (var regex = 0; regex < modelRegexes.Count && !hasMatch; ++regex)
            {
                matches = modelRegexes[regex].Matches(Name);
                hasMatch = matches.Count > 0;

                if (hasMatch)
                    Model = matches[0].Value.Replace(" ", "").Replace("-", "").ToUpper();
            }

            matches = Utils.GigsMemRegex.Matches(Name);
            hasMatch = matches.Count > 0;

            if (hasMatch)
                GigsMem = Convert.ToByte(new string(matches[0].Groups[0].Value.Where(c => char.IsDigit(c) || c == '.').ToArray()));

            var discountNode = node.SelectNodes("descendant::div[@class='highlight clear']").First();

            matches = Utils.DiscountRegex.Matches(discountNode.InnerText);

            if (matches.Count > 0)
                Discount = Convert.ToByte(matches[0].Groups[0].Value);

            if (string.IsNullOrWhiteSpace(Series) && string.IsNullOrWhiteSpace(Model))
                throw new ArgumentException();
            
        }

        public void SetRebatePrice(HtmlNode node)
        {
            var priceNode = node.SelectSingleNode("descendant::span[@class='price']");
            var digitArray = priceNode.InnerHtml.Where(c => char.IsDigit(c) || c == '.').ToArray();

            if (digitArray.Length < 1)
                return;
            Price = Convert.ToDecimal(new string(digitArray));
        }
    }

    class Benchmark
    {

        public string Brand { get; set; }

        public string Series { get; set; }

        public string Model { get; set; }

        public byte GigsMem { get; set; }

        public string Name { get; set; }

        public decimal Value { get; set; }

        public Benchmark(HtmlNode node, List<Benchmark> passMarks, int type)
        {
            var nameAnchor = node.SelectSingleNode(Utils.NameNodeXPath[type]);

            if (nameAnchor != null)
                Name = nameAnchor.InnerText;
            else
                return;

            MatchCollection matches = Utils.GpuBenchBrandRegex.Matches(nameAnchor.InnerText);

            matches = Utils.GigsMemRegex.Matches(Name);
            var hasMatch = matches.Count > 0;

            if (hasMatch)
                GigsMem = Convert.ToByte(new string(matches[0].Groups[0].Value.Where(c => char.IsDigit(c) || c == '.').ToArray()));

            matches = Utils.GpuBenchSeriesRegex.Matches(nameAnchor.InnerText);

            if (matches.Count > 0)
                Series = matches[0].Value.Replace(" ", "").Replace("-", "").ToUpper();

            var modelRegexes = new List<Regex>
            {
                Utils.GpuBenchModelRegex,
                Utils.GpuBenchModelRegex2,
                Utils.GpuBenchModelRegex3,
                Utils.GpuBenchModelRegex4,
                Utils.GpuBenchModelRegex5,
                Utils.GpuBenchModelRegex6
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

            //if (!match)
            //{
            //    matches = Utils.CpuBenchModelRegex6.Matches(nameAnchor.InnerText);
            //    match = matches.Count > 0;

            //    if (match)
            //        Model = (matches[0].Groups[1].Value + matches[0].Groups[2].Value).Replace(" ", "").Replace("-", "").ToUpper();
            //}

            nameAnchor = node.SelectSingleNode(Utils.ValueNodeXPath[type]);

            Value = ushort.Parse(new string(nameAnchor.InnerText.Where(c => char.IsDigit(c) || c == '.').ToArray()));

            if (
                //string.IsNullOrWhiteSpace(Series) && 
                string.IsNullOrWhiteSpace(Model))
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
            var mcGpus = new List<Gpu>();
            var passMarks = new List<Benchmark>();
            var futureMarks = new List<Benchmark>();

            var pages = new List<string>
            {
                        "Video Cards _ Video Cards, TV Tuners _ Computer Parts _ Micro Center.html",
                        "Video Cards _ Video Cards, TV Tuners _ Computer Parts _ Micro Center2.html",
                        "Video Cards _ Video Cards, TV Tuners _ Computer Parts _ Micro Center3.html",
                        "Video Cards _ Video Cards, TV Tuners _ Computer Parts _ Micro Center4.html",
                        "Video Cards _ Video Cards, TV Tuners _ Computer Parts _ Micro Center5.html",
                        "Video Cards _ Video Cards, TV Tuners _ Computer Parts _ Micro Center6.html",
                        "Video Cards _ Video Cards, TV Tuners _ Computer Parts _ Micro Center7.html",
                        "Video Cards _ Video Cards, TV Tuners _ Computer Parts _ Micro Center8.html",
            };

            var webpages = new List<string>
            {
                //"http://www.microcenter.com/search/search_results.aspx?N=4294966937&",
                //"http://www.microcenter.com/search/search_results.aspx?N=4294966995&page=2",
                //"http://www.microcenter.com/search/search_results.aspx?N=4294966995&page=3"
            };

            #region read benchmarks

            var paths = new List<string>
                    {
                        "benchmarks.txt",
                        "futuremarks.txt",
                        "geekbench.txt"
                    };

            HtmlNodeCollection nodes = null;
            string path = @"mcgpus.txt";

            File.WriteAllText(path, string.Empty);

            var useWeb = args[0].Equals("web");

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                var cookies = new CookieContainer();
                var uri = new Uri("http://www.microcenter.com");
                cookies.Add(uri, new Cookie("storeSelected", "051"));
                cookies.Add(uri, new Cookie("ipp", "25"));

                for (var microCtrPgNo = 0; microCtrPgNo < pages.Count; microCtrPgNo++)
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
                    var rebateNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='price_wrapper']");

                    foreach (var node in nodes)
                    {
                        try
                        {
                            var gpu = new Gpu(node);
                            gpu.SetRebatePrice(rebateNodes.ElementAt(nodes.IndexOf(node)));
                            mcGpus.Add(gpu);
                            sw.WriteLine(gpu.Print());
                        }
                        catch (InvalidOperationException e)
                        { Console.WriteLine(e); }
                        catch (ArgumentException e)
                        { Console.WriteLine(e); }
                    }
                } // iterate pages
            }

            pages = new List<string>
                    {
                        "PassMark Software - Video Card (GPU) Benchmarks - High End Video Cards.html",
                        "Best Graphics Cards January - 2017.html",
                        //"Processor Benchmarks - Geekbench Browser.htm"
                    };

            webpages = new List<string>
                    {
                        "https://www.cpubenchmark.net/CPU_mega_page.html",
                        //"http://www.futuremark.com/hwc/hwcenter/page-main.php?type=cpu&filters=desktop,mobile,server",
                        //"https://browser.primatelabs.com/processor-benchmarks"
                    };

            var benchContainerXPaths = new List<string>
                    {
                        "//descendant::div[@id='mark']/table/tbody/tr",
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

                            if (benchmark.Model == null)
                                continue;

                            sw.WriteLine(benchmark.Print());

                            var matchingGpus = mcGpus.Where(c => (c.Series + c.Model).Equals((benchmark.Series + benchmark.Model)) && ((c.GigsMem < 1 || benchmark.GigsMem < 1) || (c.GigsMem == benchmark.GigsMem))).ToList();

                            //really doesn't help
                            //var sameMemGpus = matchingGpus.Where(g => benchmark.GigsMem > 0 && benchmark.GigsMem == g.GigsMem).ToList();

                            //if (sameMemGpus.Any())
                            //    matchingGpus.RemoveAll(g => !sameMemGpus.Contains(g));

                            // TODO unflag ones where price isn't lower and bench isn't higher

                            foreach (var gpu in matchingGpus.Where(g => (page == 0 && (g.PassMark == null || g.PassMark.GigsMem < 1)) || (page == 1 && (g.FutureMark == null || g.FutureMark.GigsMem < 1))||page > 1))
                            {
                                switch (page)
                                {
                                    case 0:
                                        gpu.PassMark = benchmark;
                                        break;
                                    case 1:
                                        gpu.FutureMark = benchmark;
                                        break;
                                    case 2:
                                        gpu.PassMark = benchmark;
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

            var sheetFile = new FileInfo(@"gpubench.xlsx");
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
                sheet.Cells["L1"].Value = "Flagged";

                var row = 2;

                mcGpus = mcGpus.ToList();
                //mcCpus = mcCpus.Where(c => c.PassMark.Value > 4000 && Math.Round(c.Price) < 900).ToList();
                mcGpus = mcGpus.Where(c => c.PassMark != null).OrderByDescending(c => c.PassMark.Value).ToList();
                //mcCpus = mcCpus.OrderByDescending(c => c.PassMark.Value / (c.Price - c.Discount)).ToList();

                //var passmark = 0;
                var price = decimal.MaxValue;

                foreach (var gpu in mcGpus)
                    if (gpu.Price - gpu.Discount < price )
                    //if (cpu.PassMark.Value > passmark && !cpu.Skip)
                    {
                        gpu.FlaggedForPurchase = true;
                        price = gpu.Price - gpu.Discount;
                        //passmark = cpu.PassMark.Value;

                        // unflag where price isn't lower and passmark isn't higher.  if price isn't lower passmark ought to be higher.
                        var markedUp=mcGpus.Where(g => g != gpu && g.FlaggedForPurchase && g.Price >= gpu.Price && g.PassMark != null && gpu.PassMark != null && g.PassMark.Value <= gpu.PassMark.Value).ToList();
                        markedUp.ForEach(g=>g.FlaggedForPurchase=false);
                    }
                
                var priceMin = mcGpus.Min(c => c.Price);
                var priceMax = mcGpus.Max(c => c.Price);
                var priceRange = priceMax - priceMin;

                var discountMin = mcGpus.Min(c => c.Discount);
                var discountMax = mcGpus.Max(c => c.Discount);
                var discountRange = discountMax - discountMin;

                var drpMin = mcGpus.Min(c => c.Price - c.Discount);
                var drpMax = mcGpus.Max(c => c.Price - c.Discount);
                var drpRange = drpMax - drpMin;

                var passmarkMin = mcGpus.Min(c => c.PassMark.Value);
                var passmarkMax = mcGpus.Max(c => c.PassMark.Value);
                var passmarkRange = passmarkMax - passmarkMin;

                var futuremarkGpus = mcGpus.Where(c => c.FutureMark != null);
                var futuremarkMin = futuremarkGpus.Min(c => c.FutureMark.Value);
                var futuremarkMax = futuremarkGpus.Max(c => c.FutureMark.Value);
                var futuremarkRange = futuremarkMax - futuremarkMin;

                //var geekbenchGpus = mcGpus.Where(c => c.UserBenchmark != null);
                //var geekbenchMin = geekbenchGpus.Min(c => c.UserBenchmark.Value);
                //var geekbenchMax = geekbenchGpus.Max(c => c.UserBenchmark.Value);
                //var geekbenchRange = geekbenchMax - geekbenchMin;

                var passmarkValueMin = mcGpus.Min(c => c.PassMark.Value / (c.Price - c.Discount));
                var passmarkValueMax = mcGpus.Max(c => c.PassMark.Value / (c.Price - c.Discount));
                var passmarkValueRange = passmarkValueMax - passmarkValueMin;

                var futuremarkValueMin = futuremarkGpus.Min(c => c.FutureMark.Value / (c.Price - c.Discount));
                var futuremarkValueMax = futuremarkGpus.Max(c => c.FutureMark.Value / (c.Price - c.Discount));
                var futuremarkValueRange = futuremarkValueMax - futuremarkValueMin;

                //var geekbenchValueMin = geekbenchGpus.Min(c => c.UserBenchmark.Value / (c.Price - c.Discount));
                //var geekbenchValueMax = geekbenchGpus.Max(c => c.UserBenchmark.Value / (c.Price - c.Discount));
                //var geekbenchValueRange = geekbenchValueMax - geekbenchValueMin;

                foreach (var gpu in mcGpus)
                {
                    sheet.Cells[row, 1].Value = gpu.Brand;

                    sheet.Cells[row, 2].Value = gpu.Name;

                    sheet.Cells[row, 3].Value = gpu.Price;
                    sheet.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    sheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(Gradient(gpu.Price, priceMin, priceRange, false));

                    //sheet.Cells[row, 4].Value = gpu.Discount;
                    //sheet.Cells[row, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    //sheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(Gradient(gpu.Discount, discountMin, discountRange));

                    var discountedPrice = gpu.Price - gpu.Discount;
                    //sheet.Cells[row, 5].Formula = "=MAX(ROUND(C" + row + ", 1)-D" + row + ",1e-304)";
                    sheet.Cells[row, 5].Formula = "=ROUND(C" + row + ", 1)-D" + row;
                    sheet.Cells[row, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    sheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(Gradient(discountedPrice, drpMin, drpRange, false));

                    if (gpu.PassMark != null)
                    {
                        sheet.Cells[row, 6].Value = gpu.PassMark.Value;
                        sheet.Cells[row, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet.Cells[row, 6].Style.Fill.BackgroundColor.SetColor(Gradient(gpu.PassMark.Value, passmarkMin, passmarkRange));
                    }

                    if (gpu.FutureMark != null)
                    {
                        sheet.Cells[row, 7].Value = gpu.FutureMark.Value;
                        sheet.Cells[row, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Gradient(gpu.FutureMark.Value, futuremarkMin, futuremarkRange));
                    }

                    //if (gpu.UserBenchmark != null)
                    //{
                    //    sheet.Cells[row, 8].Value = gpu.UserBenchmark.Value;
                    //    sheet.Cells[row, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    //    sheet.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(Gradient(gpu.UserBenchmark.Value, geekbenchMin, geekbenchRange));
                    //}

                    sheet.Cells[row, 9].Formula = "=ROUND(F" + row + "/E" + row + ",1)";
                    sheet.Cells[row, 9].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    sheet.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(Gradient(gpu.PassMark.Value / discountedPrice, passmarkValueMin, passmarkValueRange));

                    if (gpu.FutureMark != null)
                    {
                        sheet.Cells[row, 10].Formula = "=ROUND(G" + row + "/E" + row + ",1)";
                        sheet.Cells[row, 10].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet.Cells[row, 10].Style.Fill.BackgroundColor.SetColor(Gradient(gpu.FutureMark.Value / discountedPrice, futuremarkValueMin, futuremarkValueRange));
                    }

                    //if (gpu.UserBenchmark != null)
                    //{

                    //    sheet.Cells[row, 11].Formula = "=ROUND(H" + row + "/E" + row + ",1)";
                    //    sheet.Cells[row, 11].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    //    sheet.Cells[row, 11].Style.Fill.BackgroundColor.SetColor(Gradient(gpu.UserBenchmark.Value / discountedPrice, geekbenchValueMin, geekbenchValueRange));
                    //}

                    if (gpu.FlaggedForPurchase)
                        sheet.Cells[row, 12].Value = "✓";
                    ++row;
                }

                for (var col = 1; col < 8; ++col)
                    sheet.Column(col).AutoFit();

                package.Save();
            }
            #endregion
        }

        static Color Gradient(decimal value, decimal rangeMin, decimal range, bool greenLarge = true)
        {

            var coefficient = (value - rangeMin) / range;
            coefficient = Math.Max(coefficient, 0);
            coefficient = Math.Min(coefficient, 1);
            int red, green;

            if (greenLarge) // (0.75, 1.0] green -> lime, 0 red 128->255 green
                if (coefficient > .75m)
                {
                    coefficient = (coefficient - .75m) * 4;

                    red = 0;
                    green = (int)(0xFF - (0x80 * coefficient));
                }
                else if (coefficient > .5m) // (0.5, 0.75] lime -> yellow, 255 green 255->0 red
                {
                    coefficient = (coefficient - .5m) * 4;

                    red = (int)(0xFF * (1 - coefficient));
                    green = 0xFF;
                }
                else // [0.0, 0.5] yellow -> red, red 255 
                {
                    coefficient = 2 * coefficient;

                    red = 0xFF;
                    green = (int)(0xFF * coefficient);
                }
            else if (coefficient < .25m) // [0.0, 0.25)
            {
                coefficient *= 4;

                red = 0;
                green = (int)(0x80 + (0x80 * coefficient));
            }
            else if (coefficient < .5m) // [0.25, 0.5)
            {
                coefficient = (coefficient - .25m) * 4;

                red = (int)(0xFF * coefficient);
                green = 0xFF;
            }
            else // [0.5, 1.0)
            {
                coefficient = 2 * (coefficient - .5m);

                red = 0xFF;
                green = (int)(0xFF * (1 - coefficient));
            }
            return Color.FromArgb(red, green, 0);
        }
    }
}
