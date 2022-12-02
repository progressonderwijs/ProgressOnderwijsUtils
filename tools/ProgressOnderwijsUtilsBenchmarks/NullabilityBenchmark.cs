using System.Reflection;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Mathematics.OutlierDetection;
using ProgressOnderwijsUtils.RequiredFields;
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
                    .WithWarmupCount(2)
                    .WithOutlierMode(OutlierMode.DontRemove)
                    .WithMaxIterationCount(3000)
                    .WithMaxRelativeError(0.01)
                    .WithToolchain(InProcessNoEmitToolchain.Instance)
                    .WithId("InProcess")
            );
    }

    static readonly NullablityTestClass EverythingInvalidTestCase = new() { Name = nameof(EverythingInvalidTestCase), };

    static readonly NullablityTestClass ValidTestCase = new() {
        SomeNullString = "",
        Name = nameof(ValidTestCase),
        SomeObject = new(),
        SomeNullableObject = null,
        SomeObjectArray = new object[] { },
        SomeFilledObjectArray = new object[] { },
    };

    static readonly NullablityTestClass OneNullInNonNullTestCase = new() {
        SomeNullString = null!,
        Name = nameof(OneNullInNonNullTestCase),
        SomeObject = new(),
        SomeNullableObject = null,
        SomeObjectArray = new object[] { },
        SomeFilledObjectArray = new object[] { },
    };

    public static NullablityTestClass[] ObjectsToTest { get; set; } = { EverythingInvalidTestCase, ValidTestCase, OneNullInNonNullTestCase, };

    [ParamsSource(nameof(ObjectsToTest))]
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public NullablityTestClass ObjToTest = null!;

    static string WarnAbout(string field)
        => "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + field;

    readonly NullabilityInfoContext context = new();

    string[] CheckValidNonNullablitiy(Type type, NullablityTestClass obj)
        => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(
                f => f.GetValue(obj) == null && context.Create(f).WriteState == NullabilityState.NotNull
                    ? WarnAbout(f.Name)
                    : null
            ).WhereNotNull().ToArray();

    [Benchmark]
    public void WithReflection()
        => _ = CheckValidNonNullablitiy(typeof(NullablityTestClass), ObjToTest);

    [Benchmark]
    public void HardCoded()
        => _ = new[] {
            ObjToTest.SomeNullString == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeNullString) : null,
            ObjToTest.SomeObject == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObject) : null,
            ObjToTest.SomeObjectArray == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObjectArray) : null,
            ObjToTest.SomeFilledObjectArray == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeFilledObjectArray) : null,
        }.WhereNotNull().ToArray();

    public static string[]? HardCoded2Meth(NullablityTestClass nullablityTestClass)
    {
        var errCount = 0;

        string? v1;
        if (nullablityTestClass.SomeNullString == null!) {
            v1 = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeNullString);
            errCount += 1;
        } else {
            v1 = null;
        }
        string? v2;
        if (nullablityTestClass.SomeObject == null!) {
            v2 = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObject);
            errCount += 1;
        } else {
            v2 = null;
        }
        string? v3;
        if (nullablityTestClass.SomeObjectArray == null!) {
            v3 = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObjectArray);
            errCount += 1;
        } else {
            v3 = null;
        }
        string? v4;
        if (nullablityTestClass.SomeFilledObjectArray == null!) {
            v4 = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeFilledObjectArray);
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
        => _ = HardCoded2Meth(ObjToTest);


    [Benchmark]
    public void HardCoded3()
        => _ = HardCoded3Meth(ObjToTest);

    public static string[]? HardCoded3Meth(NullablityTestClass nullablityTestClass)
    {
        var errCount = 0;

        if (nullablityTestClass.SomeNullString == null!) {
            errCount++;
        }
        if (nullablityTestClass.SomeObject == null!) {
            errCount++;
        }
        if (nullablityTestClass.SomeObjectArray == null!) {
            errCount++;
        }
        if (nullablityTestClass.SomeFilledObjectArray == null!) {
            errCount++;
        }
        if (errCount == 0) {
            return null;
        }
        var errors = new string[errCount];
        errCount = 0;

        if (nullablityTestClass.SomeNullString is null) {
            errors[errCount++] = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeNullString);
        }
        if (nullablityTestClass.SomeObject is null) {
            errors[errCount++] = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObject);
        }
        if (nullablityTestClass.SomeObjectArray is null) {
            errors[errCount++] = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObjectArray);
        }
        if (nullablityTestClass.SomeFilledObjectArray is null) {
            errors[errCount] = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeFilledObjectArray);
        }
        return errors;
    }


    static readonly Func<NullablityTestClass, string> Verifier0 = NonNullableFieldVerifier0.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled0()
        => _ = Verifier0(ObjToTest);

    static readonly Func<NullablityTestClass, string[]?> Verifier = NonNullableFieldVerifier.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled()
        => _ = Verifier(ObjToTest);

    static readonly Func<NullablityTestClass, string[]?> Verifier1 = NonNullableFieldVerifier1.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled1()
        => _ = Verifier1(ObjToTest);


    static readonly Func<NullablityTestClass, string[]?> Verifier2 = NonNullableFieldVerifier2.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled2()
        => _ = Verifier2(ObjToTest);

    static readonly Func<NullablityTestClass, string[]?> Verifier3 = NonNullableFieldVerifier3.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled3()
        => _ = Verifier3(ObjToTest);

    static readonly Func<NullablityTestClass, string[]?> Verifier4 = NonNullableFieldVerifier4.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled4()
        => _ = Verifier4(ObjToTest);

}
