using System.Linq.Expressions;

//Intentionally violate nullability assumptions so we can test this:
#pragma warning disable CS8625
// ReSharper disable NullableWarningSuppressionIsUsed
namespace ProgressOnderwijsUtils.Tests;

public sealed class NonNullableNullabilityCheck
{
    readonly NullablityTestPropertyClass containingAllNullPropertyClass = new(null!, null, null!, null!, null, [null,]);

    readonly NullablityTestClass OneContainingNull = new() {
        SomeNullString = null, //non nullable
        Name = null,
        SomeObject = new(),
        SomeNullableObject = null,
        SomeObjectArray = [new(),],
        SomeFilledObjectArray = [new(),],
    };

    readonly NullablityTestClass AllContainingNull = new();
    readonly NullablityTestStruct AllContainingNull_struct = new();

    readonly NullablityTestClass NotContainingNull = new() {
        SomeNullString = "",
        Name = null,
        SomeObject = new(),
        SomeNullableObject = null,
        SomeObjectArray = [new(),],
        SomeFilledObjectArray = [new(),],
    };

    readonly NullabilityTestSubClass ContainingAllNullSubClass = new();

    [Fact]
    public void AssertOneNullFieldIsDetected()
        => ValidateExpectedNullabilityErrors(OneContainingNull, o => o.SomeNullString);

    static void ValidateExpectedNullabilityErrors<T>(T poco, params Expression<Func<T, object>>[] membersReportingNullabilityErrors)
    {
        var typeName = typeof(T).ToCSharpFriendlyTypeName();
        var foundErrors = NonNullableFieldVerifier.Verify(poco).EmptyIfNull();
        var expectedErrors = membersReportingNullabilityErrors.ArraySelect(prop => $"{typeName}.{ExpressionToCode.GetNameIn(prop)} contains NULL despite being non-nullable");
        var unexpectedErrors = foundErrors.Except(expectedErrors);
        var missingErrors = expectedErrors.Except(foundErrors);
        PAssert.That(() => unexpectedErrors.None() && missingErrors.None(), $"{typeName} poco ({poco}) did not report the expected nullability errors");
    }

    [Fact]
    public void AssertAllNullFieldsAreDetected()
        => ValidateExpectedNullabilityErrors(AllContainingNull, o => o.SomeNullString, o => o.SomeObject, o => o.SomeObjectArray);

    [Fact]
    public void AssertNoNullFieldsReturnsNull()
        => PAssert.That(() => NonNullableFieldVerifier.Verify(NotContainingNull) == null);

    [Fact]
    public void AssertAllNullFieldsAreDetected_SubClass()
        => ValidateExpectedNullabilityErrors(ContainingAllNullSubClass, o => o.SomeNullString, o => o.SomeObject, o => o.SomeObjectArray);

    [Fact]
    public void AssertAllNullFieldsAreDetected_ConstructorBasedProperties()
        => ValidateExpectedNullabilityErrors(containingAllNullPropertyClass, o => o.SomeNullString, o => o.SomeObject, o => o.SomeObjectArray);

    [Fact]
    public void AssertAllNullFieldsAreDetected_Struct()
        => ValidateExpectedNullabilityErrors(AllContainingNull_struct, o => o.SomeNullString, o => o.SomeObject, o => o.SomeObjectArray);
}

[UsedImplicitlyBySerialization]
public sealed class NullablityTestClass
{
    public string SomeNullString = null!;
    public string? Name = "Everything null";
    public object SomeObject = null!;
    public object? SomeNullableObject;
    public object[] SomeObjectArray = null!;
    public object[] SomeFilledObjectArray = [null!,];

    public override string? ToString()
        => Name;
}

[UsedImplicitlyBySerialization]
public sealed class NullablityTestStruct
{
    public string SomeNullString = null!;
    public string? Name;
    public object SomeObject = null!;
    public object? SomeNullableObject;
    public object[] SomeObjectArray { get; set; } = null!;
    public object[] SomeFilledObjectArray = [null!,];
}

[UsedImplicitlyBySerialization]
public sealed record NullablityTestPropertyClass(string SomeNullString, string? SomeNullableField, object SomeObject, object? SomeNullableObject, object[] SomeObjectArray, object[] SomeFilledObjectArray);

[UsedImplicitlyBySerialization]
public abstract class NullablityTestBaseClass
{
    public string SomeNullString = null!;
    public object SomeObject = null!;
    public object? SomeNullableObject;
    public object[] SomeObjectArray = null!;
    public object[] SomeFilledObjectArray = [null!,];
}

public sealed class NullabilityTestSubClass : NullablityTestBaseClass { }
