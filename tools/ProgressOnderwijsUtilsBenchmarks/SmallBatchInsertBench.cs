using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Microsoft.Data.SqlClient;
using Perfolizer.Mathematics.OutlierDetection;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Tests.Data;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsBenchmarks
{
    // ReSharper disable once ClassCanBeSealed.Global - BenchmarkDotNet complexity
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    public class SmallBatchInsertBench : IDisposable
    {
        sealed class Config : ManualConfig
        {
            public Config()
            {
                AddJob(
                    Job.MediumRun
                        .WithLaunchCount(1)
                        .WithWarmupCount(3)
                        .WithOutlierMode(OutlierMode.DontRemove)
                        .WithMaxIterationCount(5000)
                        .WithMaxRelativeError(0.002)
                        .WithToolchain(InProcessNoEmitToolchain.Instance)
                        .WithId("InProcess")
                );
            }
        }

        readonly SqlConnection sqlConn;
        readonly BulkInsertTarget cachedTarget;
        static readonly ParameterizedSql tableName = SQL($"#test");

        public void Dispose()
            => sqlConn.Dispose();

        static BulkInsertTestSampleRow[] SampleData(int n)
            => Enumerable.Range(0, (n + 3) / 4).SelectMany(i => BulkInsertTestSampleRow.SampleData).ToArray();

        public SmallBatchInsertBench()
        {
            sqlConn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB");
            sqlConn.Open();
            ParameterizedSql.TableValuedTypeDefinitionScripts.ExecuteNonQuery(sqlConn);
            cachedTarget = BulkInsertTestSampleRow.CreateTable(sqlConn, tableName);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
            => SQL($"truncate table {tableName}").ExecuteNonQuery(sqlConn);

        [Benchmark]
        public void BulkInsertTarget_LoadFromTable()
            => BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText());

        [Benchmark]
        public void BulkCopyToSqlServer_cachedTarget_0()
            => SampleData(0).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_cachedTarget_1()
            => SampleData(1).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_cachedTarget_9()
            => SampleData(9).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_cachedTarget_81()
            => SampleData(81).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_cachedTarget_729()
            => SampleData(729).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_0()
            => SampleData(0).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));

        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_1()
            => SampleData(1).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));

        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_9()
            => SampleData(9).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));
        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_81()
            => SampleData(81).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));

        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_729()
            => SampleData(729).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));
    }
}
