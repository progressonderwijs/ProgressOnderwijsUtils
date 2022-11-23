using System.Reflection;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Mathematics.OutlierDetection;
using ProgressOnderwijsUtils.Tests;

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
            nullablityTestClass.SomeNullString == null! ? WarnAbout(nameof(NullablityTestClass.SomeNullString)) : null,
            nullablityTestClass.SomeObject == null! ? WarnAbout(nameof(NullablityTestClass.SomeObject)) : null,
            nullablityTestClass.SomeObjectArray == null! ? WarnAbout(nameof(NullablityTestClass.SomeObjectArray)) : null,
            nullablityTestClass.SomeFilledObjectArray == null! ? WarnAbout(nameof(NullablityTestClass.SomeFilledObjectArray)) : null
        }.WhereNotNull().ToArray();

    [Benchmark]
    public string[]? HardCoded2()
    {
        var errCount = 0;

        string? v1;
        if (nullablityTestClass.SomeNullString == null!) {
            v1 = WarnAbout(nameof(NullablityTestClass.SomeNullString));
            errCount += 1;

        } else {
            v1 = null;
        }
        string? v2;
        if (nullablityTestClass.SomeObject == null!) {
            v2 = WarnAbout(nameof(NullablityTestClass.SomeObject));
            errCount += 1;
        } else {
            v2 = null;
        }
        string? v3;
        if (nullablityTestClass.SomeObjectArray == null!) {
            v3 = WarnAbout(nameof(NullablityTestClass.SomeObjectArray));
            errCount += 1;
        } else {
            v3 = null;
        }
        string? v4;
        if (nullablityTestClass.SomeFilledObjectArray == null!) {
            v4 = WarnAbout(nameof(NullablityTestClass.SomeFilledObjectArray));
            errCount += 1;
        } else {
            v4 = null;
        }
        if(errCount == 0 ) return null;
        var errors = new string[errCount];
        errCount = 0;

        if (v1 is not null) {
            errors[errCount++] = v1;
        } 
        if (v2 is not null) {
            errors[errCount++] = v2;
        } 
        if (v3 is not null) {
            errors[errCount++] = v3;
        } 

        if (v4 is not null) {
            errors[errCount] = v4;
        }
        return errors;

    }

    static readonly Func<NullablityTestClass, string[]?> Verifier = NonNullableFieldVerifier.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled()
        => _ = Verifier(nullablityTestClass);
    static readonly Func<NullablityTestClass, string> Verifier0 = NonNullableFieldVerifier0.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled0()
        => _ = Verifier0(nullablityTestClass);

    static readonly Func<NullablityTestClass, string[]?> Verifier1 = NonNullableFieldVerifier1.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled1()
        => _ = Verifier1(nullablityTestClass);

    static readonly Func<NullablityTestClass, string[]?> Verifier2 = NonNullableFieldVerifier2.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled2()
        => _ = Verifier2(nullablityTestClass);
}
