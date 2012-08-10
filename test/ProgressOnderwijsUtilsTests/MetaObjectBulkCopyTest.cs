﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Data.Volg.DataSource;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
	[NightlyOnly]
	public sealed class MetaObjectBulkCopyTest : TestSuiteBase
	{
		static readonly BlaOk[] SampleObjects = new[] {
			new BlaOk { Bla = "bl34ga", Bla2 = "blaasdfgasfg2", Id = -1 },
			new BlaOk { Bla = "bla", Bla2 = "bla2", Id = 0 }, 
			new BlaOk { Bla = "dfg", Bla2 = "bla342", Id = 1 }, 
			new BlaOk { Bla = "blfgjha", Bla2 = "  bla2  ", Id = 2 }, 
			new BlaOk { Bla2 = "", Id = 3 }, 
		};

		public sealed class BlaOk : MetaObjectBase<BlaOk>
		{
			public int Id { get; set; }
			public string Bla2 { get; set; }
			public string Bla { get; set; }
		}
		public sealed class BlaOk2 : MetaObjectBase<BlaOk2>
		{
			public int Id { get; set; }
			public string Bla { get; set; }
			public string Bla2 { get; set; }
		}

		public sealed class BlaWithMispelledColumns : MetaObjectBase<BlaWithMispelledColumns>
		{
			public int Idd { get; set; }
			public string Bla { get; set; }
			public string Bla2 { get; set; }
		}

		public sealed class BlaWithMistypedColumns : MetaObjectBase<BlaWithMistypedColumns>
		{
			public int Bla { get; set; }
			public int Id { get; set; }
			public string Bla2 { get; set; }
		}

		public sealed class BlaWithMistypedColumns2 : MetaObjectBase<BlaWithMistypedColumns2>
		{
			public int Bla { get; set; }
			public string Id { get; set; }
			public string Bla2 { get; set; }
		}

		public sealed class BlaWithExtraClrFields : MetaObjectBase<BlaWithExtraClrFields>
		{
			public string ExtraBla { get; set; }
			public string Id { get; set; }
			public int Bla { get; set; }
			public string Bla2 { get; set; }
		}
		public sealed class BlaWithMissingClrFields : MetaObjectBase<BlaWithMissingClrFields>
		{
			public int Id { get; set; }
			public string Bla2 { get; set; }
		}

		[SetUp]
		public void CreateTempTable()
		{
			QueryBuilder.Create(@"create table #MyTable (id int not null primary key, bla nvarchar(max) null, bla2 nvarchar(max) not null)").ExecuteNonQuery(conn);
		}

		[Test]
		public void BulkCopyChecksNames()
		{
			Assert.Throws<InvalidOperationException>(() => MetaObject.SqlBulkCopy(new BlaWithMispelledColumns[0], conn.SqlConnection, "#MyTable"));
		}

		[Test]
		public void BulkCopyChecksTypes()
		{
			Assert.Throws<InvalidOperationException>(() => MetaObject.SqlBulkCopy(new BlaWithMistypedColumns[0], conn.SqlConnection, "#MyTable"));
		}
		[Test]
		public void BulkCopyChecksTypes2()
		{
			Assert.Throws<InvalidOperationException>(() => MetaObject.SqlBulkCopy(new BlaWithMistypedColumns2[0], conn.SqlConnection, "#MyTable"));
		}
		[Test]
		public void BulkCopyVerifiesExistanceOfDestinationColumns()
		{
			Assert.Throws<InvalidOperationException>(() => MetaObject.SqlBulkCopy(new BlaWithExtraClrFields[0], conn.SqlConnection, "#MyTable"));
		}

		[Test]
		public void BulkCopyAllowsExtraDestinationColumns()
		{
			MetaObject.SqlBulkCopy(new BlaWithMissingClrFields[0], conn.SqlConnection, "#MyTable");
		}

		[Test]
		public void BulkCopyAllowsExactMatch()
		{
			MetaObject.SqlBulkCopy(SampleObjects, conn.SqlConnection, "#MyTable");
			var fromDb = QueryBuilder.Create("select * from #MyTable order by Id").ReadObjects<BlaOk>(conn);
			PAssert.That(() => SampleObjects.SequenceEqual(fromDb));
		}
		[Test]
		public void BulkCopySupportsColumnReordering()
		{
			MetaObject.SqlBulkCopy(SampleObjects, conn.SqlConnection, "#MyTable");
			var fromDb = QueryBuilder.Create("select * from #MyTable order by Id").ReadObjects<BlaOk2>(conn);
			PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2 })));
		}
	}
}
