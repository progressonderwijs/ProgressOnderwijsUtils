using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	public sealed class DatabaseSanityTest : TestSuiteBase
	{
		[Test]
		public void OntbrekendeForeignKeyIndexes()
		{
			const string q = @"
				select
					'Geen index op kolom ['  + c.name + '] in tabel [' + s.name + '.' + pt.name + '], gebruikt in [' + k.name
				from sys.foreign_keys k
				join sys.foreign_key_columns kc on kc.constraint_object_id = k.object_id
				join sys.tables pt on pt.object_id = kc.parent_object_id
				join sys.columns c on c.object_id = kc.parent_object_id and c.column_id = kc.parent_column_id
				join sys.schemas s on s.schema_id = pt.schema_id
				left join sys.index_columns ic on ic.object_id = kc.parent_object_id and ic.column_id = kc.parent_column_id
				where ic.object_id is null
				";

			Assert.That(QueryBuilder.Create(q).ReadPlain<string>(conn), Is.Empty, "Geen index op FK-kolom.");
		}
	}
}
