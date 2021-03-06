﻿using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    static class ErrorMessageHelpers
    {
        public static ReusableCommand ReusableCommand(this INestableSql dbCommand, SqlConnection conn)
            => dbCommand.Sql.CreateSqlCommand(conn, dbCommand.CommandTimeout);
    }

    public interface INestableSql
    {
        ParameterizedSql Sql { get; }
        CommandTimeout CommandTimeout { get; }
    }


    public interface IWithTimeout<out TSelf> : INestableSql
        where TSelf : IWithTimeout<TSelf>
    {
        TSelf WithTimeout(CommandTimeout timeout);
    }

    public interface ITypedSqlCommand<out TQueryReturnValue>
    {
        [UsefulToKeep("lib method")]
        [MustUseReturnValue]
        TQueryReturnValue Execute(SqlConnection conn);
    }

    public readonly struct NonQuerySqlCommand : IWithTimeout<NonQuerySqlCommand>
    {
        public ParameterizedSql Sql { get; }
        public CommandTimeout CommandTimeout { get; }

        public NonQuerySqlCommand WithTimeout(CommandTimeout timeout)
            => new(Sql, timeout);

        public NonQuerySqlCommand(ParameterizedSql sql, CommandTimeout timeout)
            => (Sql, CommandTimeout) = (sql, timeout);

        public int Execute(SqlConnection conn)
        {
            using var cmd = this.ReusableCommand(conn);
            try {
                if (string.IsNullOrWhiteSpace(cmd.Command.CommandText)) {
                    return 0;
                }
                return cmd.Command.ExecuteNonQuery();
            } catch (Exception e) {
                throw cmd.CreateExceptionWithTextAndArguments(e, this);
            }
        }
    }

    /// <summary>
    /// Executes a DataTable-returning query op basis van het huidige commando met de huidige parameters
    /// </summary>
    public readonly struct DataTableSqlCommand : ITypedSqlCommand<DataTable>, IWithTimeout<DataTableSqlCommand>
    {
        public ParameterizedSql Sql { get; }
        public CommandTimeout CommandTimeout { get; }
        public MissingSchemaAction MissingSchemaAction { get; }

        public DataTableSqlCommand WithTimeout(CommandTimeout timeout)
            => new(Sql, timeout, MissingSchemaAction);

        [UsefulToKeep("lib method")]
        public DataTableSqlCommand WithMissingSchemaAction(MissingSchemaAction missingSchemaAction)
            => new(Sql, CommandTimeout, missingSchemaAction);

        public DataTableSqlCommand(ParameterizedSql sql, CommandTimeout timeout, MissingSchemaAction missingSchemaAction)
            => (Sql, CommandTimeout, MissingSchemaAction) = (sql, timeout, missingSchemaAction);

        [MustUseReturnValue]
        public DataTable Execute(SqlConnection conn)
        {
            using var cmd = this.ReusableCommand(conn);
            using var adapter = new SqlDataAdapter(cmd.Command);
            try {
                adapter.MissingSchemaAction = MissingSchemaAction;
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            } catch (Exception e) {
                throw cmd.CreateExceptionWithTextAndArguments(e, this);
            }
        }
    }

    public readonly struct ScalarSqlCommand<T> : ITypedSqlCommand<T?>, IWithTimeout<ScalarSqlCommand<T>>
    {
        public ParameterizedSql Sql { get; }
        public CommandTimeout CommandTimeout { get; }

        public ScalarSqlCommand(ParameterizedSql sql, CommandTimeout timeout)
            => (Sql, CommandTimeout) = (sql, timeout);

        public ScalarSqlCommand<T> WithTimeout(CommandTimeout timeout)
            => new(Sql, timeout);

        [MustUseReturnValue]
        public T? Execute(SqlConnection conn)
        {
            using var cmd = this.ReusableCommand(conn);
            try {
                var value = cmd.Command.ExecuteScalar();

                return DbValueConverter.FromDb<T>(value);
            } catch (Exception e) {
                throw cmd.CreateExceptionWithTextAndArguments(e, this);
            }
        }
    }

    public readonly struct BuiltinsSqlCommand<T> : ITypedSqlCommand<T?[]>, IWithTimeout<BuiltinsSqlCommand<T>>
    {
        public ParameterizedSql Sql { get; }
        public CommandTimeout CommandTimeout { get; }

        public BuiltinsSqlCommand(ParameterizedSql sql, CommandTimeout timeout)
            => (Sql, CommandTimeout) = (sql, timeout);

        public BuiltinsSqlCommand<T> WithTimeout(CommandTimeout timeout)
            => new(Sql, timeout);

        public T?[] Execute(SqlConnection conn)
        {
            using var cmd = this.ReusableCommand(conn);
            try {
                return ParameterizedSqlObjectMapper.ReadPlainUnpacker<T>(cmd.Command);
            } catch (Exception e) {
                throw cmd.CreateExceptionWithTextAndArguments(e, this);
            }
        }
    }

    public readonly struct PocosSqlCommand<
        [MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
        T
    > : ITypedSqlCommand<T[]>, IWithTimeout<PocosSqlCommand<T>>
        where T : IWrittenImplicitly
    {
        public ParameterizedSql Sql { get; }
        public CommandTimeout CommandTimeout { get; }
        public readonly FieldMappingMode FieldMapping;

        public PocosSqlCommand(ParameterizedSql sql, CommandTimeout timeout, FieldMappingMode fieldMapping)
            => (Sql, CommandTimeout, FieldMapping) = (sql, timeout, fieldMapping);

        [UsefulToKeepAttribute("lib method")]
        public PocosSqlCommand<T> WithFieldMappingMode(FieldMappingMode fieldMapping)
            => new(Sql, CommandTimeout, fieldMapping);

        public EnumeratedObjectsSqlCommand<T> ToLazilyEnumeratedCommand()
            => new(Sql, CommandTimeout, FieldMapping);

        public PocosSqlCommand<T> WithTimeout(CommandTimeout commandTimeout)
            => new(Sql, commandTimeout, FieldMapping);

        public T[] Execute(SqlConnection conn)
        {
            using var cmd = this.ReusableCommand(conn);
            var lastColumnRead = -1;
            SqlDataReader? reader = null;
            try {
                reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
                var unpacker = ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.ByPocoImpl<T>.DataReaderToSingleRowUnpacker(reader, FieldMapping);
                var builder = new ArrayBuilder<T>();
                while (reader.Read()) {
                    var nextRow = unpacker(reader, out lastColumnRead);
                    builder.Add(nextRow);
                }
                return builder.ToArray();
            } catch (Exception ex) {
                throw cmd.CreateExceptionWithTextAndArguments(ex, this, ParameterizedSqlObjectMapper.UnpackingErrorMessage<T>(reader, lastColumnRead));
            } finally {
                reader?.Dispose();
            }
        }
    }

    public readonly struct EnumeratedObjectsSqlCommand<T> : ITypedSqlCommand<IEnumerable<T>>, IWithTimeout<EnumeratedObjectsSqlCommand<T>>
        where T : IWrittenImplicitly
    {
        public ParameterizedSql Sql { get; }
        public CommandTimeout CommandTimeout { get; }
        public readonly FieldMappingMode FieldMapping;

        public EnumeratedObjectsSqlCommand(ParameterizedSql sql, CommandTimeout timeout, FieldMappingMode fieldMapping)
            => (Sql, CommandTimeout, FieldMapping) = (sql, timeout, fieldMapping);

        public EnumeratedObjectsSqlCommand<T> WithTimeout(CommandTimeout timeout)
            => new(Sql, timeout, FieldMapping);

        [UsefulToKeep("lib method")]
        public EnumeratedObjectsSqlCommand<T> WithFieldMappingMode(FieldMappingMode fieldMapping)
            => new(Sql, CommandTimeout, fieldMapping);

        [UsefulToKeep("lib method")]
        public PocosSqlCommand<T> ToEagerlyEnumeratedCommand()
            => new(Sql, CommandTimeout, FieldMapping);

        public IEnumerable<T> Execute(SqlConnection conn)
        {
            var cmd = this.ReusableCommand(conn);
            SqlDataReader? reader = null;
            var lastColumnRead = -1;
            ParameterizedSqlExecutionException CreateHelpfulException(Exception ex, EnumeratedObjectsSqlCommand<T> o)
                => cmd.CreateExceptionWithTextAndArguments(ex, o, ParameterizedSqlObjectMapper.UnpackingErrorMessage<T>(reader, lastColumnRead));

            try {
                ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.TRowReader<T> unpacker;
                try {
                    reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
                    unpacker = ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.ByPocoImpl<T>.DataReaderToSingleRowUnpacker(reader, FieldMapping);
                } catch (Exception e) {
                    throw CreateHelpfulException(e, this);
                }

                while (true) {
                    bool isDone;
                    try {
                        isDone = !reader.Read();
                    } catch (Exception e) {
                        throw CreateHelpfulException(e, this);
                    }

                    if (isDone) {
                        break;
                    }
                    T nextRow;
                    try {
                        nextRow = unpacker(reader, out lastColumnRead);
                    } catch (Exception e) {
                        throw CreateHelpfulException(e, this);
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
