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

		struct SqlObject : IMetaObject
		{
			public string Type { get; set; }
			public string Name { get; set; }
			public DateTime ModifyDate { get; set; }
			public DateTime ExpectedModifyDate { get; set; }

			#region Overrides of ValueType

			public override string ToString()
			{
				return string.Format("{0}: type={1}, name={2}, modifyDate={3}, expectedModifyDate={4}",
					base.ToString(), Type, Name, ModifyDate, ExpectedModifyDate);
			}

			#endregion
		}

		[Test]
		public void AlleSqlObjectenInVersiebeheer()
		{
			const string q = @"
				declare
				  @l_date date

				select top ( 1 )
				  @l_date = cast( o.modify_date as date )
				from sys.objects o 
				join sys.sql_modules m on m.object_id = o.object_id
				where o.is_ms_shipped = 0
				  and o.name not like 'sp@_%' escape '@' -- Microsoft functionaliteit voor database diagrams
				  and o.name not like 'fn@_%' escape '@' -- Microsoft functionaliteit voor database diagrams

				select  type = o.type_desc,
					o.name,
					modifydate = cast( o.modify_date as date ),
					expectedmodifydate = @l_date
				from sys.objects o 
				join sys.sql_modules m on m.object_id = o.object_id
				where o.is_ms_shipped = 0
				  and cast( o.modify_date as date ) != @l_date
				  and o.name not like 'sp@_%' escape '@' -- Microsoft functionaliteit voor database diagrams
				  and o.name not like 'fn@_%' escape '@' -- Microsoft functionaliteit voor database diagrams
				";
			Assert.That(QueryBuilder.Create(q).ReadMetaObjects<SqlObject>(conn), Is.Empty);
		}
	}
}
