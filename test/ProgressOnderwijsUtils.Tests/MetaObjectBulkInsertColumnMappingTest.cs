using System;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class MetaObjectBulkInsertColumnMappingTest : TransactedLocalConnection
    {
        static readonly ParameterizedSql testTableName = SQL($"MetaObjectBulkInsertColumnMappingTestTable");

        void CreateTestTable()
        {
            SQL($@"
                create table {testTableName} (
                    Id int not null
                    , SomeColumn int null
                    , OtherColumn int null
                );
            ").ExecuteNonQuery(Context);
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
            CreateTestTable();

            Assert.Throws<InvalidOperationException>(() => new[] { new LessColumns() }.BulkCopyToSqlServerWithSpecificColumnMapping(Context, testTableName.CommandText(), FieldMappingMode.RequireExactColumnMatches));
        }

        [Fact]
        public void Exact_mapping_gives_exception_on_more_columns()
        {
            CreateTestTable();

            Assert.Throws<InvalidOperationException>(() => new[] { new MoreColumns() }.BulkCopyToSqlServerWithSpecificColumnMapping(Context, testTableName.CommandText(), FieldMappingMode.RequireExactColumnMatches));
        }

        [Fact]
        public void Exact_mapping_works_when_mapping_is_exact()
        {
            CreateTestTable();

            new[] { new ExactMapping() }.BulkCopyToSqlServerWithSpecificColumnMapping(Context, testTableName.CommandText(), FieldMappingMode.RequireExactColumnMatches);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }

        [Fact]
        public void IgnoreExtraDestination_mapping_gives_exception_on_more_columns()
        {
            CreateTestTable();

            Assert.Throws<InvalidOperationException>(() => new[] { new MoreColumns() }.BulkCopyToSqlServerWithSpecificColumnMapping(Context, testTableName.CommandText(), FieldMappingMode.IgnoreExtraDestinationFields));
        }

        [Fact]
        public void IgnoreExtraDestination_mapping_works_on_less_columns()
        {
            CreateTestTable();

            new[] { new LessColumns() }.BulkCopyToSqlServerWithSpecificColumnMapping(Context, testTableName.CommandText(), FieldMappingMode.IgnoreExtraDestinationFields);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }

        [Fact]
        public void IgnoreExtraDestination_mapping_works_when_mapping_is_exact()
        {
            CreateTestTable();

            new[] { new ExactMapping() }.BulkCopyToSqlServerWithSpecificColumnMapping(Context, testTableName.CommandText(), FieldMappingMode.IgnoreExtraDestinationFields);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }

        [Fact]
        public void IgnoreExtraSource_mapping_gives_exception_on_less_columns()
        {
            CreateTestTable();

            Assert.Throws<InvalidOperationException>(() => new[] { new LessColumns() }.BulkCopyToSqlServerWithSpecificColumnMapping(Context, testTableName.CommandText(), FieldMappingMode.IgnoreExtraSourceFields));
        }

        [Fact]
        public void IgnoreExtraSource_mapping_works_on_more_columns()
        {
            CreateTestTable();

            new[] { new MoreColumns() }.BulkCopyToSqlServerWithSpecificColumnMapping(Context, testTableName.CommandText(), FieldMappingMode.IgnoreExtraSourceFields);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }

        [Fact]
        public void IgnoreExtraSource_mapping_works_when_mapping_is_exact()
        {
            CreateTestTable();

            new[] { new ExactMapping() }.BulkCopyToSqlServerWithSpecificColumnMapping(Context, testTableName.CommandText(), FieldMappingMode.IgnoreExtraSourceFields);

            PAssert.That(() => ExactMapping.Load(Context).Any());
        }
    }
}
