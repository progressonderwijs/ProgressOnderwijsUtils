namespace ProgressOnderwijsUtils.Tests;

public interface ISimpleInterface
{
    [UsedImplicitly]
    string? Property { get; set; }
}

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0169 // field unused
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
#pragma warning restore CS0169 // field unused
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members

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
        PAssert.That(() => (string?)moDef.GetByName("Property").Getter.AssertNotNull()(o) == "foo");
        PAssert.That(() => (string?)moDef.GetByName("labelledProperty").Getter.AssertNotNull()(o) == "bar");

        moDef.GetByName("property").Setter.AssertNotNull()(ref o, "aha");
        moDef.GetByName("LabelledProperty").Setter.AssertNotNull()(ref o, "really");

        PAssert.That(() => o.Equals(new() { Property = "aha", LabelledProperty = "really", }));
    }

    [Fact]
    public void CanTryGet()
    {
        var o = new SimpleObject { LabelledProperty = "bar", };
        var moDef = PocoUtils.GetProperties<SimpleObject>();

        var existing = moDef.TryGetByName(nameof(SimpleObject.LabelledProperty), out var prop);
        PAssert.That(() => existing);
        PAssert.That(() => (string?)prop.AssertNotNull().Getter.AssertNotNull()(o) == "bar");

        existing = moDef.TryGetByName("NonExisting", out var noprop);
        PAssert.That(() => !existing);
        PAssert.That(() => noprop == null);
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

        PAssert.That(() => (int?)prop.Getter.AssertNotNull()(obj) == 0);
        prop.Setter.AssertNotNull()(ref obj, 42);
        PAssert.That(() => (int?)prop.Getter.AssertNotNull()(obj) == 42);
    }

    [Fact]
    public void CanReadWrite_ReferenceTypedProperty_On_ValueTypeObject()
    {
        var obj = new SetterTestStruct();
        var prop = PocoUtils.GetByExpression((SetterTestStruct o) => o.StringProperty);

        PAssert.That(() => prop.Getter.AssertNotNull()(obj) == null);
        prop.Setter.AssertNotNull()(ref obj, "42");
        PAssert.That(() => (string?)prop.Getter.AssertNotNull()(obj) == "42");
    }

    [Fact]
    public void CanReadWrite_ValueTypedProperty_On_ReferenceTypeObject()
    {
        var obj = new SetterTestClass();
        var prop = PocoUtils.GetByExpression((SetterTestClass o) => o.IntProperty);

        PAssert.That(() => (int?)prop.Getter.AssertNotNull()(obj) == 0);
        prop.Setter.AssertNotNull()(ref obj, 42);
        PAssert.That(() => (int?)prop.Getter.AssertNotNull()(obj) == 42);
    }

    [Fact]
    public void CanReadWrite_ReferenceTypedProperty_On_ReferenceTypeObject()
    {
        var obj = new SetterTestClass();
        var prop = PocoUtils.GetByExpression((SetterTestClass o) => o.StringProperty);

        PAssert.That(() => prop.Getter.AssertNotNull()(obj) == null);
        prop.Setter.AssertNotNull()(ref obj, "42");
        PAssert.That(() => (string?)prop.Getter.AssertNotNull()(obj) == "42");
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

    sealed record PropertiesDiffLogPoco(int i, string? s) : IReadImplicitly;

    [Fact]
    public void PropertiesDiffLogTest()
    {
        PAssert.That(() => PocoUtils.PropertiesDiffLog(new PropertiesDiffLogPoco(1, null), new PropertiesDiffLogPoco(1, null)) == "");
        PAssert.That(() => PocoUtils.PropertiesDiffLog(new PropertiesDiffLogPoco(1, null), new PropertiesDiffLogPoco(2, null)) == "i=1»»2");
        PAssert.That(() => PocoUtils.PropertiesDiffLog(new PropertiesDiffLogPoco(1, null), new PropertiesDiffLogPoco(1, "iets")) == "s=»»iets");
        PAssert.That(() => PocoUtils.PropertiesDiffLog(new PropertiesDiffLogPoco(1, null), new PropertiesDiffLogPoco(2, "iets")) == "i=1»»2; s=»»iets");
    }
}
