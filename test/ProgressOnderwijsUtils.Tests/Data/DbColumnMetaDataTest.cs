using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class DbColumnMetaDataTest : TransactedLocalConnection
    {
        public sealed record SamplePoco : IWrittenImplicitly, IReadImplicitly
        {
            public DayOfWeek AnEnum { get; set; }
            public DateTime? ADateTime { get; set; }
            public string? SomeString { get; set; }
            public decimal? LotsOfMoney { get; set; }
            public double VagueNumber { get; set; }
            public DateTime DateTime { get; set; }
            public TimeSpan TimeSpan { get; set; }
        }

        [Fact]
        public void CreatedTempTableMetaDataRoundTrips()
        {
            var columnsFromCode = PocoProperties<SamplePoco>.Instance.ArraySelect(prop => DbColumnMetaData.Create(prop.Name, prop.DataType, prop.IsKey, null));

            var tempTableName = SQL($"#test");
            columnsFromCode.CreateNewTableQuery(tempTableName).ExecuteNonQuery(Connection);

            var columnsFromCodeAsSql = columnsFromCode.ArraySelect(c => c.ToSqlColumnDefinition());
            var columnsFromDbAsSql = DbColumnMetaData.ColumnMetaDatas(Connection, tempTableName).ArraySelect(c => c.ToSqlColumnDefinition());

            PAssert.That(() => columnsFromCodeAsSql.SequenceEqual(columnsFromDbAsSql));
        }
    }
}
