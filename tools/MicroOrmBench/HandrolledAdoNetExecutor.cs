using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using ProgressOnderwijsUtils;

namespace MicroOrmBench
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

        static int ExecuteQuery(SqlCommandCreationContext ctx, int rows)
        {
            using (var cmd = new SqlCommand()) {
                cmd.CommandText = ExampleObject.RawQueryString;
                cmd.Connection = ctx.Connection;
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
                using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess)) {
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
            }
        }

        static int ExecuteSqliteQuery(SQLiteConnection ctx, int rows)
        {
            var conn = ctx;
            using (var cmd = conn.CreateCommand()) {
                cmd.CommandText = ExampleObject.RawSqliteQueryString;
                cmd.Connection = ctx;
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
                using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess)) {
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
            }
        }

        static readonly ConcurrentDictionary<SQLiteConnection, Dictionary<string, SQLiteCommand>> connCmdCache = new ConcurrentDictionary<SQLiteConnection, Dictionary<string, SQLiteCommand>>();

        static int ExecuteSqliteQueryCached(SQLiteConnection ctx, int rows)
        {
            if (!connCmdCache.TryGetValue(ctx, out var cmdCache)) {
                cmdCache = connCmdCache.GetOrAdd(ctx, conn => {
                    conn.Disposed += (o, e) => connCmdCache.TryRemove(conn, out var _);
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
                sqliteCmd.Connection = ctx;
            }

            sqliteCmd.Parameters[0].Value = rows;
            sqliteCmd.Parameters[1].Value = 2;
            sqliteCmd.Parameters[2].Value = "hehe";
            sqliteCmd.Parameters[3].Value = ExampleObject.someInt64Value;

            var list = new List<ExampleObject>();
            using (var reader = sqliteCmd.ExecuteReader(CommandBehavior.SequentialAccess))
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
            //cmd.Connection = null;
            sqliteCmd.Parameters[0].Value = null;
            sqliteCmd.Parameters[1].Value = null;
            sqliteCmd.Parameters[2].Value = null;
            sqliteCmd.Parameters[3].Value = null;
            return list.Count;
        }

        // ReSharper disable once UnusedMember.Local
        static int ExecuteQuery2(SqlCommandCreationContext ctx, int rows)
        {
            using (var cmd = new SqlCommand()) {
                cmd.CommandText = ExampleObject.RawQueryString;
                cmd.Connection = ctx.Connection;
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
                using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess)) {
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
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int? ToNullableInt(this SqlInt32 num) => num.IsNull ? default(int?) : num.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool? ToNullableBool(this SqlBoolean num) => num.IsNull ? default(bool?) : num.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once UnusedMember.Local
        static string ToNullableString(this SqlString str) => str.IsNull ? default(string) : str.Value;

        public static void RunWideQuery(Benchmarker benchmarker)
        {
            benchmarker.BenchSqlServer("Raw (26-col)", ExecuteWideQuery);
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
                            OnlineOrderFlag = reader.GetBoolean(6),
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
                            SubTotal = reader.GetDecimal(19),
                            TaxAmt = reader.GetDecimal(20),
                            Freight = reader.GetDecimal(21),
                            TotalDue = reader.GetDecimal(22),
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
