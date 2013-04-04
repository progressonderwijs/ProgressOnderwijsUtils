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

	public sealed class SimpleObject : IMetaObject
	{
		public int Field;
		public string Property { get; set; }
		[MpNotMapped]
		public string IgnoredProperty { get; set; }
		[Hide]
		public string HiddenProperty { get; set; }
		[MpLabel("bla", "bla")]
		public string LabelledProperty { get; set; }
		string PrivateProperty { get; set; }
		DateTime PrivateField;
		public readonly double ReadonlyField;
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
			var mpsAlt = new SimpleObject().GetMetaProperties();
			PAssert.That(() => mps.SequenceEqual(mpsAlt));
		}


		[Test]
		public void EnumeratesAsExpected()
		{
			var mps = MetaObject.GetMetaProperties<SimpleObject>();
			var names = mps.Select(mp => mp.Name);
			var expected = new[] { "Field", "Property", "HiddenProperty", "LabelledProperty", "ReadonlyField", "ReadonlyProperty", "WriteonlyProperty", "PrivateSetter", "PrivateGetter", };
			PAssert.That(() => names.SequenceEqual(expected));
		}


	}
}
