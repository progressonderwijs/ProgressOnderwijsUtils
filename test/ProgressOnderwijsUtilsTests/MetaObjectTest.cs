using System;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Test.CodeStyle;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    public interface ISimpleInterface
    {
        string Property { get; set; }
    }

    public sealed class SimpleObject : ValueBase<SimpleObject>, IMetaObject, ISimpleInterface
    {
        public int Field;
        public string Property { get; set; }

        internal string IgnoredProperty { get; set; }

        [Hide]
        public string HiddenProperty { get; set; }

        [MpLabel("bla", "bla")]
        public string LabelledProperty { get; set; }

        [MpReadonly]
        public string MpReadonlyProperty { get; set; }

        string PrivateProperty { get; }
#pragma warning disable 169
        DateTime PrivateField;
#pragma warning restore 169
#pragma warning disable 649
        public readonly double ReadonlyField;
#pragma warning restore 649
        public double ReadonlyProperty => 0.0;

        public char WriteonlyProperty
        {
            set { }
        }

        public object PrivateSetter { get; }
        public object PrivateGetter { set; private get; }
    }

    struct SetterTestStruct : IMetaObject
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }
    }

    class SetterTestClass : IMetaObject
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }
    }

    [Continuous]
    public sealed class MetaObjectTest
    {
        [Test]
        public void MetaObjectsAreAbstractOrNotInherited()
        {
            var metaObjectTypes =
                from assembly in ProgressAssemblies.All
                from type in assembly.GetTypes()
                where typeof(IMetaObject).IsAssignableFrom(type)
                select type;

            var typesWithNonAbstractBaseMetaObjects =
                metaObjectTypes.Where(type => !type.IsAbstract && type.BaseTypes().Any(baseT => !baseT.IsAbstract && typeof(IMetaObject).IsAssignableFrom(baseT)));

            PAssert.That(
                () => !typesWithNonAbstractBaseMetaObjects.Any(),
                "MetaObject types must not be inherited (unless they're abstract).  Reason: metaproperties can be resolved using ANY of the concrete types of the metaobject, so that inheritance can cause subclass instances' properties to be omitted unpredictably."
                );
        }

        [Test]
        public void ReturnsSameMetaProperties()
        {
            var mps = MetaObject.GetMetaProperties<SimpleObject>();
            var mpsAlt = new SimpleObject().GetMetaProperties();
            PAssert.That(() => mps.SequenceEqual(mpsAlt));
        }

        [Test]
        public void EnumeratesAsExpected()
        {
            var mps = MetaObject.GetMetaProperties<SimpleObject>();
            var names = mps.Select(mp => mp.Name);
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

        [Test]
        public void IsReadable()
        {
            var readable = MetaObject.GetMetaProperties<SimpleObject>().Where(mp => mp.CanRead);
            var expected = new[] { "Property", "HiddenProperty", "LabelledProperty", "MpReadonlyProperty", "ReadonlyProperty", "PrivateSetter" };
            PAssert.That(() => readable.Select(mp => mp.Name).SequenceEqual(expected));
        }

        [Test]
        public void IsWritable()
        {
            var writable = MetaObject.GetMetaProperties<SimpleObject>().Where(mp => mp.CanWrite);
            var expected = new[] { "Property", "HiddenProperty", "LabelledProperty", "MpReadonlyProperty", "WriteonlyProperty", "PrivateGetter", };
            PAssert.That(() => writable.Select(mp => mp.Name).SequenceEqual(expected));
        }

        [Test]
        public void CanSetAndGet()
        {
            var o = new SimpleObject { Property = "foo", LabelledProperty = "bar" };
            var moDef = MetaObject.GetMetaProperties<SimpleObject>();
            PAssert.That(() => (string)moDef.GetByName("Property").Getter(o) == "foo");
            PAssert.That(() => (string)moDef.GetByName("labelledProperty").Getter(o) == "bar");

            moDef.GetByName("property").Setter(ref o, "aha");
            moDef.GetByName("LabelledProperty").Setter(ref o, "really");

            PAssert.That(() => o.Equals(new SimpleObject { Property = "aha", LabelledProperty = "really" }));
        }

        [Test]
        public void CanGetByExpression()
        {
            var mp = MetaObject.GetByExpression((SimpleObject o) => o.Property);
            PAssert.That(() => mp.Name == "Property" && mp.DataType == typeof(string));
        }

        [Test]
        public void ReadonlyWorks()
        {
            var readonlyPropertyMp = MetaObject.GetByExpression((SimpleObject o) => o.ReadonlyProperty);
            PAssert.That(() => readonlyPropertyMp.ExtraMetaData().IsReadonly && !readonlyPropertyMp.CanWrite);
            var mpReadonlyPropertyMp = MetaObject.GetByExpression((SimpleObject o) => o.MpReadonlyProperty);
            PAssert.That(() => mpReadonlyPropertyMp.ExtraMetaData().IsReadonly && mpReadonlyPropertyMp.CanWrite);
            var propertyMp = MetaObject.GetByExpression((SimpleObject o) => o.Property);
            PAssert.That(() => !propertyMp.ExtraMetaData().IsReadonly && propertyMp.CanWrite);
        }

        [Test]
        public void CanReadWrite_ValueTypedProperty_On_ValueTypeObject()
        {
            var obj = new SetterTestStruct();
            var prop = MetaObject.GetByExpression((SetterTestStruct o) => o.IntProperty);

            PAssert.That(() => (int)prop.Getter(obj) == 0);
            prop.Setter(ref obj, 42);
            PAssert.That(() => (int)prop.Getter(obj) == 42);
        }

        [Test]
        public void CanReadWrite_ReferenceTypedProperty_On_ValueTypeObject()
        {
            var obj = new SetterTestStruct();
            var prop = MetaObject.GetByExpression((SetterTestStruct o) => o.StringProperty);

            PAssert.That(() => prop.Getter(obj) == null);
            prop.Setter(ref obj, "42");
            PAssert.That(() => (string)prop.Getter(obj) == "42");
        }

        [Test]
        public void CanReadWrite_ValueTypedProperty_On_ReferenceTypeObject()
        {
            var obj = new SetterTestClass();
            var prop = MetaObject.GetByExpression((SetterTestClass o) => o.IntProperty);

            PAssert.That(() => (int)prop.Getter(obj) == 0);
            prop.Setter(ref obj, 42);
            PAssert.That(() => (int)prop.Getter(obj) == 42);
        }

        [Test]
        public void CanReadWrite_ReferenceTypedProperty_On_ReferenceTypeObject()
        {
            var obj = new SetterTestClass();
            var prop = MetaObject.GetByExpression((SetterTestClass o) => o.StringProperty);

            PAssert.That(() => prop.Getter(obj) == null);
            prop.Setter(ref obj, "42");
            PAssert.That(() => (string)prop.Getter(obj) == "42");
        }
    }
}
