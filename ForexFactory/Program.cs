using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ForexFactory
{
    internal class Program
    {
        private static readonly Random Random = new Random();
        private const string Path = @"F:\ff.csv";

        public Dictionary<string, string> Metadata { get; set; }

        private static void Main(string[] args)
        {
            var ids = ScrapeIds(LoadHtmlFromFile(@"F:\FF.htm")).ToArray();
            var events = ScrapeEvents(LoadHtmlFromFile(@"F:\FF.htm")).ToArray();

            var parser = new HtmlDocument();

            for (var i = 0; i < 2; i++)
            {
                Console.WriteLine("Item {0} out of {1}", i + 1, ids.Length + 1);
                parser.LoadHtml(LoadHtmlFromFf(ids[i]));
                var html = parser.DocumentNode.InnerText;
                var metadata = ScrapeMetadata(html);
                metadata.Add("EventId", ids[i]);
                metadata.Add("Event", events[i]);
                SaveToCsv(metadata);

                var random = Random.Next(10, 40);
                Console.WriteLine("Delaying for {0} seconds from {1}", random, DateTime.Now);
                Thread.Sleep(new TimeSpan(0, 0, 0, random));
            }

            Console.ReadLine();
        }

        private static void SaveToCsv(Dictionary<string, string> lists)
        {
            Console.WriteLine("Saving to csv...");
            // create header for first time
            if (!File.Exists(Path))
            {
                File.WriteAllText(Path, "sep=;\n");
                File.AppendAllText(Path, String.Join(";", lists.Keys));
            }

            string csv = String.Join(";", lists.Values);
            File.AppendAllText(Path, "\n");
            File.AppendAllText(Path, csv);

            Console.WriteLine("Done saving.");
        }

        private static Dictionary<string, string> ScrapeMetadata(string content)
        {
            var source = ReadAllLines(content);
            var items = source.Where(s => !String.IsNullOrWhiteSpace(s)).Select(s => s.Replace("\t", "")).ToList();
            var metadata = new Dictionary<string, string>();
            var itemsToSearch = new[]
                {
                    "Source",
                    "Measures",
                    "Usual Effect", //Usual effect
                    "Frequency",
                    "Next Release",
                    "FF Notes",
                    "Why TradersCare", //Why Traders Care
                    "Derived Via",
                    "Acro Expand",
                    "Also Called"
                };

            foreach (var search in itemsToSearch)
            {
                if (items.Contains(search))
                {
                    var index = items.FindIndex(s => s == search);
                    metadata.Add(search, items[index + 1].Trim().TrimEnd(';'));
                }
                else
                {
                    metadata.Add(search, "null");
                }
            }

            return metadata;
        }

        private static string LoadHtmlFromFf(string eventId)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://www.forexfactory.com/") };

            httpClient.DefaultRequestHeaders.Add("User-Agent",
                                                 "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31");
            httpClient.DefaultRequestHeaders.Add("Host", "www.forexfactory.com");

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("do", "ajax"),
                    new KeyValuePair<string, string>("contentType", "Content"),
                    new KeyValuePair<string, string>("flex", "calendar_main"),
                    new KeyValuePair<string, string>("details", eventId)
                });

            Console.WriteLine("Requesting  {0}", eventId);
            var response = httpClient.PostAsync("/flex.php", content).Result;

            if (response.StatusCode != HttpStatusCode.OK)
                Console.WriteLine("Error downloading  {0}", eventId);
            else
                Console.WriteLine("Done downloading {0}", eventId);

            return response.Content.ReadAsStringAsync().Result;
        }

        private static IEnumerable<string> ReadAllLines(string content)
        {
            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static IEnumerable<string> ScrapeIds(string html)
        {
            var parser = new HtmlDocument();
            parser.LoadHtml(html);

            var datas = parser.DocumentNode.SelectNodes("//tr[@data-eventid]");
            var ids = datas.Select(data => data.GetAttributeValue("data-eventid", null).Trim());

            return ids;
        }

        private static IEnumerable<string> ScrapeEvents(string html)
        {
            var parser = new HtmlDocument();
            parser.LoadHtml(html);

            var datas = parser.DocumentNode.SelectNodes("//td[@class='event']");
            var events = datas.Select(data => data.InnerText.Trim());

            return events;
        }

        private static string LoadHtmlFromFile(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        
    }
}