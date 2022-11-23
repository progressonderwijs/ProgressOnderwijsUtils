namespace ProgressOnderwijsUtils.Tests;

public sealed class NullablityTestClass
{
    //Intentionally violate nullability assumptions so we can test this:
    public string SomeNullString = null!;
    public string? SomeNullableField = null;
    public object SomeObject = null!;
    public object? SomeNullableObject = null;
    public object[] SomeObjectArray = null!;
    public object[] SomeFilledObjectArray = { null!, };
}

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
