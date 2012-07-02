using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Progress.Business;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class TableValuedParameterTest
	{

		[Test]
		public void DatabaseCanProcessTableValuedParameters()
		{
			DatabaseVersion.DevTestDB.ExecuteNonQuery(conn => {

				                                                  QueryBuilder q = @"select sum(val) from " + QueryBuilder.TableParam(Enumerable.Range(1, 100));
				                                                  int sum = q.ExecuteScalar<int>(conn);

				                                                  Assert.That(sum, Is.EqualTo((100 * 100 + 100) / 2));

			});
		}

		[Test]
		public void QueryBuildersCanIncludeTvps()
		{
			QueryBuilder q = QueryBuilder.Create(@"select sum(val) from {0}", Enumerable.Range(1, 100));

			DatabaseVersion.DevTestDB.ExecuteNonQuery(conn => {
				                                                  int sum = q.ExecuteScalar<int>(conn);

				                                                  Assert.That(sum, Is.EqualTo((100 * 100 + 100) / 2));

			});
		}

	}
}
