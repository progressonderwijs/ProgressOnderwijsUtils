using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Organisatie.Financieel.Betaling;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Data;
using Progress.Business.GenericLijst;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class TypeExtensionTest
	{
		[Test]
		public void TestNullability()
		{
			PAssert.That(() => typeof(int?).CanBeNull());
			PAssert.That(() => typeof(string).CanBeNull());
			PAssert.That(() => typeof(TypeExtensionTest).CanBeNull());
			PAssert.That(() => typeof(IEnumerable<int>).CanBeNull());
			PAssert.That(() => typeof(ServerContext).CanBeNull());
			PAssert.That(() => !typeof(int).CanBeNull());
			PAssert.That(() => !typeof(Enum).CanBeNull());
			PAssert.That(() => !typeof(DocumentType).CanBeNull());
			PAssert.That(() => !typeof(SelectItem<int?>).CanBeNull());
		}

		[Test]
		public void TestBases()
		{
			PAssert.That(() => typeof(int?).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(string).BaseTypes().SequenceEqual(new[] { typeof(object) }));
			PAssert.That(() => typeof(TypeExtensionTest).BaseTypes().SequenceEqual(new[] { typeof(object) }));
			PAssert.That(() => typeof(IEnumerable<int>).BaseTypes().SequenceEqual(new Type[] { }));
			PAssert.That(() => typeof(int).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(ServerContext).BaseTypes().SequenceEqual(new[] { typeof(object) }));
			PAssert.That(() => typeof(Enum).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(DocumentType).BaseTypes().SequenceEqual(new[] { typeof(Enum), typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(SelectItem<int?>).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
			PAssert.That(() => typeof(ClieopRegelsLijstManager).BaseTypes().SequenceEqual(new[] { typeof(GenericDualLijstManager), typeof(GenericLijstManager), typeof(object) }));
		}
	}
}
