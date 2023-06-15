using System.IO.Pipelines;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
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
            _ = AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(false).WithId("Server"));
            //_ = AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(true).WithId("WorkstationClean"));
            //_ = AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(false).WithId("Workstation"));
        }
    }

    //*
    static readonly string htmlString = Utils.F(
        () => {
            var s = htmlFragment.ToStringWithDoctype();
            //Console.WriteLine(s.Length);

            return s;
        }
    )();

    //static readonly byte[] htmlUtf8 = Encoding.UTF8.GetBytes(htmlString);
    //static readonly IHtmlDocument angleSharpDocument = new HtmlParser().ParseDocument(htmlString);

    [Benchmark]
    public void SerializeLargeDocument()
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

    [Benchmark]
    public void WriteToStreamViaWriter()
    {
        MemoryStream ms = new();
        htmlFragment.SaveHtmlFragmentToStreamViaWriter(ms, StringUtils.Utf8WithoutBom);
    }

    [Benchmark]
    public void JustConvertToUtf8()
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, StringUtils.Utf8WithoutBom);
        writer.Write(htmlString);
    }

    [Benchmark]
    public void JustSerializeToUtf8LargeDocument()
    {
        using var stream = new MemoryStream();
        htmlFragment.SaveHtmlFragmentToStream(stream, StringUtils.Utf8WithoutBom);
    }

    [Benchmark]
    public void CreateLargeDocument()
        => WikiPageHtml5.MakeHtml();

    /*
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
