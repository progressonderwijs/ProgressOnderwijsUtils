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

    readonly NullablityTestClass NullablityTestClass = new();

    readonly NullablityTestClass NullablityNullTestClass = new() {
        SomeNullString = "",
        SomeNullableField = null,
        SomeObject = new(),
        SomeNullableObject = null,
        SomeObjectArray = new object[] { },
        SomeFilledObjectArray = new object[] { },
    };

    readonly NullablityTestClass NullablityOneNullTestClass = new() {
        SomeNullString = null!,
        SomeNullableField = null,
        SomeObject = new(),
        SomeNullableObject = null,
        SomeObjectArray = new object[] { },
        SomeFilledObjectArray = new object[] { },
    };

    static string WarnAbout(string field)
        => "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + field;

    readonly NullabilityInfoContext context = new();

    string[] CheckValidNonNullablitiy(Type type)
        => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(
                f => f.GetValue(NullablityTestClass) == null && context.Create(f).WriteState == NullabilityState.NotNull
                    ? WarnAbout(f.Name)
                    : null
            ).WhereNotNull().ToArray();

    [Benchmark]
    public void WithReflection()
        => _ = CheckValidNonNullablitiy(typeof(NullablityTestClass));

    [Benchmark]
    public void HardCoded()
        => _ = new[] {
            NullablityTestClass.SomeNullString == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(ProgressOnderwijsUtils.Tests.NullablityTestClass.SomeNullString) : null,
            NullablityTestClass.SomeObject == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(ProgressOnderwijsUtils.Tests.NullablityTestClass.SomeObject) : null,
            NullablityTestClass.SomeObjectArray == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(ProgressOnderwijsUtils.Tests.NullablityTestClass.SomeObjectArray) : null,
            NullablityTestClass.SomeFilledObjectArray == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(ProgressOnderwijsUtils.Tests.NullablityTestClass.SomeFilledObjectArray) : null,
        }.WhereNotNull().ToArray();

    public static string[]? HardCoded2Meth(NullablityTestClass nullablityTestClass)
    {
        var errCount = 0;

        string? v1;
        if (nullablityTestClass.SomeNullString == null!) {
            v1 = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(ProgressOnderwijsUtils.Tests.NullablityTestClass.SomeNullString);
            errCount += 1;
        } else {
            v1 = null;
        }
        string? v2;
        if (nullablityTestClass.SomeObject == null!) {
            v2 = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(ProgressOnderwijsUtils.Tests.NullablityTestClass.SomeObject);
            errCount += 1;
        } else {
            v2 = null;
        }
        string? v3;
        if (nullablityTestClass.SomeObjectArray == null!) {
            v3 = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(ProgressOnderwijsUtils.Tests.NullablityTestClass.SomeObjectArray);
            errCount += 1;
        } else {
            v3 = null;
        }
        string? v4;
        if (nullablityTestClass.SomeFilledObjectArray == null!) {
            v4 = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(ProgressOnderwijsUtils.Tests.NullablityTestClass.SomeFilledObjectArray);
            errCount += 1;
        } else {
            v4 = null;
        }
        if (errCount == 0) {
            return null;
        }
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

    [Benchmark]
    public void HardCoded2()
        => _ = HardCoded2Meth(NullablityTestClass);

    [Benchmark]
    public void HardCoded2OneNull()
        => _ = HardCoded2Meth(NullablityOneNullTestClass);

    [Benchmark]
    public void HardCodedNoNull2()
        => _ = HardCoded2Meth(NullablityNullTestClass);

    static readonly Func<NullablityTestClass, string> Verifier0 = NonNullableFieldVerifier0.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled0()
        => _ = Verifier0(NullablityTestClass);

    [Benchmark]
    public void Compiled0OneNull()
        => _ = Verifier0(NullablityOneNullTestClass);

    [Benchmark]
    public void Compiled0NoNull()
        => _ = Verifier0(NullablityNullTestClass);

    static readonly Func<NullablityTestClass, string[]?> Verifier = NonNullableFieldVerifier.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled()
        => _ = Verifier(NullablityTestClass);

    [Benchmark]
    public void CompiledOneNull()
        => _ = Verifier(NullablityOneNullTestClass);

    [Benchmark]
    public void CompiledNoNull()
        => _ = Verifier(NullablityNullTestClass);

    static readonly Func<NullablityTestClass, string[]?> Verifier1 = NonNullableFieldVerifier1.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled1()
        => _ = Verifier1(NullablityTestClass);

    [Benchmark]
    public void CompiledOneNull1()
        => _ = Verifier1(NullablityOneNullTestClass);

    [Benchmark]
    public void CompiledNoNull1()
        => _ = Verifier1(NullablityNullTestClass);

    static readonly Func<NullablityTestClass, string[]?> Verifier2 = NonNullableFieldVerifier2.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled2()
        => _ = Verifier2(NullablityTestClass);

    [Benchmark]
    public void CompiledOneNull2()
        => _ = Verifier2(NullablityOneNullTestClass);

    [Benchmark]
    public void CompiledNoNull2()
        => _ = Verifier2(NullablityNullTestClass);

    static readonly Func<NullablityTestClass, string[]?> Verifier3 = NonNullableFieldVerifier3.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled3()
        => _ = Verifier3(NullablityTestClass);

    [Benchmark]
    public void CompiledOneNull3()
        => _ = Verifier3(NullablityOneNullTestClass);

    [Benchmark]
    public void CompiledNoNull3()
        => _ = Verifier3(NullablityNullTestClass);
}
