using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.ComponentModel;

namespace cpuScraper
{
    class Printable
    {
        public void Print()
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                Console.WriteLine("{0}\t{1}", name, value);
            }
            Console.WriteLine("");
        }
    }

    class Cpu : Printable
    {
        public uint MCId { get; set; }

        public decimal Price { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public string Name { get; set; }

        public byte Discount { get; set; }

        public Cpu(HtmlNode node)
        {
            //Console.WriteLine(node.OuterHtml);
            var anchor = node.SelectSingleNode("descendant::a[starts-with(@id,'hypProductH2_')]");
            MCId = Convert.ToUInt32(anchor.Attributes.Where(a => a.Name.Equals("data-id")).First().Value);
            Name = anchor.Attributes.Where(a => a.Name.Equals("data-name")).First().Value;
            Price = Convert.ToDecimal(anchor.Attributes.Where(a => a.Name.Equals("data-price")).First().Value);
            Brand = anchor.Attributes.Where(a => a.Name.Equals("data-brand")).First().Value;

            var modelRegex = new Regex(@"((G)|(i[357]( |-))|(E[35]( |-)?)|(FX )|(A(6|10)[ |-])|(Opteron )|(Athlon )|(Sempron ))?(\d{4}[^vV]?)( ?[vV]\d)?", RegexOptions.Compiled);
            MatchCollection matches = modelRegex.Matches(Name);

            Model = "";

            if (matches.Count > 0)
            {
                for (var match = 0; match < matches.Count; ++match)
                    for (var group = 0; group < matches[match].Length; ++group)
                        if (matches[match].Groups[group].Value.Length > Model.Length)
                            Model = matches[match].Groups[group].Value;
            }
            else
            {
                modelRegex = new Regex(@"Pentium 4", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                matches = modelRegex.Matches(Name);
                Model = matches[0].Groups[0].Value;
            }

            var rx = new Regex(@"\d{2}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var discountNode = node.SelectNodes("descendant::div[@class='highlight clear']").First();

             matches = rx.Matches(discountNode.InnerText);

            if (matches.Count > 0)
                Discount = Convert.ToByte(matches[0].Groups[0].Value);

            Print();
        }
    }

    class Benchmark : Printable
    {

        public string Brand { get; set; }

        public string Model { get; set; }

        public string Name { get; set; }

        public ushort Value { get; set; }

        public Benchmark(HtmlNode node)
        {
            //Console.WriteLine(node.OuterHtml);
            var nameAnchor = node.SelectSingleNode("descendant::*[starts-with(@id,'rk')]/a");
            if (nameAnchor == null) return;

            Name = nameAnchor.InnerText;

            var regex = new Regex(@"(Intel|AMD)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(nameAnchor.InnerText);

            //if (matches.Count > 0)
            Brand = matches[0].Groups[0].Value;

            regex = new Regex(@"((G)|(i[357]-)|(E[35]-)|([FR]X[ |-])|(Phenom II X[46] )|(Opteron )|(Athlon X4 )|(A(6|8|10|12)(PRO )?[ |-]))?(B\d{2}|[A-Z]?\d{3,4}X?)[a-z]{0,2}( v[1-5])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            matches = regex.Matches(nameAnchor.InnerText);

            //if (matches.Count > 0)
            Model = matches[0].Groups[0].Value;

            nameAnchor = node.SelectSingleNode("descendant::*[starts-with(@id,'rt')]/div");

            Value = Convert.ToUInt16(nameAnchor.InnerText.Trim().Replace(",", ""));

            Print();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var htmlDoc = new HtmlDocument();
            var pageOfMCCpus = new List<Cpu>();
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
                                pageOfMCCpus = htmlDoc.DocumentNode.SelectNodes("//div[@class='detail_wrapper']").ToList().ConvertAll(n => new Cpu(n));

                                pageOfMCCpus.AddRange(pageOfMCCpus);

                            }
                        } // iterate pages

                    } // httpclient
                    break;
                case "file":
                    var pages = new List<string> { "AMD _ Intel _ Processors_CPUs _ Computer Parts _ Micro Center.html", "AMD _ Intel _ Processors_CPUs _ Computer Parts _ Micro Center2.html", "AMD _ Intel _ Processors_CPUs _ Computer Parts _ Micro Center3.html" };

                    HtmlNodeCollection nodes = null;

                    foreach (var page in pages)
                    {
                        htmlDoc.Load(page);

                        nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='detail_wrapper']");

                        foreach (var node in nodes)
                        {
                            try
                            {
                                pageOfMCCpus.Add(new Cpu(node));
                            }
                            catch (Exception) { }
                        }
                    }

                    htmlDoc.Load("PassMark Intel vs AMD CPU Benchmarks - High End.html");
                    nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id='mark']/table/tbody/tr");

                    foreach (var node in nodes)
                    {
                        try
                        {
                            benches.Add(new Benchmark(node));
                        }
                        catch (Exception) { }
                    }

                    break;
            }
        }
    }
}
