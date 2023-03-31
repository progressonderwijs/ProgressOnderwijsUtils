#pragma warning disable CS8625

namespace ProgressOnderwijsUtils.Tests;

public sealed class NonNullableNullabilityCheck
{
    readonly NullablityTestPropertyClass containingAllNullPropertyClass = new(null!, null, null!, null!, null, new object[] { null, });

    readonly NullablityTestClass OneContainingNull = new() {
        SomeNullString = null, //non nullable
        Name = null,
        SomeObject = new(),
        SomeNullableObject = null,
        SomeObjectArray = new object[] { },
        SomeFilledObjectArray = new object[] { },
    };

    readonly NullablityTestClass AllContainingNull = new();

    readonly NullablityTestClass NotContainingNull = new() {
        SomeNullString = "",
        Name = null,
        SomeObject = new(),
        SomeNullableObject = null,
        SomeObjectArray = new object[] { },
        SomeFilledObjectArray = new object[] { },
    };

    readonly NullabilityTestSubClass ContainingAllNullSubClass = new();

    static string getVerifierMessage(string field)
        => "NullablityTestClass." + field + " contains NULL despite being non-nullable";

    [Fact]
    public void AssertOneNullFieldIsDetected()
        => ValidateExpectedNullabilityErrors(OneContainingNull, nameof(NullablityTestClass.SomeNullString));

    static void ValidateExpectedNullabilityErrors<T>(T poco, params string[] membersReportingNullabilityErrors)
    {
        var foundErrors = NonNullableFieldVerifier.Verify(poco).EmptyIfNull();
        var expectedErrors = membersReportingNullabilityErrors.ArraySelect(getVerifierMessage);
        var unexpectedErrors = foundErrors.Except(expectedErrors);
        var missingErrors = expectedErrors.Except(foundErrors);
        PAssert.That(() => unexpectedErrors.None() && missingErrors.None(), $"{typeof(T).ToCSharpFriendlyTypeName()} poco ({poco}) did not report the expected nullability errors");
    }

    [Fact]
    public void AssertAllNullFieldsAreDetected()
        => ValidateExpectedNullabilityErrors(AllContainingNull, nameof(NullablityTestClass.SomeNullString), nameof(NullablityTestClass.SomeObject), nameof(NullablityTestClass.SomeObjectArray));

    [Fact]
    public void AssertNoNullFieldsReturnsNull()
        => PAssert.That(() => NonNullableFieldVerifier.Verify(NotContainingNull) == null);
}

public sealed class NullablityTestClass
{
    //Intentionally violate nullability assumptions so we can test this:
    public string SomeNullString = null!;
    public string? Name = "Everything null";
    public object SomeObject = null!;
    public object? SomeNullableObject;
    public object[] SomeObjectArray = null!;
    public object[] SomeFilledObjectArray = { null!, };

    public override string? ToString()
        => Name;
}

public sealed record NullablityTestPropertyClass(string SomeNullString, string? SomeNullableField, object SomeObject, object? SomeNullableObject, object[] SomeObjectArray, object[] SomeFilledObjectArray);

public abstract class NullablityTestBaseClass
{
    //Intentionally violate nullability assumptions so we can test this:
    public string SomeNullString = null!;
    public string? SomeNullableField = null;
    public object SomeObject = null!;
    public object? SomeNullableObject = null;
    public object[] SomeObjectArray = null!;
    public object[] SomeFilledObjectArray = { null!, };
}

public sealed class NullabilityTestSubClass : NullablityTestBaseClass { }
