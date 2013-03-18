using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
	[Continuous]
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

		[Test]
		public void QueryBuilderValidation()
		{
			Assert.Throws<ArgumentNullException>(() => QueryBuilder.Create(null));
			Assert.Throws<ArgumentNullException>(() => QueryBuilder.Create(null));
			var abc = (QueryBuilder)"abc";
#pragma warning disable 168
			Assert.Throws<ArgumentNullException>(() => { var _ = abc + default(string); });
			Assert.Throws<ArgumentNullException>(() => { var _ = default(QueryBuilder) + "abc"; });
		}

		[Test]
		public void QueryBuilding()
		{
			PAssert.That(() => QueryBuilder.Empty + QueryBuilder.Create("abc") == (QueryBuilder)"abc");
			Assert.Throws<ArgumentNullException>(() => { var _ = default(QueryBuilder) + QueryBuilder.Create("abc") == (QueryBuilder)"abc"; });
			Assert.Throws<ArgumentNullException>(() => { var _ = default(QueryBuilder) + "abc" == (QueryBuilder)"abc"; });
			Assert.Throws<ArgumentNullException>(() => { var _ = "abc" + default(QueryBuilder) == (QueryBuilder)"abc"; });
		}

		[Test]
		public void EmptyQueryBuilders()
		{
			var qEmpty = QueryBuilder.Empty;
			var qZeroWidth = QueryBuilder.Create("");
			var qZeroWidthArg = QueryBuilder.Create("", 42);
			var qZeroWidth2 = QueryBuilder.Create(42.ToStringInvariant().Substring(42.ToStringInvariant().Length));
			PAssert.That(() => qEmpty != default(QueryBuilder));
			PAssert.That(() => default(QueryBuilder) != qEmpty);
			PAssert.That(() => !(default(QueryBuilder) == qZeroWidth));
			PAssert.That(() => qEmpty == qZeroWidth);
			PAssert.That(() => qEmpty.GetHashCode() == qZeroWidth.GetHashCode());
			PAssert.That(() => qZeroWidth == qZeroWidth2);
			PAssert.That(() => qZeroWidth.GetHashCode() == qZeroWidth2.GetHashCode());
			PAssert.That(() => qZeroWidthArg == qZeroWidth);
			PAssert.That(() => qZeroWidthArg.GetHashCode() == qZeroWidth.GetHashCode());

			PAssert.That(() => QueryBuilder.Create("abc") + qZeroWidth == (QueryBuilder)"abc");
			PAssert.That(() => QueryBuilder.Create("abc") + qZeroWidthArg == QueryBuilder.Create("abc", 42));
			PAssert.That(() => (QueryBuilder.Create("abc") + qZeroWidth).GetHashCode() == ((QueryBuilder)"abc").GetHashCode());
			PAssert.That(() => (QueryBuilder.Create("abc") + qZeroWidthArg).GetHashCode() == QueryBuilder.Create("abc", 42).GetHashCode());
		}
	}
}
