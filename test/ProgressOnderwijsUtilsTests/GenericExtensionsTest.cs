using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class GenericExtensionsTest
	{
		[Test]
		public void InStruct()
		{
			PAssert.That(() => DatabaseVersion.DevTestDB.In(DatabaseVersion.ProductieDB, DatabaseVersion.DevTestDB));
			PAssert.That(() => !DatabaseVersion.DevTestDB.In(DatabaseVersion.ProductieDB, DatabaseVersion.TestDB));
			PAssert.That(() => !default(DatabaseVersion?).In(DatabaseVersion.ProductieDB, DatabaseVersion.TestDB));
			PAssert.That(() => default(DatabaseVersion?).In(DatabaseVersion.ProductieDB, DatabaseVersion.TestDB, null));

			PAssert.That(() => 3.In(1, 2, 3));
			PAssert.That(() => !3.In(1, 2, 4, 8));
		}
	}
}
