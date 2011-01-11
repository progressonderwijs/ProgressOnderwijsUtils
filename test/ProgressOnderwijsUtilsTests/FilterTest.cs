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
	public class FilterTest
	{
		[Test]
		public void BasicChecks()
		{
			PAssert.That(() => Filter.ToSqlString(null, default(Dictionary<string, string>)) == QueryBuilder.Create("1=1"));//shouldn't throw and should be equal
			PAssert.That(() => Filter.CreateCriterium("test",BooleanComparer.LessThan,3).ToSqlString(s=>s) == QueryBuilder.Create("test<{0}",3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThanOrEqual, 3).ToSqlString(s => s) == QueryBuilder.Create("test<={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, 3).ToSqlString(s => s) == QueryBuilder.Create("test={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.GreaterThanOrEqual, 3).ToSqlString(s => s) == QueryBuilder.Create("test>={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.GreaterThan, 3).ToSqlString(s => s) == QueryBuilder.Create("test>{0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.NotEqual, 3).ToSqlString(s => s) == QueryBuilder.Create("test!={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Contains, "world").ToSqlString(s => s) == QueryBuilder.Create("test like {0}", "%world%"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.StartsWith, "world").ToSqlString(s => s) == QueryBuilder.Create("test like {0}", "world%"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.IsNull, null).ToSqlString(s => s) == QueryBuilder.Create("test is null"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.IsNotNull, null).ToSqlString(s => s) == QueryBuilder.Create("test is not null"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.In, "1 2 3 4 5 ").ToSqlString(s => s) == QueryBuilder.Create("test in ({0}, {1}, {2}, {3}, {4})", Enumerable.Range(1,5).Select(i=>i.ToString()).Cast<object>().ToArray()));
		}
	}
}
