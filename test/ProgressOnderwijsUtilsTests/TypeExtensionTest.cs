using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.AppVersion;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
	public sealed class TypeExtensionTest
	{
		[Test]
		public void TestNullability()
		{
			PAssert.That(() => typeof(int?).CanBeNull());
			PAssert.That(() => typeof(string).CanBeNull());
			PAssert.That(() => typeof(TypeExtensionTest).CanBeNull());
			PAssert.That(() => typeof(IEnumerable<int>).CanBeNull());
			PAssert.That(() => typeof(ServerLocationAuto).CanBeNull());
			PAssert.That(() => !typeof(int).CanBeNull());

			PAssert.That(() => !typeof(DocumentType).CanBeNull());
			PAssert.That(() => !typeof(SelectItem<int?>).CanBeNull());


			PAssert.That(() => typeof(Enum).CanBeNull());//WTF???? maar goed, dit is dan ook wel een heel gek type.
		}

		class SampleBase : ArrayList { }
		class Sample<A> : SampleBase { }
		class SampleX<A> : Sample<A> { }

		[Test]
		public void TestBases()
		{
			PAssert.That(() => typeof(int?).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(string).BaseTypes().SequenceEqual(new[] { typeof(object) }));
			PAssert.That(() => typeof(TypeExtensionTest).BaseTypes().SequenceEqual(new[] { typeof(object) }));
			PAssert.That(() => typeof(IEnumerable<int>).BaseTypes().SequenceEqual(new Type[] { }));
			PAssert.That(() => typeof(int).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(ServerLocationAuto).BaseTypes().SequenceEqual(new[] { typeof(object) }));
			PAssert.That(() => typeof(Enum).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(DocumentType).BaseTypes().SequenceEqual(new[] { typeof(Enum), typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(SelectItem<int?>).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(SampleX<string>).BaseTypes().SequenceEqual(new[] { typeof(Sample<string>), typeof(SampleBase), typeof(ArrayList), typeof(object) }));
		}

		[Test]
		public void TestNullableGetter()
		{
			PAssert.That(() => typeof(int?).IfNullableGetNonNullableType() == typeof(int));
			PAssert.That(() => typeof(int).IfNullableGetNonNullableType() == null);
			PAssert.That(() => typeof(string).IfNullableGetNonNullableType() == null);
		}

		[Test]
		public void TestNonGenericName()
		{
			PAssert.That(() => typeof(int?).GetNonGenericName() == "System.Nullable");
			PAssert.That(() => typeof(int).GetNonGenericName() == "System.Int32");
			PAssert.That(() => typeof(string).GetNonGenericName() == "System.String");
			PAssert.That(() => typeof(Func<Tuple<int, string>, bool>).GetNonGenericName() == "System.Func");
		}

	}
}
