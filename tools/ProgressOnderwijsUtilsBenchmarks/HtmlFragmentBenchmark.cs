using System.IO;
using System.Text;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Html;
using ProgressOnderwijsUtils.Tests;

namespace ProgressOnderwijsUtilsBenchmarks
{
    [MemoryDiagnoser]
    [Config(typeof(Config))]
    public class HtmlFragmentBenchmark
    {
        static readonly HtmlFragment htmlFragment = WikiPageHtml5.MakeHtml();

        readonly MemoryStream ms = new MemoryStream();

        [Benchmark]
        public void SerializeLargeDocument()
        {
            htmlFragment.SerializeToString();
        }

        [Benchmark]
        public void WriteToStream()
        {
            ms.SetLength(0);
            htmlFragment.SaveHtmlFragmentToStream(ms, Encoding.UTF8);
        }

        class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.MediumRun.WithGcServer(true).WithGcForce(true).WithId("ServerForce"));
                Add(Job.MediumRun.WithGcServer(true).WithGcForce(false).WithId("Server"));
                Add(Job.MediumRun.WithGcServer(false).WithGcForce(true).WithId("Workstation"));
                Add(Job.MediumRun.WithGcServer(false).WithGcForce(false).WithId("WorkstationForce"));
            }
        }

        /*
        static readonly string htmlString = Utils.F(() => {
            var s = htmlFragment.SerializeToString();
            //Console.WriteLine(s.Length);

            return s;
        })();

        static readonly byte[] htmlUtf8 = Encoding.UTF8.GetBytes(htmlString);
        static readonly IHtmlDocument angleSharpDocument = new HtmlParser().Parse(htmlString);

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
