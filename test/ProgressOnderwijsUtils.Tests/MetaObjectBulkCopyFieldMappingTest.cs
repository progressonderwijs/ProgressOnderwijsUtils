using System;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class MetaObjectBulkCopyFieldMappingTest : TransactedLocalConnection
    {
        static readonly ParameterizedSql testTableName = SQL($"MetaObjectBulkCopyFieldMappingTestTable");

        (string tableName, DbColumnMetaData[] columns) CreateTestTable()
        {
            SQL($@"
                create table {testTableName} (
                    Id int not null
                    , SomeColumn int null
                    , OtherColumn int null
                );
            ").ExecuteNonQuery(Context);

            return (testTableName.CommandText(), DbColumnMetaData.ColumnMetaDatas(Context.Connection, testTableName));
        }

        struct ExactMapping : IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public int? SomeColumn { get; set; }
            public int? OtherColumn { get; set; }

            [NotNull]
            public static ExactMapping[] Load([NotNull] SqlCommandCreationContext context)
            {
                return SQL($@"select t.* from {testTableName} t").ReadMetaObjects<ExactMapping>(context);
            }
        }

        struct LessColumns : IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public int? SomeColumn { get; set; }
        }

        struct MoreColumns : IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public int? SomeColumn { get; set; }
            public int? OtherColumn { get; set; }
            public int? ExtraColumn { get; set; }
        }

        [Fact]
        public void Exact_mapping_gives_exception_on_less_columns()
        {
            var (table, columns) = CreateTestTable();

            Assert.Throws<InvalidOperationException>(() => new[] { new LessColumns() }.BulkCopyToSqlServer(Context, table, columns, BulkCopyFieldMappingMode.ExactMatch));
        }

        [Fact]
        public void Exact_mapping_gives_exception_on_more_columns()
        {
            var (table, columns) = CreateTestTable();

            Assert.Throws<InvalidOperationException>(() => new[] { new MoreColumns() }.BulkCopyToSqlServer(Context, table, columns, BulkCopyFieldMappingMode.ExactMatch));
        }

        [Fact]
        public void Exact_mapping_works_when_mapping_is_exact()
        {
            var (table, columns) = CreateTestTable();

            new[] { new ExactMapping() }.BulkCopyToSqlServer(Context, table, columns, BulkCopyFieldMappingMode.ExactMatch);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }

        [Fact]
        public void AllowExtraDatabaseColumns_mapping_gives_exception_on_more_columns()
        {
            var (table, columns) = CreateTestTable();

            Assert.Throws<InvalidOperationException>(() => new[] { new MoreColumns() }.BulkCopyToSqlServer(Context, table, columns, BulkCopyFieldMappingMode.AllowExtraDatabaseColumns));
        }

        [Fact]
        public void AllowExtraDatabaseColumns_mapping_works_on_less_columns()
        {
            var (table, columns) = CreateTestTable();

            new[] { new LessColumns() }.BulkCopyToSqlServer(Context, table, columns, BulkCopyFieldMappingMode.AllowExtraDatabaseColumns);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }

        [Fact]
        public void AllowExtraDatabaseColumns_mapping_works_when_mapping_is_exact()
        {
            var (table, columns) = CreateTestTable();

            new[] { new ExactMapping() }.BulkCopyToSqlServer(Context, table, columns, BulkCopyFieldMappingMode.AllowExtraDatabaseColumns);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }

        [Fact]
        public void AllowExtraMetaObjectProperties_mapping_gives_exception_on_less_columns()
        {
            var (table, columns) = CreateTestTable();

            Assert.Throws<InvalidOperationException>(() => new[] { new LessColumns() }.BulkCopyToSqlServer(Context, table, columns, BulkCopyFieldMappingMode.AllowExtraMetaObjectProperties));
        }

        [Fact]
        public void AllowExtraMetaObjectProperties_mapping_works_on_more_columns()
        {
            var (table, columns) = CreateTestTable();

            new[] { new MoreColumns() }.BulkCopyToSqlServer(Context, table, columns, BulkCopyFieldMappingMode.AllowExtraMetaObjectProperties);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }

        [Fact]
        public void AllowExtraMetaObjectProperties_mapping_works_when_mapping_is_exact()
        {
            var (table, columns) = CreateTestTable();

            new[] { new ExactMapping() }.BulkCopyToSqlServer(Context, table, columns, BulkCopyFieldMappingMode.AllowExtraMetaObjectProperties);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }
    }
}
