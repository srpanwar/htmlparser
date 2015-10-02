using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using HTMLParser;

namespace TestParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            TimeSpan ts = default(TimeSpan);
            string elapsedTime = String.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://news.bing.com/");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                String html = reader.ReadToEnd();

                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                Console.WriteLine(elapsedTime, "RunTime");
                int count = 0;
                Parser parser = null;
                while (count < 10)
                {
                    parser = new Parser();
                    stopWatch = new Stopwatch();
                    stopWatch.Start();
                    parser.Parse(html);
                    stopWatch.Stop();
                    ts = stopWatch.Elapsed;
                    elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.TotalMilliseconds);
                    Console.WriteLine(elapsedTime, "RunTime");
                    count++;
                }
                //Printing the parsed dom
                StreamWriter writer = new StreamWriter("page.html");
                writer.WriteLine(parser.HDocument.Header);
                PrintDomTree(writer, parser.HDocument.Root);
                writer.Flush(); writer.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="hTMLElement"></param>
        /// <param name="p"></param>
        private static void PrintDomTree(StreamWriter writer, HTMLElement parent)
        {
            if (parent == null)
                return;

            foreach (HTMLElement child in parent.ChildNodes)
            {
                if (child.TagType != ElementType.text)
                {
                    if (!child.HasNoStartNode)
                    {
                        if (child.CanHaveChildren)
                            writer.Write(child.Text.Replace("/>", ">"));
                        else
                            writer.Write(child.Text);
                    }
                }
                else
                    writer.Write(child.Text);

                PrintDomTree(writer, child);

                if (!child.HasNoStartNode)
                {
                    if (child.CanHaveChildren)
                        writer.Write("</" + child.Tag + ">");
                }
            }
        }
    }
}
