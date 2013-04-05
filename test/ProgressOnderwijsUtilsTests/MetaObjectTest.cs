using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;
using NUnit.Framework;
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
		[MpNotMapped]
		public string IgnoredProperty { get; set; }
		[Hide]
		public string HiddenProperty { get; set; }
		[MpLabel("bla", "bla")]
		public string LabelledProperty { get; set; }
		[MpReadonly]
		public string MpReadonlyProperty { get; set; }
		string PrivateProperty { get; set; }
#pragma warning disable 169
		DateTime PrivateField;
#pragma warning restore 169
#pragma warning disable 649
		public readonly double ReadonlyField;
#pragma warning restore 649
		public double ReadonlyProperty { get { return 0.0; } }
		public char WriteonlyProperty { set { } }
		public object PrivateSetter { get; private set; }
		public object PrivateGetter { set; private get; }
	}

	[Continuous]
	public sealed class MetaObjectTest
	{
		[Test]
		public void MetaObjectsAreAbstractOrNotInherited()
		{
			var metaObjectTypes =
				from assembly in ClassNameConflicts.ProgressAssemblies
				from type in assembly.GetTypes()
				where typeof(IMetaObject).IsAssignableFrom(type)
				select type;

			var typesWithNonAbstractBaseMetaObjects = metaObjectTypes.Where(type => !type.IsAbstract && type.BaseTypes().Any(baseT => !baseT.IsAbstract && typeof(IMetaObject).IsAssignableFrom(baseT)));

			PAssert.That(() => !typesWithNonAbstractBaseMetaObjects.Any(),
				"MetaObject types must not be inherited (unless they're abstract).  Reason: metaproperties can be resolved using ANY of the concrete types of the metaobject, so that inheritance will cause subclass instances' properties to be omitted."
				);

		}

		[Test]
		public void ReturnsSameMetaProperties()
		{
			var mps = MetaObject.GetMetaProperties<SimpleObject>();
			var mpsAlt = new SimpleObject().GetMetaProperties().Properties;
			PAssert.That(() => mps.SequenceEqual(mpsAlt));
		}


		[Test]
		public void EnumeratesAsExpected()
		{
			var mps = MetaObject.GetMetaProperties<SimpleObject>();
			var names = mps.Select(mp => mp.Name);
			var expected = new[] { "Property", "HiddenProperty", "LabelledProperty", "ReadonlyProperty", "WriteonlyProperty", "PrivateSetter", "PrivateGetter", };
			PAssert.That(() => names.SequenceEqual(expected));
		}

		[Test]
		public void IsReadable()
		{
			var readable = MetaObject.GetMetaProperties<SimpleObject>().Where(mp => mp.CanRead);
			var expected = new[] { "Property", "HiddenProperty", "LabelledProperty", "ReadonlyProperty", "PrivateSetter" };
			PAssert.That(() => readable.Select(mp => mp.Name).SequenceEqual(expected));
		}

		[Test]
		public void IsWritable()
		{
			var writable = MetaObject.GetMetaProperties<SimpleObject>().Where(mp => mp.CanWrite);
			var expected = new[] { "Property", "HiddenProperty", "LabelledProperty", "WriteonlyProperty", "PrivateGetter", };
			PAssert.That(() => writable.Select(mp => mp.Name).SequenceEqual(expected));
		}


		[Test]
		public void CanSetAndGet()
		{
			var o = new SimpleObject { Property = "foo", LabelledProperty = "bar" };
			var moDef = MetaObject.GetMetaProperties<SimpleObject>();
			PAssert.That(() => (string)moDef["Property"].Getter(o) == "foo");
			PAssert.That(() => (string)moDef["labelledProperty"].Getter(o) == "bar");

			moDef["property"].Setter(o, "aha");
			moDef["LabelledProperty"].Setter(o, "really");

			PAssert.That(() => o.Equals(new SimpleObject { Property = "aha", LabelledProperty = "really" }) );
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
			PAssert.That(() => readonlyPropertyMp.IsReadonly && !readonlyPropertyMp.CanWrite);
			var mpReadonlyPropertyMp = MetaObject.GetByExpression((SimpleObject o) => o.MpReadonlyProperty);
			PAssert.That(() => mpReadonlyPropertyMp.IsReadonly && mpReadonlyPropertyMp.CanWrite);
			var propertyMp = MetaObject.GetByExpression((SimpleObject o) => o.Property);
			PAssert.That(() => !propertyMp.IsReadonly && propertyMp.CanWrite);
		}



	}
}
