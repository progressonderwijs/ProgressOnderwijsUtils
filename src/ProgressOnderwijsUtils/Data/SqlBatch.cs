using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Text;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    using static ErrorMessageHelpers;

    static class ErrorMessageHelpers
    {
        public static string CurrentMethodName<T>()
            => typeof(T).ToCSharpFriendlyTypeName();

        public static SqlCommandCreationContext AsTmpContext(this SqlConnection conn, int? timeout)
            => new SqlCommandCreationContext(conn, conn.TimeoutWithFallback(timeout), Tracer(conn));

        public static ISqlCommandTracer Tracer(this SqlConnection conn)
            => (conn.Site as IAttachedToTracer)?.Tracer;

        public static int TimeoutWithFallback(this SqlConnection conn, int? timeout)
            => timeout ?? (conn.Site as IHasDefaultCommandTimeout)?.DefaultCommandTimeoutInS ?? 0;

        public static ReusableCommand ReusableCommand<T>(this T batch, SqlConnection conn)
            where T : INestableSql, IDefinesTimeoutInSeconds
            => batch.Sql.CreateSqlCommand(conn, batch.Timeout);
    }

    public interface INestableSql
    {
        ParameterizedSql Sql { get; }
    }

    public interface IDefinesTimeoutInSeconds
    {
        int? Timeout { get; }
    }

    public interface IExecutableBatch<out TQueryReturnValue>
    {
        TQueryReturnValue Execute([NotNull] SqlConnection conn);
    }

    public readonly struct BatchNonQuery : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<int>
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }

        public BatchNonQuery(ParameterizedSql sql, int? timeout)
            => (Sql, Timeout) = (sql, timeout);

        public int Execute(SqlConnection conn)
        {
            using (var cmd = this.ReusableCommand(conn)) {
                try {
                    if (string.IsNullOrWhiteSpace(cmd.Command.CommandText)) {
                        return 0;
                    }
                    return cmd.Command.ExecuteNonQuery();
                } catch (Exception e) {
                    throw cmd.CreateExceptionWithTextAndArguments(nameof(ParameterizedSqlObjectMapper.ExecuteNonQuery) + " failed", e);
                }
            }
        }
    }

    /// <summary>
    /// Executes a DataTable-returning query op basis van het huidige commando met de huidige parameters
    /// </summary>
    public readonly struct BatchOfDataTable : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<DataTable>
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }
        public MissingSchemaAction MissingSchemaAction { get; }

        public BatchOfDataTable(ParameterizedSql sql, int? timeout, MissingSchemaAction missingSchemaAction)
            => (Sql, Timeout, MissingSchemaAction) = (sql, timeout, missingSchemaAction);

        [MustUseReturnValue]
        [NotNull]
        public DataTable Execute(SqlConnection conn)
        {
            using (var cmd = this.ReusableCommand(conn))
            using (var adapter = new SqlDataAdapter(cmd.Command)) {
                try {
                    adapter.MissingSchemaAction = MissingSchemaAction;
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                } catch (Exception e) {
                    throw cmd.CreateExceptionWithTextAndArguments(nameof(ParameterizedSqlObjectMapper.ReadDataTable) + "() failed", e);
                }
            }
        }
    }

    public readonly struct BatchOfScalar<T> : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<T>
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }

        public BatchOfScalar(ParameterizedSql sql, int? timeout)
            => (Sql, Timeout) = (sql, timeout);

        [MustUseReturnValue]
        public T Execute(SqlConnection conn)
        {
            using (var cmd = this.ReusableCommand(conn)) {
                try {
                    var value = cmd.Command.ExecuteScalar();

                    return DbValueConverter.FromDb<T>(value);
                } catch (Exception e) {
                    throw cmd.CreateExceptionWithTextAndArguments(CurrentMethodName<T>() + " failed.", e);
                }
            }
        }
    }

    public readonly struct BatchOfBuiltins<T> : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<T[]>
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }

        public BatchOfBuiltins(ParameterizedSql sql, int? timeout)
            => (Sql, Timeout) = (sql, timeout);

        public T[] Execute(SqlConnection conn)
        {
            using (var cmd = this.ReusableCommand(conn)) {
                try {
                    return ParameterizedSqlObjectMapper.ReadPlainUnpacker<T>(cmd.Command);
                } catch (Exception e) {
                    throw cmd.CreateExceptionWithTextAndArguments(CurrentMethodName<T>() + " failed.", e);
                }
            }
        }
    }

    public readonly struct BatchOfObjects<
        [MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
        T
    > : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<T[]>
        where T : IMetaObject, new()
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }
        public readonly FieldMappingMode FieldMapping;

        public BatchOfObjects(ParameterizedSql sql, int? timeout, FieldMappingMode fieldMapping)
            => (Sql, Timeout, FieldMapping) = (sql, timeout, fieldMapping);

        public T[] Execute(SqlConnection conn)
        {
            using (var cmd = this.ReusableCommand(conn)) {
                var lastColumnRead = -1;
                SqlDataReader reader = null;
                try {
                    reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
                    var unpacker = ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.ByMetaObjectImpl<T>.DataReaderToSingleRowUnpacker(reader, FieldMapping);
                    var builder = new ArrayBuilder<T>();
                    while (reader.Read()) {
                        var nextRow = unpacker(reader, out lastColumnRead);
                        builder.Add(nextRow);
                    }
                    return builder.ToArray();
                } catch (Exception ex) {
                    throw cmd.CreateExceptionWithTextAndArguments(CurrentMethodName<T>() + " failed. " + ParameterizedSqlObjectMapper.UnpackingErrorMessage<T>(reader, lastColumnRead), ex);
                } finally {
                    reader?.Dispose();
                }
            }
        }

        public BatchOfObjects<T> WithFieldMappingMode(FieldMappingMode fieldMapping)
            => new BatchOfObjects<T>(Sql, Timeout, fieldMapping);

        public LazyBatchOfObjects<T> EnumerateLazily()
            => new LazyBatchOfObjects<T>(Sql, Timeout, FieldMapping);
    }

    public readonly struct LazyBatchOfObjects<T> : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<IEnumerable<T>>
        where T : IMetaObject, new()
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }
        public readonly FieldMappingMode FieldMapping;

        public LazyBatchOfObjects(ParameterizedSql sql, int? timeout, FieldMappingMode fieldMapping)
            => (Sql, Timeout, FieldMapping) = (sql, timeout, fieldMapping);

        public IEnumerable<T> Execute(SqlConnection conn)
            => Sql.ReadMetaObjects<T>(conn.AsTmpContext(Timeout), FieldMapping);
    }
}
