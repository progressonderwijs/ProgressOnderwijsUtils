using System.IO.Pipelines;
using ProgressOnderwijsUtils.Tests;

namespace ProgressOnderwijsUtilsBenchmarks;

[MemoryDiagnoser]
[Config(typeof(Config))]
// ReSharper disable once ClassCanBeSealed.Global
public class HtmlFragmentBenchmark
{
    static readonly HtmlFragment htmlFragment = WikiPageHtml5.MakeHtml();

    sealed class Config : ManualConfig
    {
        public Config()
        {
            //_ = AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(true).WithId("ServerClean"));
            _ = AddJob(
                Job.InProcess.WithGcServer(true).WithGcForce(false).WithId("Server")
                    .WithUnrollFactor(16)
                    .WithWarmupCount(1)
                    .WithLaunchCount(1)
                    .WithInvocationCount(256)
                    .WithIterationCount(100)
            );
            //_ = AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(true).WithId("WorkstationClean"));
            //_ = AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(false).WithId("Workstation"));
        }
    }

    //*

    //static readonly byte[] htmlUtf8 = Encoding.UTF8.GetBytes(htmlString);
    //static readonly IHtmlDocument angleSharpDocument = new HtmlParser().ParseDocument(htmlString);

    [Benchmark]
    public void WriteToString()
        => htmlFragment.ToStringWithDoctype();

    [Benchmark]
    public void WriteToStream()
    {
        MemoryStream ms = new();
        htmlFragment.SaveHtmlFragmentToStream(ms, StringUtils.Utf8WithoutBom);
    }

    [Benchmark]
    public void WriteToPipe()
    {
        var pipe = new Pipe();
        htmlFragment.SaveHtmlFragmentToPipe(pipe.Writer);
    }

    /*
    static readonly string htmlString = Utils.F(
        () => {
            var s = htmlFragment.ToStringWithDoctype();
            //Console.WriteLine(s.Length);

            return s;
        }
    )();

    [Benchmark]
    public void JustConvertToUtf8()
        => _ = StringUtils.Utf8WithoutBom.GetBytes(htmlString);

    [Benchmark]
    public void CreateLargeDocument()
        => WikiPageHtml5.MakeHtml();

    [Benchmark]
    public void CreateAndSerializeLargeDocument()
        => WikiPageHtml5.MakeHtml().ToStringWithDoctype();

    [Benchmark]
    public void SerializeLargeDocumentToCSharp()
        => htmlFragment.ToCSharp();

    [Benchmark]
    public void AngleSharpParseFromString()
        => new HtmlParser().ParseDocument(htmlString);

    [Benchmark]
    public void AngleSharpParseFromUtf8()
    {
        using var stream = new MemoryStream(htmlUtf8);
        _ = new HtmlParser().ParseDocument(stream);
    }

    [Benchmark]
    public void AngleSharpToString()
        => angleSharpDocument.DocumentElement.ToHtml();

    /**/
}
