using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench
{
    static class HandrolledAdoNetExecutor
    {
        public static void RunQuery(Benchmarker benchmarker)
        {
            benchmarker.BenchSQLite("Raw (sqlite, cached)", ExecuteSqliteQueryCached);
            benchmarker.BenchSQLite("Raw (sqlite)", ExecuteSqliteQuery);
            benchmarker.BenchSqlServer("Raw", ExecuteQuery);
            benchmarker.BenchSqlServer("Raw (SqlDbTypes)", ExecuteQuery2);
        }

        static int ExecuteQuery(SqlConnection sqlConn, int rows)
        {
            using var cmd = new SqlCommand { CommandText = ExampleObject.RawQueryString, Connection = sqlConn };
            var argP = new SqlParameter {
                SqlDbType = SqlDbType.BigInt,
                ParameterName = "@Arg",
                IsNullable = false,
                Value = ExampleObject.someInt64Value,
            };
            var numP = new SqlParameter {
                SqlDbType = SqlDbType.Int,
                ParameterName = "@Num2",
                IsNullable = false,
                Value = 2,
            };
            var heheP = new SqlParameter {
                SqlDbType = SqlDbType.NVarChar,
                ParameterName = "@hehe",
                IsNullable = false,
                Value = "hehe",
            };
            var topP = new SqlParameter {
                SqlDbType = SqlDbType.Int,
                ParameterName = "@Top",
                IsNullable = false,
                Value = rows,
            };
            cmd.Parameters.Add(topP);
            cmd.Parameters.Add(numP);
            cmd.Parameters.Add(heheP);
            cmd.Parameters.Add(argP);
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            var list = new List<ExampleObject>();
            while (reader.Read()) {
                list.Add(new ExampleObject {
                    A = reader.IsDBNull(0) ? default(int?) : reader.GetInt32(0),
                    B = reader.GetInt32(1),
                    C = reader.GetString(2),
                    D = reader.IsDBNull(3) ? default(bool?) : reader.GetBoolean(3),
                    E = reader.GetInt32(4),
                    Arg = reader.GetInt64(5),
                });
            }
            return list.Count;
        }

        static int ExecuteSqliteQuery(SQLiteConnection sqliteConn, int rows)
        {
            using var cmd = sqliteConn.CreateCommand();
            cmd.CommandText = ExampleObject.RawSqliteQueryString;
            cmd.Connection = sqliteConn;
            var argP = new SQLiteParameter {
                DbType = DbType.Int64,
                ParameterName = "@Arg",
                IsNullable = false,
                Value = ExampleObject.someInt64Value,
            };
            var numP = new SQLiteParameter {
                DbType = DbType.Int32,
                ParameterName = "@Num2",
                IsNullable = false,
                Value = 2,
            };
            var heheP = new SQLiteParameter {
                DbType = DbType.String,
                ParameterName = "@hehe",
                IsNullable = false,
                Value = "hehe",
            };
            var topP = new SQLiteParameter {
                DbType = DbType.Int32,
                ParameterName = "@Top",
                IsNullable = false,
                Value = rows,
            };
            cmd.Parameters.Add(topP);
            cmd.Parameters.Add(numP);
            cmd.Parameters.Add(heheP);
            cmd.Parameters.Add(argP);
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            var list = new List<ExampleObject>();
            while (reader.Read()) {
                list.Add(new ExampleObject {
                    A = reader.IsDBNull(0) ? default(int?) : reader.GetInt32(0),
                    B = reader.GetInt32(1),
                    C = reader.GetString(2),
                    D = reader.IsDBNull(3) ? default(bool?) : reader.GetBoolean(3),
                    E = reader.GetInt32(4),
                    Arg = reader.GetInt64(5),
                });
            }
            return list.Count;
        }

        static readonly ConcurrentDictionary<SQLiteConnection, Dictionary<string, SQLiteCommand>> connCmdCache = new ConcurrentDictionary<SQLiteConnection, Dictionary<string, SQLiteCommand>>();

        static int ExecuteSqliteQueryCached(SQLiteConnection sqliteConn, int rows)
        {
            if (!connCmdCache.TryGetValue(sqliteConn, out var cmdCache)) {
                cmdCache = connCmdCache.GetOrAdd(sqliteConn, conn => {
                    conn.Disposed += (o, e) => connCmdCache.TryRemove(conn, out _);
                    return new Dictionary<string, SQLiteCommand>();
                });
            }

            if (!cmdCache.TryGetValue(ExampleObject.RawSqliteQueryString, out var sqliteCmd)) {
                cmdCache[ExampleObject.RawSqliteQueryString] = sqliteCmd = new SQLiteCommand { CommandText = ExampleObject.RawSqliteQueryString };
                var topP = new SQLiteParameter {
                    DbType = DbType.Int32,
                    ParameterName = "@Top",
                    IsNullable = false,
                    //Value = rows,
                };
                var numP = new SQLiteParameter {
                    DbType = DbType.Int32,
                    ParameterName = "@Num2",
                    IsNullable = false,
                    //Value = 2,
                };
                var heheP = new SQLiteParameter {
                    DbType = DbType.String,
                    ParameterName = "@hehe",
                    IsNullable = false,
                    //Value = "hehe",
                };
                var argP = new SQLiteParameter {
                    DbType = DbType.Int64,
                    ParameterName = "@Arg",
                    IsNullable = false,
                    //Value = ExampleObject.someInt64Value,
                };
                sqliteCmd.Parameters.Add(topP);
                sqliteCmd.Parameters.Add(numP);
                sqliteCmd.Parameters.Add(heheP);
                sqliteCmd.Parameters.Add(argP);
                sqliteCmd.Connection = sqliteConn;
            }

            sqliteCmd.Parameters[0].Value = rows;
            sqliteCmd.Parameters[1].Value = 2;
            sqliteCmd.Parameters[2].Value = "hehe";
            sqliteCmd.Parameters[3].Value = ExampleObject.someInt64Value;

            var list = new List<ExampleObject>();
            using (var reader = sqliteCmd.ExecuteReader(CommandBehavior.SequentialAccess)) {
                while (reader.Read()) {
                    list.Add(new ExampleObject {
                        A = reader.IsDBNull(0) ? default(int?) : reader.GetInt32(0),
                        B = reader.GetInt32(1),
                        C = reader.GetString(2),
                        D = reader.IsDBNull(3) ? default(bool?) : reader.GetBoolean(3),
                        E = reader.GetInt32(4),
                        Arg = reader.GetInt64(5),
                    });
                }
            }
            //cmd.Connection = null;
            sqliteCmd.Parameters[0].Value = null;
            sqliteCmd.Parameters[1].Value = null;
            sqliteCmd.Parameters[2].Value = null;
            sqliteCmd.Parameters[3].Value = null;
            return list.Count;
        }

        static int ExecuteQuery2(SqlConnection sqlConn, int rows)
        {
            using var cmd = new SqlCommand { CommandText = ExampleObject.RawQueryString, Connection = sqlConn };
            var argP = new SqlParameter {
                SqlDbType = SqlDbType.BigInt,
                ParameterName = "@Arg",
                IsNullable = false,
                Value = ExampleObject.someInt64Value,
            };
            var numP = new SqlParameter {
                SqlDbType = SqlDbType.Int,
                ParameterName = "@Num2",
                IsNullable = false,
                Value = 2,
            };
            var heheP = new SqlParameter {
                SqlDbType = SqlDbType.NVarChar,
                ParameterName = "@hehe",
                IsNullable = false,
                Value = "hehe",
            };
            var topP = new SqlParameter {
                SqlDbType = SqlDbType.Int,
                ParameterName = "@Top",
                IsNullable = false,
                Value = rows,
            };
            cmd.Parameters.Add(topP);
            cmd.Parameters.Add(numP);
            cmd.Parameters.Add(heheP);
            cmd.Parameters.Add(argP);
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            var list = new List<ExampleObject>();
            while (reader.Read()) {
                list.Add(new ExampleObject {
                    A = reader.GetSqlInt32(0).ToNullableInt(),
                    B = reader.GetInt32(1),
                    C = reader.GetString(2),
                    D = reader.GetSqlBoolean(3).ToNullableBool(),
                    E = reader.GetInt32(4),
                    Arg = reader.GetInt64(5),
                });
            }
            return list.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int? ToNullableInt(this SqlInt32 num)
            => num.IsNull ? default(int?) : num.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool? ToNullableBool(this SqlBoolean num)
            => num.IsNull ? default(bool?) : num.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once UnusedMember.Local
        static string? ToNullableString(this SqlString str)
            => str.IsNull ? default : str.Value;

        public static void RunWideQuery(Benchmarker benchmarker)
        {
            benchmarker.BenchSqlServer("Raw (26-col)", ExecuteWideQuery);
            benchmarker.BenchSqlServer("Raw (26-col, SqlDbTypes)", ExecuteWideQuery2);
        }

        static int ExecuteWideQuery2(SqlConnection sqlConn, int rows)
        {
            using var cmd = new SqlCommand { CommandText = WideExampleObject.RawQueryString, Connection = sqlConn };
            var topP = new SqlParameter {
                SqlDbType = SqlDbType.Int,
                ParameterName = "@Top",
                IsNullable = false,
                Value = rows,
            };
            cmd.Parameters.Add(topP);
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            var list = new List<WideExampleObject>();
            while (reader.Read()) {
                list.Add(new WideExampleObject {
                    SalesOrderId = reader.GetInt32(0),
                    RevisionNumber = reader.GetByte(1),
                    OrderDate = reader.GetDateTime(2),
                    DueDate = reader.GetDateTime(3),
                    ShipDate = reader.IsDBNull(4) ? default(DateTime?) : reader.GetDateTime(4),
                    Status = reader.GetByte(5),
                    OnlineOrderFlag = reader.GetBoolean(6),
                    SalesOrderNumber = reader.GetSqlString(7).ToNullableString(),
                    PurchaseOrderNumber = reader.GetSqlString(8).ToNullableString(),
                    AccountNumber = reader.GetSqlString(9).ToNullableString(),
                    CustomerId = reader.GetInt32(10),
                    SalesPersonId = reader.GetSqlInt32(11).ToNullableInt(),
                    TerritoryId = reader.GetSqlInt32(12).ToNullableInt(),
                    BillToAddressId = reader.GetInt32(13),
                    ShipToAddressId = reader.GetInt32(14),
                    ShipMethodId = reader.GetInt32(15),
                    CreditCardId = reader.GetSqlInt32(16).ToNullableInt(),
                    CreditCardApprovalCode = reader.GetSqlString(17).ToNullableString(),
                    CurrencyRateId = reader.GetSqlInt32(18).ToNullableInt(),
                    SubTotal = reader.GetDecimal(19),
                    TaxAmt = reader.GetDecimal(20),
                    Freight = reader.GetDecimal(21),
                    TotalDue = reader.GetDecimal(22),
                    Comment = reader.GetSqlString(23).ToNullableString(),
                    Rowguid = reader.GetGuid(24),
                    ModifiedDate = reader.GetDateTime(25),
                });
            }
            return list.Count;
        }

        static int ExecuteWideQuery(SqlConnection sqlConn, int rows)
        {
            using var cmd = new SqlCommand { CommandText = WideExampleObject.RawQueryString, Connection = sqlConn };
            var topP = new SqlParameter {
                SqlDbType = SqlDbType.Int,
                ParameterName = "@Top",
                IsNullable = false,
                Value = rows,
            };
            cmd.Parameters.Add(topP);
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            var list = new List<WideExampleObject>();
            while (reader.Read()) {
                list.Add(new WideExampleObject {
                    SalesOrderId = reader.GetInt32(0),
                    RevisionNumber = reader.GetByte(1),
                    OrderDate = reader.GetDateTime(2),
                    DueDate = reader.GetDateTime(3),
                    ShipDate = reader.IsDBNull(4) ? default(DateTime?) : reader.GetDateTime(4),
                    Status = reader.GetByte(5),
                    OnlineOrderFlag = reader.GetBoolean(6),
                    SalesOrderNumber = reader.IsDBNull(7) ? default : reader.GetString(7),
                    PurchaseOrderNumber = reader.IsDBNull(8) ? default : reader.GetString(8),
                    AccountNumber = reader.IsDBNull(9) ? default : reader.GetString(9),
                    CustomerId = reader.GetInt32(10),
                    SalesPersonId = reader.IsDBNull(11) ? default(int?) : reader.GetInt32(11),
                    TerritoryId = reader.IsDBNull(12) ? default(int?) : reader.GetInt32(12),
                    BillToAddressId = reader.GetInt32(13),
                    ShipToAddressId = reader.GetInt32(14),
                    ShipMethodId = reader.GetInt32(15),
                    CreditCardId = reader.IsDBNull(16) ? default(int?) : reader.GetInt32(16),
                    CreditCardApprovalCode = reader.IsDBNull(17) ? default : reader.GetString(17),
                    CurrencyRateId = reader.IsDBNull(18) ? default(int?) : reader.GetInt32(18),
                    SubTotal = reader.GetDecimal(19),
                    TaxAmt = reader.GetDecimal(20),
                    Freight = reader.GetDecimal(21),
                    TotalDue = reader.GetDecimal(22),
                    Comment = reader.IsDBNull(23) ? default : reader.GetString(23),
                    Rowguid = reader.GetGuid(24),
                    ModifiedDate = reader.GetDateTime(25),
                });
            }
            return list.Count;
        }
    }
}
