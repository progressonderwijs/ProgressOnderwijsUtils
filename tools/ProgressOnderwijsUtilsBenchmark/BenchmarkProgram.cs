﻿//#define FOR_PROFILING
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ProgressOnderwijsUtils.Html;
using ProgressOnderwijsUtilsTests;

namespace ProgressOnderwijsUtilsBenchmark
{
    public class BenchmarkProgram
    {
        static readonly HtmlFragment htmlFragment = WikiPageHtml5.MakeHtml();
        static BenchmarkProgram() { }

        static void Main(string[] args)
        {
#if FOR_PROFILING
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (var i = 0; i < 5000; i++) {
                WikiPageHtml5.MakeHtml().SerializeToString();
            }
            System.Console.WriteLine(sw.Elapsed.TotalSeconds);
#else
            BenchmarkRunner.Run<BenchmarkProgram>();
#endif
        }

        [Benchmark]
        public void CreateLargeDocument()
        {
            WikiPageHtml5.MakeHtml();
        }

        [Benchmark]
        public void CreateAndSerializeLargeDocument()
        {
            WikiPageHtml5.MakeHtml().SerializeToString();
        }

        [Benchmark]
        public void SerializeLargeDocument()
        {
            htmlFragment.SerializeToString();
        }

        /*
        static readonly string htmlString = htmlFragment.SerializeToString();
        static readonly byte[] htmlUtf8 = Encoding.UTF8.GetBytes(htmlString);
        static readonly IHtmlDocument angleSharpDocument = new HtmlParser().Parse(htmlString);

        [Benchmark]
        public void SerializeLargeDocumentToCSharp()
        {
            htmlFragment.ToCSharp();
        }

        [Benchmark]
        public void JustConvertToUtf8()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
                writer.Write(htmlString);
        }

        [Benchmark]
        public void JustSerializeToUtf8LargeDocument()
        {
            using (var stream = new MemoryStream())
                htmlFragment.SaveHtmlFragmentToStream(stream, Encoding.UTF8);
        }

        [Benchmark]
        public void AngleSharpParseFromString()
        {
            new HtmlParser().Parse(htmlString);
        }

        [Benchmark]
        public void AngleSharpParseFromUtf8()
        {
            using (var stream = new MemoryStream(htmlUtf8))
                new HtmlParser().Parse(stream);
        }

        [Benchmark]
        public void AngleSharpToString()
        {
            angleSharpDocument.DocumentElement.ToHtml();
        }
        /**/
    }
}
