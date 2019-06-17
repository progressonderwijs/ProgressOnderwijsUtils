using System;
using System.Data.SqlClient;
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

        struct ExactMapping : IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public int? SomeColumn { get; set; }
            public int? OtherColumn { get; set; }

            [NotNull]
            public static ExactMapping[] Load([NotNull] SqlConnection context)
                => SQL($@"select t.* from {testTableName} t").ReadMetaObjects<ExactMapping>(context);
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
            var bulkInsertTarget = CreateTargetTable();
            Assert.Throws<InvalidOperationException>(() => { bulkInsertTarget.With(BulkCopyFieldMappingMode.ExactMatch).BulkInsert(Connection, new[] { new LessColumns { Id = 37, SomeColumn = 42 } }); });
        }

        [Fact]
        public void Exact_mapping_gives_exception_on_more_columns()
            => Assert.Throws<InvalidOperationException>(() => { CreateTargetTable().With(BulkCopyFieldMappingMode.ExactMatch).BulkInsert(Connection, new[] { new MoreColumns() }); });

        [NotNull]
        BulkInsertTarget CreateTargetTable()
        {
            SQL($@"
                create table {testTableName} (
                    Id int not null
                    , SomeColumn int null
                    , OtherColumn int null
                );
            ").ExecuteNonQuery(Connection);

            return BulkInsertTarget.FromCompleteSetOfColumns(testTableName.CommandText(), DbColumnMetaData.ColumnMetaDatas(Connection, testTableName));
        }

        [Fact]
        public void Exact_mapping_works_when_mapping_is_exact()
        {
            CreateTargetTable().With(BulkCopyFieldMappingMode.ExactMatch).BulkInsert(Connection, new[] { new ExactMapping() });
            PAssert.That(() => ExactMapping.Load(Connection).Any());
        }

        [Fact]
        public void AllowExtraDatabaseColumns_mapping_gives_exception_on_more_columns()
            => Assert.Throws<InvalidOperationException>(() => { CreateTargetTable().With(BulkCopyFieldMappingMode.AllowExtraDatabaseColumns).BulkInsert(Connection, new[] { new MoreColumns() }); });

        [Fact]
        public void AllowExtraDatabaseColumns_mapping_works_on_less_columns()
        {
            CreateTargetTable().With(BulkCopyFieldMappingMode.AllowExtraDatabaseColumns).BulkInsert(Connection, new[] { new LessColumns() });
            PAssert.That(() => ExactMapping.Load(Connection).Any());
        }

        [Fact]
        public void AllowExtraDatabaseColumns_mapping_works_when_mapping_is_exact()
        {
            CreateTargetTable().With(BulkCopyFieldMappingMode.AllowExtraDatabaseColumns).BulkInsert(Connection, new[] { new ExactMapping() });
            PAssert.That(() => ExactMapping.Load(Connection).Any());
        }

        [Fact]
        public void AllowExtraMetaObjectProperties_mapping_gives_exception_on_less_columns()
            => Assert.Throws<InvalidOperationException>(() => { CreateTargetTable().With(BulkCopyFieldMappingMode.AllowExtraMetaObjectProperties).BulkInsert(Connection, new[] { new LessColumns() }); });

        [Fact]
        public void AllowExtraMetaObjectProperties_mapping_works_on_more_columns()
        {
            CreateTargetTable().With(BulkCopyFieldMappingMode.AllowExtraMetaObjectProperties).BulkInsert(Connection, new[] { new MoreColumns() });
            PAssert.That(() => ExactMapping.Load(Connection).Any());
        }

        [Fact]
        public void AllowExtraMetaObjectProperties_mapping_works_when_mapping_is_exact()
        {
            CreateTargetTable().With(BulkCopyFieldMappingMode.AllowExtraMetaObjectProperties).BulkInsert(Connection, new[] { new ExactMapping() });
            PAssert.That(() => ExactMapping.Load(Connection).Any());
        }
    }
}
