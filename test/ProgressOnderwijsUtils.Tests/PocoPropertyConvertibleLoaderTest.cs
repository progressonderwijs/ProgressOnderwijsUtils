﻿using System;
using System.Linq;
using ExpressionToCodeLib;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProgressOnderwijsUtils.Tests.Data;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class PocoPropertyConvertibleLoaderTest : TransactedLocalConnection
    {
        static readonly BlaOk[] SampleObjects = {
            new BlaOk { Bla = "bl34ga", Bla2 = "blaasdfgasfg2", Id = -1 },
            new BlaOk { Bla = "bla", Bla2 = "bla2", Id = 0 },
            new BlaOk { Bla = "dfg", Bla2 = "bla342", Id = 1 },
            new BlaOk { Bla = "blfgjha", Bla2 = "  bla2  ", Id = 2 },
            new BlaOk { Bla2 = "", Id = 3 },
        };

        public sealed class BlaOk : ValueBase<BlaOk>, IWrittenImplicitly, IReadImplicitly
        {
            public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
            public string Bla2 { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
            public string? Bla { get; set; }
        }

        public struct CustomBla : IPocoConvertibleProperty<CustomBla, string, CustomBla.Source>
        {
            public struct Source : IConverterSource<CustomBla, string>
            {
                public ValueConverter<CustomBla, string> GetValueConverter()
                    => this.DefineConverter(o => o.AsString, s => new CustomBla(s));
            }

            public CustomBla(string value)
                => AsString = value;

            public string AsString { get; }
        }

        public sealed class BlaOk3 : ValueBase<BlaOk3>, IWrittenImplicitly, IReadImplicitly
        {
            public CustomBla Bla2 { get; set; }
        }

        public sealed class BlaOk4 : ValueBase<BlaOk4>, IWrittenImplicitly, IReadImplicitly
        {
            public int Id { get; set; }
            public string? Bla { get; set; }
            public CustomBla Bla2 { get; set; }
        }

        public sealed class BlaOk5 : ValueBase<BlaOk5>, IWrittenImplicitly, IReadImplicitly
        {
            public int Id { get; set; }
            public string? Bla { get; set; }
            public CustomBla Bla2 { get; set; }
            public CustomBla? Bla3 { get; }
        }

        public sealed class BlaOk_with_struct_property : ValueBase<BlaOk_with_struct_property>, IWrittenImplicitly, IReadImplicitly
        {
            public int Id { get; set; }
            public string? Bla { get; set; }
            public TrivialValue<string> Bla2 { get; set; }
        }

        public sealed class BlaOk_with_nullable_struct_property : ValueBase<BlaOk_with_nullable_struct_property>, IWrittenImplicitly, IReadImplicitly
        {
            public TrivialValue<int> Id { get; set; }
            public TrivialValue<string>? Bla { get; set; }
            public TrivialValue<string> Bla2 { get; set; }
        }

        BulkInsertTarget CreateTempTable()
        {
            var tableName = SQL($"#MyTable");
            SQL($@"
                create table {tableName} (
                    id int not null primary key
                    , bla nvarchar(max) null
                    , bla2 nvarchar(max) not null
                )
            ").ExecuteNonQuery(Connection);
            return BulkInsertTarget.LoadFromTable(Connection, tableName);
        }

        [Fact]
        public void PocoSupportsCustomObject_only_one_property()
        {
            PAssert.That(() => new CustomBla("aap").AsString == "aap");

            var target = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Connection, target);

            var fromDb = SQL($"select Bla2 from #MyTable order by Id").ReadPocos<BlaOk3>(Connection);
            PAssert.That(() => SampleObjects.Select(s => s.Bla2).SequenceEqual(fromDb.Select(x => x.Bla2.AsString)));
        }

        [Fact]
        public void PocoSupportsCustomObject_multiple_properties()
        {
            PAssert.That(() => new CustomBla("aap").AsString == "aap");

            var target = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Connection, target);

            var fromDb = SQL($"select * from #MyTable order by Id").ReadPocos<BlaOk4>(Connection);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2.AsString })));
        }

        [Fact]
        public void PocoSupportsCustomObject_readonly()
        {
            PAssert.That(() => new CustomBla("aap").AsString == "aap");

            var target = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Connection, target);

            var fromDb = SQL($"select * from #MyTable order by Id").ReadPocos<BlaOk5>(Connection);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2.AsString })));
            PAssert.That(() => fromDb.All(x => x.Bla3 == null));
        }

        [Fact]
        public void PocoSupportsCustomObject_struct()
        {
            PAssert.That(() => new TrivialValue<string>("aap").Value == "aap");

            var target = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Connection, target);

            var fromDb = SQL($"select Id, Bla, Bla2 from #MyTable order by Id").ReadPocos<BlaOk_with_struct_property>(Connection);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2.Value })));
        }

        [Fact]
        public void PocoSupportsCustomObject_nullable_struct()
        {
            PAssert.That(() => new TrivialValue<string>("aap").Value == "aap");

            var target = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Connection, target);

            var fromDb = SQL($"select Id, Bla, Bla2 from #MyTable order by Id").ReadPocos<BlaOk_with_nullable_struct_property>(Connection);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id.Value, Bla = x.Bla.HasValue ? x.Bla.Value.Value : default(string), Bla2 = x.Bla2.Value })));
        }

        [Fact]
        public void PocoSupportsCustomObject_nonnullable_struct_with_null_values_throws_exception_with_helpful_message()
        {
            PAssert.That(() => new TrivialValue<string>("aap").Value == "aap");

            var target = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Connection, target);

            var ex = Assert.ThrowsAny<Exception>(() => SQL($"select Id, Bla, Bla2 = cast(null as varchar) from #MyTable order by Id").ReadPocos<BlaOk_with_struct_property>(Connection));
            PAssert.That(() => ex.Message.Contains("Cannot unpack NULL value from column Bla2", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Query_errors_unrelated_to_column_mapping_are_not_misleading()
        {
            PAssert.That(() => new TrivialValue<string>("aap").Value == "aap");

            var target = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Connection, target);

            var ex = Assert.ThrowsAny<Exception>(() => SQL($"select Id = (select Id from #MyTable), Bla, Bla2 from #MyTable order by Id").ReadPocos<BlaOk_with_struct_property>(Connection));
            Assert.DoesNotContain("column", ex.Message);
            Assert.Contains("Subquery returned more than 1 value", ex.InnerException.Message);
        }

        [Fact]
        public void PocoSupportsCustomObject_ReadPlain()
        {
            PAssert.That(() => new CustomBla("aap").AsString == "aap");
            var target = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Connection, target);

            var fromDb = SQL($"select Bla2 from #MyTable order by Id").ReadPlain<CustomBla>(Connection);

            PAssert.That(() => fromDb.Select(db => db.AsString).SequenceEqual(SampleObjects.Select(obj => obj.Bla2)));
        }

        [Fact]
        public void PocoSupportsCustomObject_ReadScalar()
        {
            PAssert.That(() => new CustomBla("aap").AsString == "aap");
            var target = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Connection, target);

            var fromDb = SQL($"select top(1) Bla2 from #MyTable order by Id").ReadScalar<CustomBla>(Connection);

            PAssert.That(() => fromDb.AsString == SampleObjects.First().Bla2);
        }
    }
}
