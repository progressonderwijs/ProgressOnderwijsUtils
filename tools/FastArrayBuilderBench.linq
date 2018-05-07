<Query Kind="Program">
  <Reference>C:\VCS\LocalOnly\programs\EmnExtensions\bin\Release\EmnExtensions.dll</Reference>
  <Reference>C:\VCS\external\ProgressOnderwijsUtils\src\ProgressOnderwijsUtils\bin\Release\net471\ProgressOnderwijsUtils.dll</Reference>
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


const int threads = 7;

const int extraShift = 14;
const int iters = 200000;
/*
struct BigThing {
    long A,B,C,D,E,F,G,H;
    public BigThing(int num) 
    {
        A=B=C=D=E=F=G=H=num;
    }
}
/*/
struct BigThing
{
    short A;
    public BigThing(int num)
    {
        A = (short)num;
    }
}
/**/
int GetSize(Random r)
{
    var n = (uint)r.Next();
    return (int)(n >> (int)(Math.Min(4 + extraShift + (n & 15),31)));
}

void Main()
{
    {
        var r = new Random(37);
        var sizes = Enumerable.Range(0,iters)
            .Select(_=>GetSize(r))
            .OrderBy(s=>s)
            .Select(s=>(int)s)
            .ToArray();

        $"[{sizes.First()} - {sizes.Last()}]; mean: {sizes.Average()}; median:{(sizes[sizes.Length>>1] + sizes[sizes.Length-1>>1])/2.0}".Dump();
    }

    for (int run = 0; run < 3; run++)
    {
        Bench("List", () =>
        {
            var r =new Random(37);
            for (int i = 0; i < iters; i++)
            {
                var s = GetSize(r);
                var b = new List<BigThing>();
                for (var k = 0; k < s; k++)
                    b.Add(new BigThing(k));
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

    }
}




static void Bench(string name, Action action)
{
    GC.Collect();
    var initialGen0 = GC.CollectionCount(0);
    var initialGen1 = GC.CollectionCount(1);
    var initialGen2 = GC.CollectionCount(2);
    var sw = Stopwatch.StartNew();
    var latencyDistribution = Enumerable.Range(0, threads).Select(_ => Task.Run(() =>
    {
        var swInner = Stopwatch.StartNew();
        action();
        return swInner.Elapsed.TotalMilliseconds;
    })).ToArray().Aggregate(MeanVarianceAccumulator.Empty, (a, t) => a.Add(t.Result));
    sw.Stop();
    var gen0 = GC.CollectionCount(0) - initialGen0;
    var gen1 = GC.CollectionCount(1) - initialGen1;
    var gen2 = GC.CollectionCount(2) - initialGen2;
    $"{name.PadLeft(20,'_')}: {gen0}/{gen1}/{gen2}; {sw.Elapsed.TotalMilliseconds:f1}ms; per thread: {latencyDistribution}ms".Dump();
}
