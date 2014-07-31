using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
	[Continuous]
	public sealed class DatabaseSanityTest : TestSuiteBase
	{
		[Test]
		public void OntbrekendeForeignKeyIndexes()
		{
			const string q = @"
				select
					'Geen index op kolom ['  + c.name + '] in tabel [' + s.name + '.' + t.name + '], gebruikt in [' + k.name
				from sys.foreign_keys k
				join sys.foreign_key_columns kc on kc.constraint_object_id = k.object_id
				join sys.tables pt on pt.object_id = kc.parent_object_id
				join sys.columns c on c.object_id = kc.parent_object_id and c.column_id = kc.parent_column_id
				join sys.schemas s on s.schema_id = pt.schema_id
				left join sys.index_columns ic on ic.object_id = kc.parent_object_id and ic.column_id = kc.parent_column_id
				where ic.object_id is null
				";

			var sb = new StringBuilder();
			QueryBuilder.Create(q).ReadPlain<string>(conn).ForEach(i => sb.AppendLine(i));
			Assert.That(sb.ToString(), Is.EqualTo(string.Empty), "Geen index op FK-kolom.");
		}

		[Test]
		public void OntbrekendeForeignKeys()
		{
			const string q = @"
				select 
					'Foreign key lijkt te ontbreken voor tabel [' 
					+ sp.name + '.' + tp.name + '], kolom [' + cp.name + '] naar tabel '
					+ sc.name + '.' + tc.name + '], kolom [' + cc.name + ']'
				from sys.tables tp
				join sys.columns cp on cp.object_id = tp.object_id
				join sys.columns cc on cc.name = cp.name + 'id' and cc.object_id != cp.object_id and cc.system_type_id = cp.system_type_id
				join sys.tables tc on cc.object_id = tc.object_id
				join sys.schemas sp on sp.schema_id = tp.schema_id
				join sys.schemas sc on sc.schema_id = tc.schema_id
				left join sys.foreign_key_columns fkcu on fkcu.parent_object_id = tp.object_id
														and fkcu.parent_column_id = cp.column_id
				left join sys.foreign_key_columns fkc on fkc.parent_object_id = tp.object_id
														and fkc.parent_column_id = cp.column_id
														and fkc.referenced_object_id = tc.object_id
														and fkc.referenced_column_id = cc.column_id
				left join sys.foreign_keys fk on fk.object_id = fkc.constraint_object_id
				where tc.name like '%' + substring( cc.name, 1, len(cc.name) - 2 )
				  and fkc.constraint_object_id is null
				  and fkcu.constraint_object_id is null -- fkcu : als de kolom al een relatie heeft aannemen dat dit de juiste is
				  and tp.name not like 'weg%'
				  and tp.name not like 'tmp%'
				  and tp.name not like 'fb%'
				  and tp.name not like 'temp%'
				  and tc.name not like 'conversie%'
				  and tc.name not like 'weg%'
				  and tc.name not like 'tmp%'
				  and tc.name not like 'fb%'
				  and tc.name not like 'temp%'
				  and tc.name not like 'conversie%'
				";

			var sb = new StringBuilder();
			QueryBuilder.Create(q).ReadPlain<string>(conn).ForEach(i => sb.AppendLine(i));
			Assert.That(sb.ToString(), Is.EqualTo(string.Empty), "Ontbrekende foreign key relatie.");
		}
	}
}
