﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ExpressionToCodeLib;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class PocoObjectMapperTest : TransactedLocalConnection
    {
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

        [Fact]
        public void ReadWithJustSetters()
        {
            // ReSharper disable once UnusedVariable
            var bla = ParameterizedSqlForRows(512).OfPocos<ExampleWithJustSetters>().Execute(Connection);
            PAssert.That(() => bla.Length == 512);
            PAssert.That(() => bla.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        public sealed class ExampleWithJustSettersWithExtraProperties : IWrittenImplicitly
        {
            public int ExtraExtra { get; set; }
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

        [Fact]
        public void ReadWithMissingMappingsDoesNotIgnoreExtraProperties()
        {
            // ReSharper disable once UnusedVariable
            var query = ParameterizedSqlForRows(512).OfPocos<ExampleWithJustSettersWithMissingProperties>();
            Assert.ThrowsAny<Exception>(() => query.Execute(Connection));
            Assert.ThrowsAny<Exception>(() => query.WithFieldMappingMode(FieldMappingMode.IgnoreExtraPocoProperties).Execute(Connection));
            var retval = SQL($"select AccountNumber, SalesOrderId from ({query.Sql}) x").ReadPocos<ExampleWithJustSettersWithMissingProperties>(Connection); //implicitly assert does not throw

            PAssert.That(() => retval.Length == 512);
            PAssert.That(() => retval.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        public sealed class ExampleWithJustSettersWithMissingProperties : IWrittenImplicitly
        {
            public string AccountNumber { get; set; } = null!;
            public int SalesOrderId { get; set; }
        }

        [Fact]
        public void ReadWithExtraMappings()
        {
            // ReSharper disable once UnusedVariable
            var query = ParameterizedSqlForRows(512).OfPocos<ExampleWithJustSettersWithExtraProperties>();
            Assert.ThrowsAny<Exception>(() => query.Execute(Connection));
            var retval = query.WithFieldMappingMode(FieldMappingMode.IgnoreExtraPocoProperties).Execute(Connection); // implicitly assert: does not throw.

            PAssert.That(() => retval.Length == 512);
            PAssert.That(() => retval.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
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

        [Fact]
        public void ReadWithSomeConstructorArgs()
        {
            // ReSharper disable once UnusedVariable
            var bla = ParameterizedSqlForRows(512).OfPocos<ExampleWithConstructor>().Execute(Connection);
            PAssert.That(() => bla.Length == 512);
            PAssert.That(() => bla.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        public sealed class ExampleWithMoreConstructor : IWrittenImplicitly
        {
            public ExampleWithMoreConstructor(string accountNumber, byte[] someBlob)
            {
                AccountNumber = accountNumber;
                SomeBlob = someBlob;
                throw new Exception("This constructor should never be selected, the poco orm should choose the longest constructor");
            }

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
            // ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060 // Remove unused parameter
            public ExampleWithMoreConstructor(int salesOrderId, string accountNumber, DateTime dueDate, bool onlineOrderFlag, byte status, decimal totalDue, byte[] someBlob, int unmatchable)
#pragma warning restore IDE0060 // Remove unused parameter
                // ReSharper restore UnusedParameter.Local
            {
                throw new Exception("This constructor should never be selected, the poco orm should not choose a constructor with an unmatchable arg");
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

        [Fact]
        public void ReadWithManyConstructorArgs()
        {
            // ReSharper disable once UnusedVariable
            var bla = ParameterizedSqlForRows(512).OfPocos<ExampleWithMoreConstructor>().Execute(Connection);
            PAssert.That(() => bla.Length == 512);
            PAssert.That(() => bla.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
            PAssert.That(() => bla.Select(o => o.Status).Distinct().SetEqual(new byte[] { 1, 10 }));
            PAssert.That(() => bla.None(o => o.SomeBlob.PretendNullable() == null));
            PAssert.That(() => bla.Any(o => o.SomeNullableBlob == null) && bla.Any(o => o.SomeNullableBlob != null));
        }

        public sealed class ExampleWithConstructorWithExtraProperties : IWrittenImplicitly
        {
            public ExampleWithConstructorWithExtraProperties(string accountNumber, byte[] someBlob)
            {
                AccountNumber = accountNumber;
                SomeBlob = someBlob;
            }

            public int ExtraExtra { get; set; }
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

        [Fact]
        public void ReadByConstructorWithExtraMappings()
        {
            // ReSharper disable once UnusedVariable
            var query = ParameterizedSqlForRows(512).OfPocos<ExampleWithConstructorWithExtraProperties>();
            Assert.ThrowsAny<Exception>(() => query.Execute(Connection));
            var retval = query.WithFieldMappingMode(FieldMappingMode.IgnoreExtraPocoProperties).Execute(Connection); // implicitly assert: does not throw.

            PAssert.That(() => retval.Length == 512);
            PAssert.That(() => retval.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }
    }
}