using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static Regex MCSeriesRegex = new Regex(@"((i[357])|(FX )|(A(6|10)[ |-])|(Opteron )|(Athlon )|(Sempron )|(Pentium 4)|(Celeron ))?", RegexOptions.Compiled);

        public static Regex MCModelRegex = new Regex(@"(G?(E[35]( |-)?)?\d{4}[^vV]?)( ?[vV]\d)?", RegexOptions.Compiled);

        public static Regex DiscountRegex = new Regex(@"\d{2}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex CpuBenchBrandRegex = new Regex(@"(Intel|AMD|HP)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex CpuBenchSeriesRegex = new Regex(@"i[357]|[FR]X[ |-]|Phenom II X?\d\d? |Opteron |(Athlon (X4|II X3 |64 )?)|(A(6|8|10|12)(PRO )?[ |-])|Turion (II Neo|X2 Ultra Dual-Core Mobile)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex CpuBenchModelRegex = new Regex(@"(\w\d{2}\w?|[A-Z]?(E[35]-)?\d{3,4}\w?|\d\w\d\d|TWKR|Hexa-Core|[a-z]{2}-\d\d)[a-z]{0,2}( v[1-5])?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
    }

    class Printable
    {

        public void PrintConsole()
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                Console.WriteLine("{0}\t{1}", name, value);
            }
            //text += string.Format("\n");
        }

        public string Print()
        {
            var text = "";
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                text += string.Format("{0}\t{1}\r\n", name, value);
            }
            //text += string.Format("\n");
            return text;
        }
    }

    class Cpu : Printable
    {
        public uint MCId { get; set; }

        public decimal Price { get; set; }

        public string Brand { get; set; }

        public string Series { get; set; }

        public string Model { get; set; }

        public string Name { get; set; }

        public byte Discount { get; set; }

        public Benchmark Benchmark { get; set; }

        public Cpu(HtmlNode node)
        {
            //Console.WriteLine(node.OuterHtml);
            var anchor = node.SelectSingleNode("descendant::a[starts-with(@id,'hypProductH2_')]");
            MCId = Convert.ToUInt32(anchor.Attributes.Where(a => a.Name.Equals("data-id")).First().Value);
            Name = anchor.Attributes.Where(a => a.Name.Equals("data-name")).First().Value;
            Price = Convert.ToDecimal(anchor.Attributes.Where(a => a.Name.Equals("data-price")).First().Value);
            Brand = anchor.Attributes.Where(a => a.Name.Equals("data-brand")).First().Value;

            MatchCollection matches = Utils.MCSeriesRegex.Matches(Name);

            Series = "";

            for (var match = 0; match < matches.Count; ++match)
                for (var group = 0; group < matches[match].Length; ++group)
                    if (matches[match].Groups[group].Value.Length > Series.Length)
                        Series = matches[match].Groups[group].Value.Replace(" ", "").Replace("-", "").ToUpper();

            matches = Utils.MCModelRegex.Matches(Name);

            Model = "";

            //if (matches.Count > 0)
            //{
            for (var match = 0; match < matches.Count; ++match)
                for (var group = 0; group < matches[match].Length; ++group)
                    if (matches[match].Groups[group].Value.Length > Model.Length)
                        Model = matches[match].Groups[group].Value.Replace(" ", "").Replace("-", "").ToUpper();
            //}
            //else
            //{
            //    matches = Utils.PentiumRegex.Matches(Name);
            //Model = matches[0].Groups[0].Value.Replace(" ", "").Replace("-", "").ToUpper();
            //}

            var discountNode = node.SelectNodes("descendant::div[@class='highlight clear']").First();

            matches = Utils.DiscountRegex.Matches(discountNode.InnerText);

            if (matches.Count > 0)
                Discount = Convert.ToByte(matches[0].Groups[0].Value);

            if (string.IsNullOrWhiteSpace(Series) && string.IsNullOrWhiteSpace(Model))
                throw new ArgumentException();

            //Print();
        }
    }

    class Benchmark : Printable
    {

        public string Brand { get; set; }

        public string Series { get; set; }

        public string Model { get; set; }

        public string Name { get; set; }

        public ushort Value { get; set; }

        public Benchmark(HtmlNode node)
        {
            //Console.WriteLine(node.OuterHtml);
            var nameAnchor = node.SelectSingleNode("descendant::*[starts-with(@id,'rk')]/a");

            Name = nameAnchor.InnerText;

            MatchCollection matches = Utils.CpuBenchBrandRegex.Matches(nameAnchor.InnerText);

            //if (matches.Count > 0)
            Brand = matches[0].Groups[0].Value;

            matches = Utils.CpuBenchSeriesRegex.Matches(nameAnchor.InnerText);

            //if (matches.Count > 0)
            Series = matches[0].Groups[0].Value.Replace(" ", "").Replace("-", "").ToUpper();

            matches = Utils.CpuBenchModelRegex.Matches(nameAnchor.InnerText);

            if (matches.Count > 0)
                Model = matches[0].Groups[0].Value.Replace(" ", "").Replace("-", "").ToUpper();

            nameAnchor = node.SelectSingleNode("descendant::*[starts-with(@id,'rt')]/div");

            Value = Convert.ToUInt16(nameAnchor.InnerText.Trim().Replace(",", ""));

            if (string.IsNullOrWhiteSpace(Series) && string.IsNullOrWhiteSpace(Model))
            {
                PrintConsole();
                throw new ArgumentNullException();
            }

            //Print();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var htmlDoc = new HtmlDocument();
            var mcCpus = new List<Cpu>();
            var benches = new List<Benchmark>();

            switch (args[0])
            {
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

                        for (var page = 1; page < 4; page++)
                        {
                            var stream = httpClient.GetStreamAsync(string.Format(requestUri, page)).Result;

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
                case "file":
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
                        foreach (var page in pages)
                        {
                            htmlDoc.Load(page);

                            nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='detail_wrapper']");

                            foreach (var node in nodes)
                            {
                                var cpu = new Cpu(node);
                                mcCpus.Add(cpu);
                                sw.WriteLine(cpu.Print());
                            }
                        }
                    }

                    pages = new List<string> {
                        "PassMark Intel vs AMD CPU Benchmarks - High End.html",
                        "PassMark CPU Benchmarks - High Mid Range CPUs.html" ,
                        "PassMark CPU Benchmarks - Low Mid Range CPUs.html" };

                    path = @"benchmarks.txt";

                    File.WriteAllText(path, string.Empty);

                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        foreach (var page in pages)
                        {
                            htmlDoc.Load(page);
                            nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id='mark']/table/tbody/tr");

                            foreach (var node in nodes)
                            {
                                try
                                {
                                    var benchmark = new Benchmark(node);
                                    benches.Add(benchmark);
                                    sw.WriteLine(benchmark.Print());

                                    var matchingCpus = mcCpus.Where(c => c.Model.Equals(benchmark.Model));

                                    //if (matchingCpus.Count() > 1)
                                    //    throw new ArgumentException();

                                    foreach (var cpu in matchingCpus)
                                        cpu.Benchmark = benchmark;

                                }
                                catch (Exception e)
                                {
                                    Console.BackgroundColor = ConsoleColor.DarkRed;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(node.OuterHtml);
                                    Console.WriteLine(e.ToString()+"\n");
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }
                        }
                    }

                    break;
            }
        }
    }
}
