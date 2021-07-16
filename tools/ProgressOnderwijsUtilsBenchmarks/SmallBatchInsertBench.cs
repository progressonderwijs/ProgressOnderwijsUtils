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
            => BulkInsertTestSampleRow.SampleRows(0).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_cachedTarget_1()
            => BulkInsertTestSampleRow.SampleRows(1).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_cachedTarget_9()
            => BulkInsertTestSampleRow.SampleRows(9).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_cachedTarget_81()
            => BulkInsertTestSampleRow.SampleRows(81).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_cachedTarget_729()
            => BulkInsertTestSampleRow.SampleRows(729).BulkCopyToSqlServer(sqlConn, cachedTarget);

        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_0()
            => BulkInsertTestSampleRow.SampleRows(0).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));

        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_1()
            => BulkInsertTestSampleRow.SampleRows(1).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));

        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_9()
            => BulkInsertTestSampleRow.SampleRows(9).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));
        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_81()
            => BulkInsertTestSampleRow.SampleRows(81).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));

        [Benchmark]
        public void BulkCopyToSqlServer_uncachedTarget_729()
            => BulkInsertTestSampleRow.SampleRows(729).BulkCopyToSqlServer(sqlConn, BulkInsertTarget.LoadFromTable(sqlConn, tableName.CommandText()));
    }
}
