﻿using System;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class MetaObjectBulkCopyTest : TransactedLocalConnection
    {
        static readonly BlaOk[] SampleObjects = new[] {
            new BlaOk { Bla = "bl34ga", Bla2 = "blaasdfgasfg2", Id = -1 },
            new BlaOk { Bla = "bla", Bla2 = "bla2", Id = 0 },
            new BlaOk { Bla = "dfg", Bla2 = "bla342", Id = 1 },
            new BlaOk { Bla = "blfgjha", Bla2 = "  bla2  ", Id = 2 },
            new BlaOk { Bla2 = "", Id = 3 },
        };

        public sealed class BlaOk : ValueBase<BlaOk>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla2 { get; set; }
            public string Bla { get; set; }
        }

        public sealed class BlaOk2 : ValueBase<BlaOk2>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla { get; set; }
            public string Bla2 { get; set; }
        }

        public struct CustomBla
        {
            readonly string value;
            CustomBla(string value)
            {
                this.value = value;
            }
            public string AsString => value;
            [UsedImplicitly]
            public static CustomBla Create(string value) => new CustomBla(value);
        }

        public sealed class BlaOk3 : ValueBase<BlaOk2>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla { get; set; }
            public CustomBla Bla2 { get; set; }
        }

        public sealed class BlaWithMispelledColumns : ValueBase<BlaWithMispelledColumns>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Idd { get; set; }
            public string Bla { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMistypedColumns : ValueBase<BlaWithMistypedColumns>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Bla { get; set; }
            public int Id { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMistypedColumns2 : ValueBase<BlaWithMistypedColumns2>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Bla { get; set; }
            public string Id { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithExtraClrFields : ValueBase<BlaWithExtraClrFields>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public string ExtraBla { get; set; }
            public string Id { get; set; }
            public int Bla { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMissingClrFields : ValueBase<BlaWithMissingClrFields>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla2 { get; set; }
        }

        public void CreateTempTable()
        {
            SQL($@"create table #MyTable (id int not null primary key, bla nvarchar(max) null, bla2 nvarchar(max) not null)").ExecuteNonQuery(Context);
        }

        [Fact]
        public void BulkCopyChecksNames()
        {
            CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithMispelledColumns[0].BulkCopyToSqlServer(Context.Connection, "#MyTable"));
        }

        [Fact]
        public void BulkCopyChecksTypes()
        {
            CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns[0].BulkCopyToSqlServer(Context.Connection, "#MyTable"));
        }

        [Fact]
        public void BulkCopyChecksTypes2()
        {
            CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns2[0].BulkCopyToSqlServer(Context.Connection, "#MyTable"));
        }

        [Fact]
        public void BulkCopyVerifiesExistanceOfDestinationColumns()
        {
            CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithExtraClrFields[0].BulkCopyToSqlServer(Context.Connection, "#MyTable"));
        }

        [Fact]
        public void BulkCopyAllowsExtraDestinationColumns()
        {
            CreateTempTable();
            new BlaWithMissingClrFields[0].BulkCopyToSqlServer(Context.Connection, "#MyTable");
        }

#if NET461
        [Fact]
#else
        [Fact(Skip = "MetaObjectBulkCopy does not have a way to set a transaction that's supported on .NET Core.")]
#endif
        public void BulkCopyAllowsExactMatch()
        {
            CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, "#MyTable");
            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb));
        }

#if NET461
        [Fact]
#else
        [Fact(Skip = "MetaObjectBulkCopy does not have a way to set a transaction that's supported on .NET Core.")]
#endif
        public void BulkCopySupportsColumnReordering()
        {
            CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, "#MyTable");
            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk2>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2 })));
        }

#if NET461
        [Fact]
#else
        [Fact(Skip = "MetaObjectBulkCopy does not have a way to set a transaction that's supported on .NET Core.")]
#endif
        public void MetaObjectSupportsCustomObject()
        {
            CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, "#MyTable");
            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk3>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2.AsString })));
        }
    }
}
