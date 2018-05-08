<Query Kind="Program">
  <Reference Relative="..\..\..\LocalOnly\programs\EmnExtensions\bin\Release\EmnExtensions.dll">C:\VCS\LocalOnly\programs\EmnExtensions\bin\Release\EmnExtensions.dll</Reference>
  <Reference Relative="..\src\ProgressOnderwijsUtils\bin\Release\net471\ProgressOnderwijsUtils.dll">C:\VCS\external\ProgressOnderwijsUtils\src\ProgressOnderwijsUtils\bin\Release\net471\ProgressOnderwijsUtils.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Core.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Numerics.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <NuGetReference>CsQuery</NuGetReference>
  <NuGetReference>Dapper</NuGetReference>
  <NuGetReference>ExpressionToCodeLib</NuGetReference>
  <NuGetReference>IncrementalMeanVarianceAccumulator</NuGetReference>
  <NuGetReference>morelinq</NuGetReference>
  <Namespace>CsQuery</Namespace>
  <Namespace>Dapper</Namespace>
  <Namespace>ExpressionToCodeLib</Namespace>
  <Namespace>IncrementalMeanVarianceAccumulator</Namespace>
  <Namespace>MoreLinq</Namespace>
  <Namespace>ProgressOnderwijsUtils</Namespace>
  <Namespace>ProgressOnderwijsUtils.Collections</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

static int threads = Environment.ProcessorCount;

const int extraShift = 21;
const int expRandomMask = 0x3;
const int iters = 1<<(extraShift+(expRandomMask>>1)-1);
const int benchTries = 40;
int ExpDistrib(uint n) => (int)(n >> (int)(Math.Min(7 + extraShift + (n & expRandomMask), 31)));
int GetSize(Random r) => ExpDistrib((uint)r.Next());

/*
struct BigThing {
    byte A,B,C,D,E,F,G,H,I,J,K,L;
    public BigThing(int num) 
    {
        A=B=C=D=E=F=G=H=I=J=K=L=(byte)num;
    }
}
/*/
struct BigThing
{
    int A;
    public BigThing(int num)
    {
        A = (short)num;
    }
}
/**/

void Main()
{
    {
        var r = new Random(37);
        var sizes = Enumerable.Range(0, iters)
            .Select(_ => GetSize(r))
            .OrderBy(s => s)
            .Select(s => (int)s)
            .ToArray();

        $"[{sizes.First()} - {sizes.Last()}]; mean: {sizes.Average()}; median:{(sizes[sizes.Length >> 1] + sizes[sizes.Length - 1 >> 1]) / 2.0}".Dump();
        $"Running {benchTries} benchmarks of {threads} thread of each {iters} array creations".Dump();
    }

    Bench("Cheat", () =>
    {
        var r = new Random(37);
        for (int i = 0; i < iters; i++)
        {
            var s = GetSize(r);
            var b = new BigThing[s];
            for (var k = 0; k < s; k++)
                b[k] = new BigThing(k);
            b.ToArray();
        }
    });
    Bench("FastArrayBuilder", () =>
    {
        var r = new Random(37);
        for (int i = 0; i < iters; i++)
        {
            var s = GetSize(r);
            var b = FastArrayBuilder<BigThing>.Create();
            for (var k = 0; k < s; k++)
                b.Add(new BigThing(k));
            b.ToArray();
        }
    });
    Bench("FastArrayBuilder2", () =>
    {
        var r = new Random(37);
        for (int i = 0; i < iters; i++)
        {
            var s = GetSize(r);
            var b = new FastArrayBuilder2<BigThing>();
            for (var k = 0; k < s; k++)
                b.Add(new BigThing(k));
            b.ToArray();
        }
    });

    Bench("FastArrayBuilder2b", () =>
    {
        var r = new Random(37);
        for (int i = 0; i < iters; i++)
        {
            var s = GetSize(r);
            var b = new FastArrayBuilder2b<BigThing>();
            for (var k = 0; k < s; k++)
                b.Add(new BigThing(k));
            b.ToArray();
        }
    });
    Bench("FastArrayBuilder2c", () =>
    {
        var r = new Random(37);
        for (int i = 0; i < iters; i++)
        {
            var s = GetSize(r);
            var b = new FastArrayBuilder2c<BigThing>();
            for (var k = 0; k < s; k++)
                b.Add(new BigThing(k));
            b.ToArray();
        }
    });
    Bench("FastArrayBuilder3", () =>
    {
        var r = new Random(37);
        for (int i = 0; i < iters; i++)
        {
            var s = GetSize(r);
            var b = new FastArrayBuilder3<BigThing>();
            for (var k = 0; k < s; k++)
                b.Add(new BigThing(k));
            b.ToArray();
        }
    });
    Bench("List", () =>
    {
        var r = new Random(37);
        for (int i = 0; i < iters; i++)
        {
            var s = GetSize(r);
            var b = new List<BigThing>();
            for (var k = 0; k < s; k++)
                b.Add(new BigThing(k));
            b.ToArray();
        }
    });
}




static void Bench(string name, Action action)
{
    GC.Collect();
    var initialGen0 = GC.CollectionCount(0);
    var initialGen1 = GC.CollectionCount(1);
    var initialGen2 = GC.CollectionCount(2);
    var timings = Enumerable.Range(0, benchTries).Select(_ =>
    {
        var sw = Stopwatch.StartNew();
        var latencyDistribution = Enumerable.Range(0, threads).Select(__ => Task.Factory.StartNew(() =>
        {
            var swInner = Stopwatch.StartNew();
            action();
            return swInner.Elapsed.TotalMilliseconds;
        }, TaskCreationOptions.LongRunning)).ToArray().Aggregate(MeanVarianceAccumulator.Empty, (a, t) => a.Add(t.Result));
        sw.Stop();
        return (sw.Elapsed.TotalMilliseconds,latencyDistribution);
    }).OrderBy(o => o.TotalMilliseconds).ToArray();
    var distrib = timings.Select(o=>o.latencyDistribution).Aggregate((a,b)=>a.Add(b));
    var duration = timings[benchTries/5].TotalMilliseconds;
    var gen0 = GC.CollectionCount(0) - initialGen0;
    var gen1 = GC.CollectionCount(1) - initialGen1;
    var gen2 = GC.CollectionCount(2) - initialGen2;
    $"{name.PadLeft(20,'_')}: {gen0/(double)benchTries:f1}/{gen1/(double)benchTries:f1}/{gen2/(double)benchTries:f1}; {duration:f1}ms; per thread: {distrib}ms".Dump();
}