namespace ProgressOnderwijsUtils.Tests;

public sealed class NullablityTestClass
{
    //Intentionally violate nullability assumptions so we can test this:
    public string SomeNullString = null!;
    public string? Name = "Everything null";
    public object SomeObject = null!;
    public object? SomeNullableObject = null;
    public object[] SomeObjectArray = null!;
    public object[] SomeFilledObjectArray = { null!, };
    public override string? ToString() => Name;
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

public sealed class NullablityTestNestedClass
{
    public NullablityTestClass SomeNestedClass = new NullablityTestClass();
}

public sealed class NullabilityTestSubClass : NullablityTestBaseClass { }
