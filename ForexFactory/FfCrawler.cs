using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ForexFactory
{
    public class FfCrawler
    {
        public List<EconData> EconDatas { get; set; }

        public FfCrawler()
        {
            EconDatas = new List<EconData>();
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