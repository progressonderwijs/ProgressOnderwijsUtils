using System.Reflection;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Mathematics.OutlierDetection;
using ProgressOnderwijsUtils.RequiredFields;
using ProgressOnderwijsUtils.Tests;

namespace ProgressOnderwijsUtilsBenchmarks.NullabilityVerifierBenchmark;

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

    static readonly NullabilityInfoContext context = new();

    static readonly IEnumerable<FieldInfo> reflectionBasedFieldsToCheck = typeof(NullablityTestClass).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .Where(f => context.Create(f).WriteState == NullabilityState.NotNull);

    [Benchmark]
    public void WithReflection()
        => _ = reflectionBasedFieldsToCheck
            .Where(f => f.GetValue(ObjToTest) == null)
            .Select(f => "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + f.Name)
            .ToArray();

    [Benchmark]
    public void NaiveHardCoded()
        => _ = new[] {
            ObjToTest.SomeNullString == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeNullString) : null,
            ObjToTest.SomeObject == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObject) : null,
            ObjToTest.SomeObjectArray == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObjectArray) : null,
            ObjToTest.SomeFilledObjectArray == null! ? "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeFilledObjectArray) : null,
        }.WhereNotNull().ToArray();

    [Benchmark]
    public void HardCoded()
    {
        var errCount = 0;

        if (ObjToTest.SomeNullString == null!) {
            errCount++;
        }
        if (ObjToTest.SomeObject == null!) {
            errCount++;
        }
        if (ObjToTest.SomeObjectArray == null!) {
            errCount++;
        }
        if (ObjToTest.SomeFilledObjectArray == null!) {
            errCount++;
        }
        if (errCount == 0) {
            _ = (string[]?)null;
        } else {
            var errors = new string[errCount];
            errCount = 0;

            if (ObjToTest.SomeNullString is null) {
                errors[errCount++] = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeNullString);
            }
            if (ObjToTest.SomeObject is null) {
                errors[errCount++] = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObject);
            }
            if (ObjToTest.SomeObjectArray is null) {
                errors[errCount++] = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeObjectArray);
            }
            if (ObjToTest.SomeFilledObjectArray is null) {
                errors[errCount] = "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + nameof(NullablityTestClass.SomeFilledObjectArray);
            }
            _ = errors;
        }
    }

    static readonly Func<NullablityTestClass, string[]?> Verifier = NonNullableFieldVerifier.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled()
        => _ = Verifier(ObjToTest);

    static readonly Func<NullablityTestClass, string> Verifier0 = NonNullableFieldVerifier0.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled0()
        => _ = Verifier0(ObjToTest);

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

    static readonly Func<NullablityTestClass, string[]?> Verifier5 = NonNullableFieldVerifier5.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    [Benchmark]
    public void Compiled5()
        => _ = Verifier5(ObjToTest);
}
