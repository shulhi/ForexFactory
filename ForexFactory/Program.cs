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
        private const int StartEventId = 40111;
        private const int EndEventId = 40115;

        private static void Main(string[] args)
        {
            var ids = new[]
                {
                    45895, 48235, 48263, 48249, 48148, 49187, 44711, 48308, 44247, 44731, 44594, 45194, 47374, 46053,
                    46089, 46077, 46065, 45935, 46101, 49188, 49189, 49190, 49191, 45278, 44316, 44959, 44960, 46961,
                    45971, 46186, 46222, 49186, 47581, 49196, 45842, 49202, 47782, 47784, 46614, 49192, 49193, 45604,
                    44190, 44315, 44788, 44920, 47155, 47160, 47164, 47167, 47171, 47175, 47091, 46243, 46248, 46149,
                    49194, 45500, 46774, 46602, 47735, 46282, 45552, 49199, 44712, 49195, 47747, 47060, 47303, 45959,
                    49200, 46336, 46348, 47292
                };

            var parser = new HtmlDocument();
            var list = new List<Dictionary<string, string>>();

            foreach (var id in ids)
            {
                parser.LoadHtml(LoadHtmlFromFf(id));
                var html = parser.DocumentNode.InnerText;
                var metadata = ConvertMetadata(html);
                list.Add(metadata);

                var random = Random.Next(10, 40);
                Console.WriteLine("Delaying for {0} seconds from {1}", random, DateTime.Now);
                Thread.Sleep(new TimeSpan(0, 0, 0, random));
            }

            MakeItCsv(list);

            Console.ReadLine();
        }

        private static void MakeItCsv(IEnumerable<Dictionary<string, string>> lists)
        {
            Console.WriteLine("Saving to csv...");
            // create header for first time
            if (!File.Exists(Path))
            {
                File.WriteAllText(Path, "sep=;\n");
                File.AppendAllText(Path, String.Join(";", lists.First().Keys));
            }

            foreach (var list in lists)
            {
                string csv = String.Join(";", list.Values);
                File.AppendAllText(Path, "\n");
                File.AppendAllText(Path, csv);
            }
            Console.WriteLine("Done saving.");
        }

        private static Dictionary<string, string> ConvertMetadata(string content)
        {
            var source = ReadAllLines(content);
            var items = source.Where(s => !String.IsNullOrWhiteSpace(s)).Select(s => s.Replace("\t", "")).ToList();
            var metadata = new Dictionary<string, string>();
            var itemsToSearch = new[]
                {
                    "Source",
                    "Measures",
                    "Usual Effect",
                    "Frequency",
                    "Next Release",
                    "FF Notes",
                    "Why Traders Care",
                    "Derived Via",
                    "Acro Expand",
                    "Also Called"
                };

            foreach (var search in itemsToSearch)
            {
                if (items.Contains(search))
                {
                    var index = items.FindIndex(s => s == search);
                    metadata.Add(search, items[index + 1].TrimEnd(';'));
                }
                else
                {
                    metadata.Add(search, "null");
                }
            }

            return metadata;
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

        private static string LoadHtmlFromFf(int eventId)
        {
            var httpClient = new HttpClient {BaseAddress = new Uri("http://www.forexfactory.com/")};

            httpClient.DefaultRequestHeaders.Add("User-Agent",
                                                 "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31");
            httpClient.DefaultRequestHeaders.Add("Host", "www.forexfactory.com");

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("do", "ajax"),
                    new KeyValuePair<string, string>("contentType", "Content"),
                    new KeyValuePair<string, string>("flex", "calendar_main"),
                    new KeyValuePair<string, string>("details", eventId.ToString())
                });

            Console.WriteLine("Requesting  {0}", eventId);
            var response = httpClient.PostAsync("/flex.php", content).Result;

            if (response.StatusCode != HttpStatusCode.OK)
                Console.WriteLine("Error downloading  {0}", eventId);
            else
                Console.WriteLine("Done downloading {0}", eventId);

            return response.Content.ReadAsStringAsync().Result;
        }

        private static string LoadHtmlFromFile()
        {
            return System.IO.File.ReadAllText(@"F:\FF.txt");
        }

        //private static void ToCsv()
        //{
        //    var start = DateTime.Now;
        //    var lines = System.IO.File.ReadAllText(@"F:\FF.txt");
        //    lines = lines.Replace("\t", "");
        //    var sep = Regex.Replace(lines, @"[\r\n?|\n]{2,}", "\n");
        //    var sep2 = sep.Split('\n');
        //    var end = DateTime.Now;
        //    Console.WriteLine("{0}ms", (end - start).TotalMilliseconds);

        //    start = DateTime.Now;
        //    var source = System.IO.File.ReadAllLines(@"F:\FF.txt").ToList();
        //    var newlist = source.Where(s => !String.IsNullOrWhiteSpace(s)).Select(s => s.Replace("\t", "")).ToList();
        //    end = DateTime.Now;
        //    Console.WriteLine("{0}ms", (end - start).TotalMilliseconds);
        //}
    }
}