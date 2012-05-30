using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public sealed class QueryBuilderTest
	{
		[Test]
		public void EqualsIgnoresComponentBoundaries()
		{
			QueryBuilder
				a = QueryBuilder.Create("a"),
				b = QueryBuilder.Create("b"),
				c = QueryBuilder.Create("c"),
				ab = QueryBuilder.Create("ab"),
				bc = QueryBuilder.Create("bc"),
				abc = QueryBuilder.Create("abc");

			PAssert.That(() => a + b == ab);
			PAssert.That(() => a + bc == ab + c);
			PAssert.That(() => a + (b + c) == (a + b) + c);
			PAssert.That(() => a + b + c == abc);
		}
		[Test]
		public void EqualsRecurses()
		{
			QueryBuilder
				a = QueryBuilder.Create("a"),
				b = QueryBuilder.Create("b"),
				c = QueryBuilder.Create("c");

			PAssert.That(() => (a + (a + c)) + b == (a + a) + (c + b));
			PAssert.That(() => (a + ((a + a) + c)) + b != a + a + c + b);
		}
	}
}
