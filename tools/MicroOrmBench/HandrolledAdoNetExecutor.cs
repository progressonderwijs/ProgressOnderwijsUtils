using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ProgressOnderwijsUtils;

namespace MicroOrmBench
{
    static class HandrolledAdoNetExecutor
    {
        public static void RunQuery(Benchmarker benchmarker)
        {
            benchmarker.Bench("Raw recreated", ExecuteQuery);
        }

        static int ExecuteQuery(SqlCommandCreationContext ctx, int rows)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = ExampleObject.RawQueryString;
                cmd.Connection = ctx.Connection;
                var argP = new SqlParameter
                {
                    SqlDbType = SqlDbType.Int,
                    ParameterName = "@Arg",
                    IsNullable = true,
                    Value = DBNull.Value,
                };
                var numP = new SqlParameter
                {
                    SqlDbType = SqlDbType.Int,
                    ParameterName = "@Num2",
                    IsNullable = false,
                    Value = 2,
                };
                var heheP = new SqlParameter
                {
                    SqlDbType = SqlDbType.NVarChar,
                    ParameterName = "@hehe",
                    IsNullable = false,
                    Value = "hehe",
                };
                var topP = new SqlParameter
                {
                    SqlDbType = SqlDbType.Int,
                    ParameterName = "@Top",
                    IsNullable = false,
                    Value = rows,
                };
                cmd.Parameters.Add(topP);
                cmd.Parameters.Add(numP);
                cmd.Parameters.Add(heheP);
                cmd.Parameters.Add(argP);
                using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    var list = new List<ExampleObject>();
                    while (reader.Read())
                    {
                        list.Add(new ExampleObject
                        {
                            A = reader.IsDBNull(0) ? default(int?) : reader.GetInt32(0),
                            B = reader.GetInt32(1),
                            C = reader.GetString(2),
                            D = reader.GetDateTime(3),
                            E = reader.GetInt32(4),
                            Arg = reader.IsDBNull(5) ? default(int?) : reader.GetInt32(5),
                        });
                    }
                    return list.Count;
                }
            }
        }

        public static void RunWideQuery(Benchmarker benchmarker)
        {
            benchmarker.Bench("Raw recreated (26-col)", ExecuteWideQuery);
        }

        static int ExecuteWideQuery(SqlCommandCreationContext ctx, int rows)
        {
            using (var cmd = new SqlCommand()) {
                cmd.CommandText = WideExampleObject.RawQueryString;
                cmd.Connection = ctx.Connection;
                var topP = new SqlParameter {
                    SqlDbType = SqlDbType.Int,
                    ParameterName = "@Top",
                    IsNullable = false,
                    Value = rows,
                };
                cmd.Parameters.Add(topP);
                using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess)) {
                    var list = new List<WideExampleObject>();
                    while (reader.Read()) {
                        list.Add(new WideExampleObject {
                            SalesOrderId = reader.GetInt32(0),
                            RevisionNumber = reader.GetByte(1),
                            OrderDate = reader.GetDateTime(2),
                            DueDate = reader.GetDateTime(3),
                            ShipDate = reader.IsDBNull(4) ? default(DateTime?) : reader.GetDateTime(4),
                            Status = reader.GetByte(5),
                            OnlineOrderFlag =  reader.GetBoolean(6),
                            SalesOrderNumber = reader.IsDBNull(7) ? default(string) : reader.GetString(7),
                            PurchaseOrderNumber = reader.IsDBNull(8) ? default(string) : reader.GetString(8),
                            AccountNumber = reader.IsDBNull(9) ? default(string) : reader.GetString(9),
                            CustomerId = reader.GetInt32(10),
                            SalesPersonId = reader.IsDBNull(11) ? default(int?) : reader.GetInt32(11),
                            TerritoryId = reader.IsDBNull(12) ? default(int?) : reader.GetInt32(12),
                            BillToAddressId = reader.GetInt32(13),
                            ShipToAddressId = reader.GetInt32(14),
                            ShipMethodId = reader.GetInt32(15),
                            CreditCardId = reader.IsDBNull(16) ? default(int?) : reader.GetInt32(16),
                            CreditCardApprovalCode = reader.IsDBNull(17) ? default(string) : reader.GetString(17),
                            CurrencyRateId = reader.IsDBNull(18) ? default(int?) : reader.GetInt32(18),
                            SubTotal =  reader.GetDecimal(19),
                            TaxAmt =  reader.GetDecimal(20),
                            Freight =  reader.GetDecimal(21),
                            TotalDue =  reader.GetDecimal(22),
                            Comment = reader.IsDBNull(23) ? default(string) : reader.GetString(23),
                            Rowguid = reader.GetGuid(24),
                            ModifiedDate = reader.GetDateTime(25),
                        });
                    }
                    return list.Count;
                }
            }
        }
    }
}