using System;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class MetaObjectPropertyLoaderTest : TransactedLocalConnection
    {
        static readonly BlaOk[] SampleObjects = {
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

        public sealed class CustomBla
        {
            CustomBla(string value)
            {
                AsString = value;
            }

            public string AsString { get; }

            [NotNull]
            [MetaObjectPropertyLoader]
            public static CustomBla MethodWithIrrelevantName(string value) => new CustomBla(value);
        }

        public struct CustomBlaStruct
        {
            CustomBlaStruct(string value)
            {
                AsString = value;
            }

            public string AsString { get; }

            [MetaObjectPropertyLoader]
            public static CustomBlaStruct MethodWithIrrelevantName(string value) => new CustomBlaStruct(value);
        }

        public sealed class BlaOk3 : ValueBase<BlaOk3>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public CustomBla Bla2 { get; set; }
        }

        public sealed class BlaOk4 : ValueBase<BlaOk4>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla { get; set; }
            public CustomBla Bla2 { get; set; }
        }

        public sealed class BlaOk5 : ValueBase<BlaOk5>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla { get; set; }
            public CustomBla Bla2 { get; set; }
            public CustomBla Bla3 { get; }
        }

        public sealed class BlaOk_with_struct_property : ValueBase<BlaOk_with_struct_property>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla { get; set; }
            public CustomBlaStruct Bla2 { get; set; }
        }

        public sealed class BlaOk_with_nullable_struct_property : ValueBase<BlaOk_with_nullable_struct_property>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public CustomBlaStruct? Bla { get; set; }
            public CustomBlaStruct Bla2 { get; set; }
        }

        (string tableName, DbColumnMetaData[] columns) CreateTempTable()
        {
            var tableName = SQL($"#MyTable");
            SQL($@"
                create table {tableName} (
                    id int not null primary key
                    , bla nvarchar(max) null
                    , bla2 nvarchar(max) not null
                )
            ").ExecuteNonQuery(Context);

            return (tableName.CommandText(), DbColumnMetaData.ColumnMetaDatas(Context.Connection, tableName));
        }

        [Fact]
        public void MetaObjectSupportsCustomObject_only_one_property()
        {
            PAssert.That(() => CustomBla.MethodWithIrrelevantName("aap").AsString == "aap");
            PAssert.That(() => default(CustomBla) == null);

            var (table, columns) = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, table, columns);

            var fromDb = SQL($"select Bla2 from #MyTable order by Id").ReadMetaObjects<BlaOk3>(Context);
            PAssert.That(() => SampleObjects.Select(s => s.Bla2).SequenceEqual(fromDb.Select(x => x.Bla2.AsString)));
        }

        [Fact]
        public void MetaObjectSupportsCustomObject_multiple_properties()
        {
            PAssert.That(() => CustomBla.MethodWithIrrelevantName("aap").AsString == "aap");
            PAssert.That(() => default(CustomBla) == null);

            var (table, columns) = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, table, columns);

            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk4>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2.AsString })));
        }

        [Fact]
        public void MetaObjectSupportsCustomObject_readonly()
        {
            PAssert.That(() => CustomBla.MethodWithIrrelevantName("aap").AsString == "aap");
            PAssert.That(() => default(CustomBla) == null);

            var (table, columns) = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, table, columns);

            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk5>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2.AsString })));
            PAssert.That(() => fromDb.All(x => x.Bla3 == null));
        }

        [Fact]
        public void MetaObjectSupportsCustomObject_struct()
        {
            PAssert.That(() => CustomBlaStruct.MethodWithIrrelevantName("aap").AsString == "aap");

            var (table, columns) = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, table, columns);

            var fromDb = SQL($"select Id, Bla, Bla2 from #MyTable order by Id").ReadMetaObjects<BlaOk_with_struct_property>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2.AsString })));
        }

        [Fact]
        public void MetaObjectSupportsCustomObject_nullable_struct()
        {
            PAssert.That(() => CustomBlaStruct.MethodWithIrrelevantName("aap").AsString == "aap");

            var (table, columns) = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, table, columns);

            var fromDb = SQL($"select Id, Bla, Bla2 from #MyTable order by Id").ReadMetaObjects<BlaOk_with_nullable_struct_property>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla.HasValue ? x.Bla.Value.AsString : default(string), Bla2 = x.Bla2.AsString })));
        }

        [Fact]
        public void MetaObjectSupportsCustomObject_nonnullable_struct_with_null_values_throws_exception_with_helpful_message()
        {
            PAssert.That(() => CustomBlaStruct.MethodWithIrrelevantName("aap").AsString == "aap");

            var (table, columns) = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, table, columns);

            var ex = Assert.ThrowsAny<Exception>(() => SQL($"select Id, Bla, Bla2 = cast(null as varchar) from #MyTable order by Id").ReadMetaObjects<BlaOk_with_struct_property>(Context));
            PAssert.That(() => ex.Message.Contains("Cannot unpack NULL value from column Bla2", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Query_errors_unrelated_to_column_mapping_are_not_misleading()
        {
            PAssert.That(() => CustomBlaStruct.MethodWithIrrelevantName("aap").AsString == "aap");

            var (table, columns) = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, table, columns);

            var ex = Assert.ThrowsAny<Exception>(() => SQL($"select Id = (select Id from #MyTable), Bla, Bla2 from #MyTable order by Id").ReadMetaObjects<BlaOk_with_struct_property>(Context));
            Assert.DoesNotContain("column", ex.Message);
            Assert.Contains("Subquery returned more than 1 value", ex.InnerException.Message);
        }
    }
}
