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
using ProgressOnderwijsUtils.Tests;
using Xunit.Sdk;

namespace ProgressOnderwijsUtilsBenchmarks;

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

    readonly NullablityTestClass nullablityTestClass = new();

    static string WarnAbout(string field)
        => "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + field;

    readonly NullabilityInfoContext context = new();

    string[] CheckValidNonNullablitiy(Type type)
        => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(
                f => f.GetValue(nullablityTestClass) == null && context.Create(f).WriteState == NullabilityState.NotNull
                    ? WarnAbout(f.Name)
                    : null
            ).WhereNotNull().ToArray();

    [Benchmark]
    public void WithReflection()
        => _ = CheckValidNonNullablitiy(typeof(NullablityTestClass));

    [Benchmark]
    public void HardCoded()
        => _ = new[] {
            nullablityTestClass.SomeNullString == null! ? WarnAbout(nameof(NullablityTestClass.SomeNullString)) : "",
            nullablityTestClass.SomeObject == null! ? WarnAbout(nameof(NullablityTestClass.SomeObject)) : "",
            nullablityTestClass.SomeObjectArray == null! ? WarnAbout(nameof(NullablityTestClass.SomeObjectArray)) : "",
            nullablityTestClass.SomeFilledObjectArray == null! ? WarnAbout(nameof(NullablityTestClass.SomeFilledObjectArray)) : ""
        };

    static readonly Func<NullablityTestClass, string[]?> Verifier = NonNullableFieldVerifier0.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled()
        => _ = Verifier(nullablityTestClass);

    static readonly Func<NullablityTestClass, string[]?> Verifier1 = NonNullableFieldVerifier1.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled1()
        => _ = Verifier1(nullablityTestClass);

    static readonly Func<NullablityTestClass, string[]?> Verifier2 = NonNullableFieldVerifier2.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled2()
        => _ = Verifier2(nullablityTestClass);
}
