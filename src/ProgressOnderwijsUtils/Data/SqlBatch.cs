using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public static ReusableCommand ReusableCommand<T>(this T batch, SqlConnection conn)
            where T : INestableSql, IDefinesBatchTimeout
            => batch.Sql.CreateSqlCommand(conn, batch.BatchTimeout);
    }

    public interface INestableSql
    {
        ParameterizedSql Sql { get; }
    }

    public interface IDefinesBatchTimeout
    {
        BatchTimeout BatchTimeout { get; }
    }

    public interface IExecutableBatch<out TQueryReturnValue>
    {
        TQueryReturnValue Execute([NotNull] SqlConnection conn);
    }

    public readonly struct BatchNonQuery : INestableSql, IDefinesBatchTimeout, IExecutableBatch<int>
    {
        public ParameterizedSql Sql { get; }
        public BatchTimeout BatchTimeout { get; }

        public BatchNonQuery(ParameterizedSql sql, BatchTimeout timeout)
            => (Sql, BatchTimeout) = (sql, timeout);

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
    public readonly struct BatchOfDataTable : INestableSql, IDefinesBatchTimeout, IExecutableBatch<DataTable>
    {
        public ParameterizedSql Sql { get; }
        public BatchTimeout BatchTimeout { get; }
        public MissingSchemaAction MissingSchemaAction { get; }

        public BatchOfDataTable(ParameterizedSql sql, BatchTimeout timeout, MissingSchemaAction missingSchemaAction)
            => (Sql, BatchTimeout, MissingSchemaAction) = (sql, timeout, missingSchemaAction);

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

    public readonly struct BatchOfScalar<T> : INestableSql, IDefinesBatchTimeout, IExecutableBatch<T>
    {
        public ParameterizedSql Sql { get; }
        public BatchTimeout BatchTimeout { get; }

        public BatchOfScalar(ParameterizedSql sql, BatchTimeout timeout)
            => (Sql, BatchTimeout) = (sql, timeout);

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

    public readonly struct BatchOfBuiltins<T> : INestableSql, IDefinesBatchTimeout, IExecutableBatch<T[]>
    {
        public ParameterizedSql Sql { get; }
        public BatchTimeout BatchTimeout { get; }

        public BatchOfBuiltins(ParameterizedSql sql, BatchTimeout timeout)
            => (Sql, BatchTimeout) = (sql, timeout);

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
    > : INestableSql, IDefinesBatchTimeout, IExecutableBatch<T[]>
        where T : IMetaObject, new()
    {
        public ParameterizedSql Sql { get; }
        public BatchTimeout BatchTimeout { get; }
        public readonly FieldMappingMode FieldMapping;

        public BatchOfObjects(ParameterizedSql sql, BatchTimeout timeout, FieldMappingMode fieldMapping)
            => (Sql, BatchTimeout, FieldMapping) = (sql, timeout, fieldMapping);

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
            => new BatchOfObjects<T>(Sql, BatchTimeout, fieldMapping);

        public LazyBatchOfObjects<T> EnumerateLazily()
            => new LazyBatchOfObjects<T>(Sql, BatchTimeout, FieldMapping);
    }

    public readonly struct LazyBatchOfObjects<T> : INestableSql, IDefinesBatchTimeout, IExecutableBatch<IEnumerable<T>>
        where T : IMetaObject, new()
    {
        public ParameterizedSql Sql { get; }
        public BatchTimeout BatchTimeout { get; }
        public readonly FieldMappingMode FieldMapping;

        public LazyBatchOfObjects(ParameterizedSql sql, BatchTimeout timeout, FieldMappingMode fieldMapping)
            => (Sql, BatchTimeout, FieldMapping) = (sql, timeout, fieldMapping);

        public IEnumerable<T> Execute(SqlConnection conn)
        {
            //return Sql.EnumerateMetaObjects<T>(conn.AsTmpContext(Timeout), FieldMapping);
            var cmd = this.ReusableCommand(conn);
            SqlDataReader reader = null;
            var lastColumnRead = -1;
            ParameterizedSqlExecutionException CreateHelpfulException(Exception ex)
                => cmd.CreateExceptionWithTextAndArguments(CurrentMethodName<T>() + " failed. " + ParameterizedSqlObjectMapper.UnpackingErrorMessage<T>(reader, lastColumnRead), ex);

            try {
                ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.TRowReader<T> unpacker;
                try {
                    reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
                    unpacker = ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.ByMetaObjectImpl<T>.DataReaderToSingleRowUnpacker(reader, FieldMapping);
                } catch (Exception e) {
                    throw CreateHelpfulException(e);
                }

                while (true) {
                    bool isDone;
                    try {
                        isDone = !reader.Read();
                    } catch (Exception e) {
                        throw CreateHelpfulException(e);
                    }

                    if (isDone) {
                        break;
                    }
                    T nextRow;
                    try {
                        nextRow = unpacker(reader, out lastColumnRead);
                    } catch (Exception e) {
                        throw CreateHelpfulException(e);
                    }

                    yield return nextRow; //cannot yield in try-catch block
                }
            } finally {
                reader?.Dispose();
                cmd.Dispose();
            }
        }
    }
}
