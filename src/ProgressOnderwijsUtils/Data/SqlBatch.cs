using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    using static ErrorMessageHelpers;

    static class ErrorMessageHelpers
    {
        public static string CurrentMethodName<T>()
            => typeof(T).ToCSharpFriendlyTypeName();
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
        TQueryReturnValue Execute([NotNull] SqlCommandCreationContext ctx);
    }

    public readonly struct BatchNonQuery : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<int>
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }

        public BatchNonQuery(ParameterizedSql sql, int? timeout)
            => (Sql, Timeout) = (sql, timeout);

        public int Execute(SqlCommandCreationContext ctx)
            => Sql.ExecuteNonQuery(Timeout == null ? ctx : ctx.OverrideTimeout(Timeout.Value));
    }

    public readonly struct BatchOfDataTable : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<DataTable>
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }
        public MissingSchemaAction MissingSchemaAction { get; }

        public BatchOfDataTable(ParameterizedSql sql, int? timeout, MissingSchemaAction missingSchemaAction)
            => (Sql, Timeout, MissingSchemaAction) = (sql, timeout, missingSchemaAction);

        public DataTable Execute(SqlCommandCreationContext ctx)
            => Sql.ReadDataTable(Timeout == null ? ctx : ctx.OverrideTimeout(Timeout.Value), MissingSchemaAction);
    }

    public readonly struct BatchOfScalar<T> : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<T>
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }

        public BatchOfScalar(ParameterizedSql sql, int? timeout)
            => (Sql, Timeout) = (sql, timeout);

        [MustUseReturnValue]
        public T Execute([NotNull] SqlCommandCreationContext ctx)
        {
            using (var cmd = Sql.CreateSqlCommand(ctx.Connection, Timeout ?? ctx.CommandTimeoutInS, ctx.Tracer)) {
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

        public T[] Execute(SqlCommandCreationContext ctx)
            => Sql.ReadPlain<T>(Timeout == null ? ctx : ctx.OverrideTimeout(Timeout.Value));
    }

    public readonly struct BatchOfObjects<T> : INestableSql, IDefinesTimeoutInSeconds, IExecutableBatch<T[]>
        where T : IMetaObject, new()
    {
        public ParameterizedSql Sql { get; }
        public int? Timeout { get; }
        public readonly FieldMappingMode FieldMapping;

        public BatchOfObjects(ParameterizedSql sql, int? timeout, FieldMappingMode fieldMapping)
            => (Sql, Timeout, FieldMapping) = (sql, timeout, fieldMapping);

        public T[] Execute(SqlCommandCreationContext ctx)
            => Sql.ReadMetaObjects<T>(Timeout == null ? ctx : ctx.OverrideTimeout(Timeout.Value), FieldMapping);

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

        public IEnumerable<T> Execute(SqlCommandCreationContext ctx)
            => Sql.ReadMetaObjects<T>(Timeout == null ? ctx : ctx.OverrideTimeout(Timeout.Value), FieldMapping);
    }
}
