namespace ProgressOnderwijsUtils.Tests;

public interface ISimpleInterface
{
    [UsedImplicitly]
    string? Property { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed record SimpleObject : IWrittenImplicitly, ISimpleInterface
{
    public int Field;
    public string? Property { get; set; }
    internal string IgnoredProperty { get; set; } = "ignored";
    public string HiddenProperty { get; set; } = "hidden";
    public required string LabelledProperty { get; set; }
    public string? MpReadonlyProperty { get; set; }
    string PrivateProperty { get; } = "private";
    DateTime PrivateField;
    public readonly double ReadonlyField;

    public double ReadonlyProperty
        => 0.0;

    public char WriteonlyProperty
    {
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public object? PrivateSetter { get; private set; }
    public object? PrivateGetter { set; private get; }
}

struct SetterTestStruct : IWrittenImplicitly
{
    public int IntProperty { get; set; }
    public string StringProperty { get; set; }
}

sealed class SetterTestClass : IWrittenImplicitly
{
    public int IntProperty { get; set; }
    public string? StringProperty { get; set; }
}

public sealed class PocoUtilsTest
{
    [Fact]
    public void ReturnsSameProperties()
    {
        var mps = PocoUtils.GetProperties<SimpleObject>();
        var mpsAlt = new SimpleObject { LabelledProperty = "label", }.GetProperties();
        PAssert.That(() => mps.SequenceEqual(mpsAlt));
    }

    [Fact]
    public void EnumeratesAsExpected()
    {
        var mps = PocoUtils.GetProperties<SimpleObject>();
        var names = mps.Select(pocoProperty => pocoProperty.Name);
        var expected = new[] {
            "Property",
            "HiddenProperty",
            "LabelledProperty",
            "MpReadonlyProperty",
            "ReadonlyProperty",
            "WriteonlyProperty",
            "PrivateSetter",
            "PrivateGetter",
        };
        PAssert.That(() => names.SequenceEqual(expected));
    }

    [Fact]
    public void IsReadable()
    {
        var readable = PocoUtils.GetProperties<SimpleObject>().Where(pocoProperty => pocoProperty.CanRead);
        var expected = new[] { "Property", "HiddenProperty", "LabelledProperty", "MpReadonlyProperty", "ReadonlyProperty", "PrivateSetter", };
        PAssert.That(() => readable.Select(pocoProperty => pocoProperty.Name).SequenceEqual(expected));
    }

    [Fact]
    public void IsWritable()
    {
        var writable = PocoUtils.GetProperties<SimpleObject>().Where(pocoProperty => pocoProperty.CanWrite);
        var expected = new[] { "Property", "HiddenProperty", "LabelledProperty", "MpReadonlyProperty", "WriteonlyProperty", "PrivateGetter", };
        PAssert.That(() => writable.Select(pocoProperty => pocoProperty.Name).SequenceEqual(expected));
    }

    [Fact]
    public void CanSetAndGet()
    {
        var o = new SimpleObject { Property = "foo", LabelledProperty = "bar", };
        var moDef = PocoUtils.GetProperties<SimpleObject>();
        PAssert.That(() => (string?)moDef.GetByName("Property").Getter!(o) == "foo");
        PAssert.That(() => (string?)moDef.GetByName("labelledProperty").Getter!(o) == "bar");

        moDef.GetByName("property").Setter!(ref o, "aha");
        moDef.GetByName("LabelledProperty").Setter!(ref o, "really");

        PAssert.That(() => o.Equals(new() { Property = "aha", LabelledProperty = "really", }));
    }

    [Fact]
    public void CanGetByExpression()
    {
        var pocoProperty = PocoUtils.GetByExpression((SimpleObject o) => o.Property);
        PAssert.That(() => pocoProperty.Name == "Property" && pocoProperty.DataType == typeof(string));
    }

    [Fact]
    public void CanReadWrite_ValueTypedProperty_On_ValueTypeObject()
    {
        var obj = new SetterTestStruct();
        var prop = PocoUtils.GetByExpression((SetterTestStruct o) => o.IntProperty);

        PAssert.That(() => (int?)prop.Getter!(obj) == 0);
        prop.Setter!(ref obj, 42);
        PAssert.That(() => (int?)prop.Getter!(obj) == 42);
    }

    [Fact]
    public void CanReadWrite_ReferenceTypedProperty_On_ValueTypeObject()
    {
        var obj = new SetterTestStruct();
        var prop = PocoUtils.GetByExpression((SetterTestStruct o) => o.StringProperty);

        PAssert.That(() => prop.Getter!(obj) == null);
        prop.Setter!(ref obj, "42");
        PAssert.That(() => (string?)prop.Getter!(obj) == "42");
    }

    [Fact]
    public void CanReadWrite_ValueTypedProperty_On_ReferenceTypeObject()
    {
        var obj = new SetterTestClass();
        var prop = PocoUtils.GetByExpression((SetterTestClass o) => o.IntProperty);

        PAssert.That(() => (int?)prop.Getter!(obj) == 0);
        prop.Setter!(ref obj, 42);
        PAssert.That(() => (int?)prop.Getter!(obj) == 42);
    }

    [Fact]
    public void CanReadWrite_ReferenceTypedProperty_On_ReferenceTypeObject()
    {
        var obj = new SetterTestClass();
        var prop = PocoUtils.GetByExpression((SetterTestClass o) => o.StringProperty);

        PAssert.That(() => prop.Getter!(obj) == null);
        prop.Setter!(ref obj, "42");
        PAssert.That(() => (string?)prop.Getter!(obj) == "42");
    }

    [Fact]
    public void CanContainNull_SanityChecks()
    {
        var setterTestClassString = PocoUtils.GetByExpression((SetterTestClass o) => o.StringProperty);
        PAssert.That(() => setterTestClassString.CanContainNull);

        var setterTestStructString = PocoUtils.GetByExpression((SetterTestStruct o) => o.StringProperty);
        var nullabilityContext = new NullabilityInfoContext();
        PAssert.That(
            () => setterTestStructString.CanContainNull
                && nullabilityContext.Create(setterTestStructString.PropertyInfo).ReadState == NullabilityState.NotNull,
            "Voor structs is C# nullability lek: non-nullable, non-required props zijn toegestaan. "
            + "Dit lijkt een variatie op https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references#known-pitfalls"
        );

        var setterTestClassInt = PocoUtils.GetByExpression((SetterTestClass o) => o.IntProperty);
        PAssert.That(() => !setterTestClassInt.CanContainNull);

        var setterTestStructInt = PocoUtils.GetByExpression((SetterTestStruct o) => o.IntProperty);
        PAssert.That(() => !setterTestStructInt.CanContainNull);

        var hiddenProperty = PocoUtils.GetByExpression((SimpleObject o) => o.HiddenProperty);
        PAssert.That(() => !hiddenProperty.CanContainNull);

        var privateSetter = PocoUtils.GetByExpression((SimpleObject o) => o.PrivateSetter);
        PAssert.That(() => privateSetter.CanContainNull);

        var labelledProperty = PocoUtils.GetByExpression((SimpleObject o) => o.LabelledProperty);
        PAssert.That(() => !labelledProperty.CanContainNull);

        var mpReadonlyProperty = PocoUtils.GetByExpression((SimpleObject o) => o.MpReadonlyProperty);
        PAssert.That(() => mpReadonlyProperty.CanContainNull);
    }
}
