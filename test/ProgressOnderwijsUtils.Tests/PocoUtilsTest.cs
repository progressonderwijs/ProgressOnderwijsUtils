﻿using System;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public interface ISimpleInterface
    {
        [UsedImplicitly]
        string Property { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed record SimpleObject : IWrittenImplicitly, ISimpleInterface
    {
#pragma warning disable 169
#pragma warning disable 649
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public int Field;
        public string Property { get; set; }
        internal string IgnoredProperty { get; set; }
        public string HiddenProperty { get; set; }
        public string LabelledProperty { get; set; }
        public string MpReadonlyProperty { get; set; }
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
        string PrivateProperty { get; }
        DateTime PrivateField;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
        public readonly double ReadonlyField;

        public double ReadonlyProperty
            => 0.0;

        public char WriteonlyProperty
        {
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        public object PrivateSetter { get; private set; }
        public object PrivateGetter { set; private get; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
#pragma warning restore 169
#pragma warning restore 649
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
            var mpsAlt = new SimpleObject().GetProperties();
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
            var expected = new[] { "Property", "HiddenProperty", "LabelledProperty", "MpReadonlyProperty", "ReadonlyProperty", "PrivateSetter" };
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
            var o = new SimpleObject { Property = "foo", LabelledProperty = "bar" };
            var moDef = PocoUtils.GetProperties<SimpleObject>();
            PAssert.That(() => (string?)moDef.GetByName("Property").Getter!(o) == "foo");
            PAssert.That(() => (string?)moDef.GetByName("labelledProperty").Getter!(o) == "bar");

            moDef.GetByName("property").Setter!(ref o, "aha");
            moDef.GetByName("LabelledProperty").Setter!(ref o, "really");

            PAssert.That(() => o.Equals(new SimpleObject { Property = "aha", LabelledProperty = "really" }));
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
    }
}
