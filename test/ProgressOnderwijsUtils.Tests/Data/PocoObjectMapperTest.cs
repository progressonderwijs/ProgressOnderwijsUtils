using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ExpressionToCodeLib;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class PocoObjectMapperTest : TransactedLocalConnection
    {
        public sealed class ExampleWithJustSetters : IWrittenImplicitly
        {
            public int SalesOrderId { get; set; }
            public string AccountNumber { get; set; } = null!;
            public string? Comment { get; set; }
            public DateTime DueDate { get; set; }
            public bool OnlineOrderFlag { get; set; }
            public Guid Rowguid { get; set; }
            public int? SalesPersonId { get; set; }
            public DateTime? ShipDate { get; set; }
            public byte Status { get; set; }
            public decimal TotalDue { get; set; }
            public byte[] SomeBlob { get; set; } = null!;
            public byte[]? SomeNullableBlob { get; set; }
        }

        public sealed class ExampleWithConstructor : IWrittenImplicitly
        {
            public ExampleWithConstructor(string accountNumber, byte[] someBlob)
            {
                AccountNumber = accountNumber;
                SomeBlob = someBlob;
            }

            public int SalesOrderId { get; set; }
            public string AccountNumber { get; }
            public string? Comment { get; set; }
            public DateTime DueDate { get; set; }
            public bool OnlineOrderFlag { get; set; }
            public Guid Rowguid { get; set; }
            public int? SalesPersonId { get; set; }
            public DateTime? ShipDate { get; set; }
            public byte Status { get; set; }
            public decimal TotalDue { get; set; }
            public byte[] SomeBlob { get; }
            public byte[]? SomeNullableBlob { get; set; }
        }

        public sealed class ExampleWithMoreConstructor : IWrittenImplicitly
        {
            public ExampleWithMoreConstructor(int salesOrderId, string accountNumber, DateTime dueDate, bool onlineOrderFlag, byte status, decimal totalDue, byte[] someBlob)
            {
                SalesOrderId = salesOrderId;
                AccountNumber = accountNumber;
                DueDate = dueDate;
                OnlineOrderFlag = onlineOrderFlag;
                Status = status;
                TotalDue = totalDue;
                SomeBlob = someBlob;
            }

            public int SalesOrderId { get; }
            public string AccountNumber { get; }
            public string? Comment { get; set; }
            public DateTime DueDate { get; }
            public bool OnlineOrderFlag { get; }
            public Guid Rowguid { get; set; }
            public int? SalesPersonId { get; set; }
            public DateTime? ShipDate { get; set; }
            public byte Status { get; }
            public decimal TotalDue { get; }
            public byte[] SomeBlob { get; }
            public byte[]? SomeNullableBlob { get; set; }
        }

        public static ParameterizedSql ParameterizedSqlForRows(int rows)
            => SQL($@"
                SELECT top ({rows})
                    SalesOrderID,DueDate,ShipDate,Status,OnlineOrderFlag,AccountNumber,SalesPersonID,TotalDue,Comment,rowGuid, SomeBlob, SomeNullableBlob
                from (select SalesOrderID = 13 union all select 14) a01
                cross join(select AccountNumber = N'abracadabra fee fi fo fum' union all select N'abcdef') a02
                cross join(select Comment = N'abracadabra fee fi fo fum' union all select null) a04
                cross join(select DueDate = cast('2014-01-02' as datetime2) union all select cast('2014-01-03' as datetime2)) a09
                cross join(select OnlineOrderFlag = cast(1 as bit) union all select cast(0 as bit)) a12
                cross join(select Rowguid = NEWID ( )) a16
                cross join(select SalesPersonId = 37 union all select null ) a18
                cross join(select ShipDate = cast('2014-01-02' as datetime2) union all select null) a19
                cross join(select Status = cast(1 as tinyint) union all select cast(10 as tinyint)) a22
                cross join(select TotalDue = cast(1.1 as decimal(18,2))) a26
                cross join(select SomeBlob = cast('deadbeef' as varbinary(max))) a27
                cross join(select SomeNullableBlob = cast('deadbeef' as varbinary(max)) union all select null) a28
            "
            );

        [Fact]
        public void Read0()
        {
            // ReSharper disable once UnusedVariable
            var bla = ParameterizedSqlForRows(512).OfPocos<ExampleWithJustSetters>().Execute(Connection);
            PAssert.That(() => bla.Length == 512);
            PAssert.That(() => bla.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        [Fact]
        public void Read1()
        {
            // ReSharper disable once UnusedVariable
            var bla = ParameterizedSqlForRows(512).OfPocos<ExampleWithConstructor>().Execute(Connection);
            PAssert.That(() => bla.Length == 512);
            PAssert.That(() => bla.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        [Fact]
        public void Read2()
        {
            // ReSharper disable once UnusedVariable
            var bla = ParameterizedSqlForRows(512).OfPocos<ExampleWithMoreConstructor>().Execute(Connection);
            PAssert.That(() => bla.Length == 512);
            PAssert.That(() => bla.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }
    }
}
