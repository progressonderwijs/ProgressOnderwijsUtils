using System;
using System.Runtime.CompilerServices;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench
{
    public sealed class WideExampleObject : IMetaObject
    {
        public int SalesOrderId { get; set; }
        public string AccountNumber { get; set; }
        public int BillToAddressId { get; set; }
        public string Comment { get; set; }
        public string CreditCardApprovalCode { get; set; }
        public int? CreditCardId { get; set; }
        public int? CurrencyRateId { get; set; }
        public int CustomerId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Freight { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool OnlineOrderFlag { get; set; }
        public DateTime OrderDate { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public byte RevisionNumber { get; set; }
        public Guid Rowguid { get; set; }
        public string SalesOrderNumber { get; set; }
        public int? SalesPersonId { get; set; }
        public DateTime? ShipDate { get; set; }
        public int ShipMethodId { get; set; }
        public int ShipToAddressId { get; set; }
        public byte Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public int? TerritoryId { get; set; }
        public decimal TotalDue { get; set; }

        static readonly FormattableString formattableQueryString = $@"
                SELECT top ({1})
                    SalesOrderID,RevisionNumber,OrderDate,DueDate,ShipDate,Status,OnlineOrderFlag,SalesOrderNumber,PurchaseOrderNumber,AccountNumber
                    ,CustomerID,SalesPersonID,TerritoryID,BillToAddressID,ShipToAddressID,ShipMethodID,CreditCardID,CreditCardApprovalCode,CurrencyRateID
                    ,SubTotal,TaxAmt,Freight,TotalDue,Comment,rowguid,ModifiedDate	
                from (select SalesOrderID = 13 union all select 14) a01
                cross join(select AccountNumber = N'abracadabra fee fi fo fum' union all select N'abcdef') a02
                cross join (select BillToAddressId = 0 union all select 1) a03
                cross join(select Comment = N'abracadabra fee fi fo fum' union all select null) a04
                cross join(select CreditCardApprovalCode = N'abracadabra fee fi fo fum' union all select null) a05
                cross join(select CreditCardId = 42 union all select null ) a06
                cross join(select CurrencyRateId = 37 union all select null ) a07
                cross join(select CustomerId = 37 union all select 38 ) a08
                cross join(select DueDate = cast('2014-01-02' as datetime2) union all select cast('2014-01-03' as datetime2)) a09
                cross join(select Freight = cast(1.1 as decimal(18,2)) union all select cast(1.1 as decimal(18,2))) a10
                cross join(select ModifiedDate = cast('2014-01-02' as datetime2) union all select cast('2014-01-03' as datetime2)) a11
                cross join(select OnlineOrderFlag = cast(1 as bit) union all select cast(0 as bit)) a12
                cross join(select OrderDate = cast('2014-01-02' as datetime2) union all select cast('2014-01-03' as datetime2)) a13
                cross join(select PurchaseOrderNumber = N'blablablabla' union all select N'fizzbuzzqwerp') a14
                cross join(select RevisionNumber = cast(1 as tinyint) union all select cast(10 as tinyint)) a15    
                cross join(select Rowguid = NEWID ( )) a16
                cross join(select SalesOrderNumber = N'blablablabla' union all select N'fizzbuzzqwerp') a17
                cross join(select SalesPersonId = 37 union all select null ) a18
                cross join(select ShipDate = cast('2014-01-02' as datetime2) union all select null) a19
                cross join(select ShipMethodId = 0 union all select 1) a20
                cross join(select ShipToAddressId = 0 union all select 1) a21
                cross join(select Status = cast(1 as tinyint) union all select cast(10 as tinyint)) a22
                cross join(select SubTotal = cast(1.1 as decimal(18,2)) union all select cast(1.1 as decimal(18,2))) a23
                cross join(select TaxAmt = cast(1.1 as decimal(18,2)) union all select cast(1.1 as decimal(18,2))) a24
                cross join(select TerritoryId = 37 union all select null ) a25
                cross join(select TotalDue = cast(1.1 as decimal(18,2)) union all select cast(1.1 as decimal(18,2))) a26
            ";

        static readonly string formatString = formattableQueryString.Format;
        public static readonly string RawQueryString = string.Format(formatString, "@Top");

        public static ParameterizedSql ParameterizedSqlForRows(int rows)
            => SafeSql.SQL(FormattableStringFactory.Create(formatString, rows));
    }
}
