using System.IO.Pipelines;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using ProgressOnderwijsUtils.Tests;

namespace ProgressOnderwijsUtilsBenchmarks;

[MedianColumn]
[MemoryDiagnoser]
// ReSharper disable once ClassCanBeSealed.Global
public class RandomHelperBenchmark
{
    public static IEnumerable<(string, RandomHelper)> RandomHelpers
        => new[] { ("secure", RandomHelper.Secure), ("pseudorandom", RandomHelper.Insecure(0)) };

    [ParamsSource(nameof(RandomHelpers))]
    public (string, RandomHelper) A
    {
        set => (_, randomHelper) = value;
    }

    RandomHelper randomHelper = RandomHelper.Secure;

    [Benchmark]
    public void Int32()
        => randomHelper.GetInt32();

    [Benchmark]
    public void UInt32()
        => randomHelper.GetUInt32();

    [Benchmark]
    public void Int64()
        => randomHelper.GetInt64();

    [Benchmark]
    public void UInt64()
        => randomHelper.GetUInt64();

    [Benchmark]
    public void GetNonNegativeInt32()
        => randomHelper.GetNonNegativeInt32();

    [Benchmark]
    public void GetStringOfUriPrintableCharacters_10()
        => randomHelper.GetStringOfUriPrintableCharacters(10);

    [Benchmark]
    public void GetBytes_16()
        => randomHelper.GetBytes(16);

    [Benchmark]
    public void GetStringOfLatinUpperOrLower()
        => randomHelper.GetStringOfLatinUpperOrLower(10);
}
