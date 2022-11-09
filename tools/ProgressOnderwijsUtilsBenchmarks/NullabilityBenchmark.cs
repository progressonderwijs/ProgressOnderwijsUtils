using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Mathematics.OutlierDetection;
using ProgressOnderwijsUtils.Data;
using Xunit.Sdk;

namespace ProgressOnderwijsUtilsBenchmarks;
#nullable enable
[Config(typeof(Config))]
[MemoryDiagnoser]
public sealed class NullabilityBenchmark
{
    sealed class Config : ManualConfig
    {
        public Config()
            => _ = AddJob(
                Job.MediumRun
                    .WithLaunchCount(1)
                    .WithWarmupCount(3)
                    .WithOutlierMode(OutlierMode.DontRemove)
                    .WithMaxIterationCount(5000)
                    .WithMaxRelativeError(0.002)
                    .WithToolchain(InProcessNoEmitToolchain.Instance)
                    .WithId("InProcess")
            );
    }

    readonly NullablityTestClass nullablityTestClass = new NullablityTestClass();

    static string WarnAbout(string field)
        => $"{field} is a non nullable field with a null value.\n";

    readonly NullabilityInfoContext context = new();

    string CheckValidNonNullablitiy(Type type)
        => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(
                f => f.GetValue(nullablityTestClass) == null && context.Create(f).WriteState == NullabilityState.NotNull
                    ? WarnAbout(f.Name)
                    : null
            ).WhereNotNull().JoinStrings();

    [Benchmark]
    public void WithReflection()
    {
        _ = CheckValidNonNullablitiy(typeof(NullablityTestClass));
    }

    [Benchmark]
    public void HardCoded()
    {
        _ = ""
            + (nullablityTestClass.SomeNullString == null ? WarnAbout(nameof(NullablityTestClass.SomeNullString)) : "")
            + (nullablityTestClass.SomeObject == null ? WarnAbout(nameof(NullablityTestClass.SomeObject)) : "")
            + (nullablityTestClass.SomeObjectArray == null ? WarnAbout(nameof(NullablityTestClass.SomeObjectArray)) : "")
            + (nullablityTestClass.SomeFilledObjectArray == null ? WarnAbout(nameof(NullablityTestClass.SomeFilledObjectArray)) : "")
            ;
    }

    static readonly NonNullableFieldVerifier verifier = new();

    [Benchmark]
    public void Compiled()
    {
        _ = verifier.Verify(nullablityTestClass);
    }
}
