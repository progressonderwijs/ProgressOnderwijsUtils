using System.Buffers;
using System.Data.Common;
using System.Text.Json;

namespace ProgressOnderwijsUtils;

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

public interface ITypedSqlCommand<out TQueryReturnValue, out TSelf> : IWithTimeout<TSelf>
    where TSelf : ITypedSqlCommand<TQueryReturnValue, TSelf>
{
    [UsefulToKeep("lib method")]
    [MustUseReturnValue]
    TQueryReturnValue Execute(SqlConnection conn);
}

public readonly record struct NonQuerySqlCommand(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<Unit, NonQuerySqlCommand>
{
    public NonQuerySqlCommand WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout, };

    Unit ITypedSqlCommand<Unit, NonQuerySqlCommand>.Execute(SqlConnection conn)
    {
        Execute(conn, out _);
        return Unit.Value;
    }

    public void Execute(SqlConnection conn)
        => Execute(conn, out _);

    public void Execute(SqlConnection conn, out int nrOfRowsAffected)
    {
        using var cmd = this.ReusableCommand(conn);
        try {
            if (string.IsNullOrWhiteSpace(cmd.Command.CommandText)) {
                nrOfRowsAffected = 0;
                return;
            }
            nrOfRowsAffected = cmd.Command.ExecuteNonQuery();
        } catch (Exception e) {
            throw cmd.CreateExceptionWithTextAndArguments(e, this);
        }
    }
}

public readonly record struct DbColumnSchemaCommand(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<DbColumn[], DbColumnSchemaCommand>
{
    public DbColumnSchemaCommand WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout, };

    public DbColumn[] Execute(SqlConnection conn)
    {
        using var cmd = this.ReusableCommand(conn);
        try {
            using var sqlReader = cmd.Command.ExecuteReader(CommandBehavior.SchemaOnly);
            return sqlReader.GetColumnSchema().ToArray();
        } catch (Exception e) {
            throw cmd.CreateExceptionWithTextAndArguments(e, this);
        }
    }
}

/// <summary>
/// Executes a DataTable-returning query op basis van het huidige commando met de huidige parameters
/// </summary>
public readonly record struct DataTableSqlCommand(ParameterizedSql Sql, CommandTimeout CommandTimeout, MissingSchemaAction MissingSchemaAction) : ITypedSqlCommand<DataTable, DataTableSqlCommand>
{
    public DataTableSqlCommand WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout, };

    [UsefulToKeep("lib method")]
    public DataTableSqlCommand WithMissingSchemaAction(MissingSchemaAction missingSchemaAction)
        => this with { MissingSchemaAction = missingSchemaAction, };

    [MustUseReturnValue]
    public DataTable Execute(SqlConnection conn)
    {
        using var cmd = this.ReusableCommand(conn);
        using var adapter = new SqlDataAdapter(cmd.Command);
        try {
            adapter.MissingSchemaAction = MissingSchemaAction;
            var dt = new DataTable();
            _ = adapter.Fill(dt);
            return dt;
        } catch (Exception e) {
            throw cmd.CreateExceptionWithTextAndArguments(e, this);
        }
    }
}

public readonly record struct ScalarSqlCommand<T>(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<T?, ScalarSqlCommand<T>>
{
    public ScalarSqlCommand<T> WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout, };

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

public readonly record struct MaybeSqlCommand<TOut, TCommand>(TCommand underlying) : ITypedSqlCommand<Maybe<TOut, Exception>, MaybeSqlCommand<TOut, TCommand>>
    where TCommand : ITypedSqlCommand<TOut, TCommand>
{
    public MaybeSqlCommand<TOut, TCommand> WithTimeout(CommandTimeout timeout)
        => new(underlying.WithTimeout(timeout));

    [MustUseReturnValue]
    public Maybe<TOut, Exception> Execute(SqlConnection conn)
    {
        try {
            return Maybe.Ok(underlying.Execute(conn));
        } catch (Exception e) {
            return Maybe.Error(e);
        }
    }

    public ParameterizedSql Sql
        => underlying.Sql;

    public CommandTimeout CommandTimeout
        => underlying.CommandTimeout;
}

public readonly record struct BuiltinsSqlCommand<T>(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<T?[], BuiltinsSqlCommand<T>>
{
    public BuiltinsSqlCommand<T> WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout, };

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

public readonly record struct PocosSqlCommand<
    [MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    T
>(ParameterizedSql Sql, CommandTimeout CommandTimeout, FieldMappingMode FieldMapping) : ITypedSqlCommand<T[], PocosSqlCommand<T>>
    where T : IWrittenImplicitly
{
    [UsefulToKeep("lib method")]
    public PocosSqlCommand<T> WithFieldMappingMode(FieldMappingMode fieldMapping)
        => this with { FieldMapping = fieldMapping, };

    public EnumeratedObjectsSqlCommand<T> ToLazilyEnumeratedCommand()
        => new(Sql, CommandTimeout, FieldMapping);

    public PocosSqlCommand<T> WithTimeout(CommandTimeout commandTimeout)
        => this with { CommandTimeout = commandTimeout, };

    public T[] Execute(SqlConnection conn)
    {
        using var cmd = this.ReusableCommand(conn);
        SqlDataReader? reader;
        try {
            reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "ExecuteReader failed");
        }
        using var disposeReader = reader;
        TRowReader<SqlDataReader, T> unpacker;
        try {
            unpacker = ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.ByPocoImpl<T>.DataReaderToSingleRowUnpacker(reader, FieldMapping);
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "DataReaderToSingleRowUnpacker failed");
        }
        var rows = ParameterizedSqlObjectMapper.ReaderToArray(this, reader, unpacker, cmd);
        var nullableVerifier = NonNullableFieldVerifier.VerificationDelegate<T>();
        foreach (var row in rows) {
            var nullablityError = nullableVerifier(row);
            if (nullablityError != null) {
                throw new(nullablityError.JoinStrings("\n"));
            }
        }
        return rows;
    }
}

public readonly record struct JsonSqlCommand(ParameterizedSql Sql, CommandTimeout CommandTimeout) : IWithTimeout<JsonSqlCommand>
{
    public JsonSqlCommand WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout, };

    public void Execute(SqlConnection conn, IBufferWriter<byte> buffer, JsonWriterOptions options)
    {
        using var cmd = this.ReusableCommand(conn);
        var reader = ExecuteReader(cmd);
        using var disposeReader = reader;
        var table = reader.GetColumnSchema()
            .Select(column => (ColumnName: JsonEncodedText.Encode(column.ColumnName), column.DataType, column.DataTypeName))
            .ToArray();

        using var writer = new Utf8JsonWriter(buffer, options);
        writer.WriteStartArray();
        while (Read(cmd, reader)) {
            writer.WriteStartObject();
            for (var i = 0; i < table.Length; i++) {
                if (!reader.IsDBNull(i)) {
                    var name = table[i].ColumnName;
                    var type = table[i].DataType;
                    var sqlType = table[i].DataTypeName;
                    if (type == typeof(bool)) {
                        writer.WriteBoolean(name, reader.GetBoolean(i));
                    } else if (type == typeof(int)) {
                        writer.WriteNumber(name, reader.GetInt32(i));
                    } else if (type == typeof(long)) {
                        writer.WriteNumber(name, reader.GetInt64(i));
                    } else if (type == typeof(decimal)) {
                        writer.WriteNumber(name, reader.GetDecimal(i));
                    } else if (type == typeof(double)) {
                        writer.WriteNumber(name, reader.GetDouble(i));
                    } else if (type == typeof(DateTime)) {
                        var dateTime = reader.GetDateTime(i);
                        if (sqlType == "date") {
                            writer.WriteString(name, dateTime.ToString("yyyy-MM-dd"));
                        } else if (name.ToString().EndsWith("_Utc", StringComparison.OrdinalIgnoreCase)) {
                            writer.WriteString(name, new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)));
                        } else {
                            writer.WriteString(name, new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Local)));
                        }
                    } else if (type == typeof(DateTimeOffset)) {
                        writer.WriteString(name, reader.GetDateTimeOffset(i));
                    } else if (type == typeof(string)) {
                        writer.WriteString(name, reader.GetString(i));
                    } else if (type == typeof(byte[])) {
                        writer.WriteBase64String(name, reader.GetFieldValue<byte[]>(i));
                    } else if (type == typeof(Guid)) {
                        writer.WriteString(name, reader.GetGuid(i));
                    } else {
                        throw cmd.CreateExceptionWithTextAndArguments(new($"Unknown type '{type}' for property '{name}'"), this);
                    }
                }
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    SqlDataReader ExecuteReader(ReusableCommand cmd)
    {
        try {
            return cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "ExecuteReader failed");
        }
    }

    bool Read(ReusableCommand cmd, SqlDataReader reader)
    {
        try {
            return reader.Read();
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "Read failed");
        }
    }
}

public readonly record struct EnumeratedObjectsSqlCommand<T>(ParameterizedSql Sql, CommandTimeout CommandTimeout, FieldMappingMode FieldMapping) : ITypedSqlCommand<IEnumerable<T>, EnumeratedObjectsSqlCommand<T>>
    where T : IWrittenImplicitly
{
    public EnumeratedObjectsSqlCommand<T> WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout, };

    [UsefulToKeep("lib method")]
    public EnumeratedObjectsSqlCommand<T> WithFieldMappingMode(FieldMappingMode fieldMapping)
        => this with { FieldMapping = fieldMapping, };

    [UsefulToKeep("lib method")]
    public PocosSqlCommand<T> ToEagerlyEnumeratedCommand()
        => new(Sql, CommandTimeout, FieldMapping);

    public IEnumerable<T> Execute(SqlConnection conn)
    {
        using var cmd = this.ReusableCommand(conn);
        SqlDataReader? reader;
        try {
            reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "ExecuteReader failed");
        }
        using var disposeReader = reader;
        TRowReader<SqlDataReader, T> unpacker;
        try {
            unpacker = ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.ByPocoImpl<T>.DataReaderToSingleRowUnpacker(reader, FieldMapping);
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "DataReaderToSingleRowUnpacker failed");
        }

        while (true) {
            bool isDone;
            try {
                isDone = !reader.Read();
            } catch (Exception e) {
                throw cmd.CreateExceptionWithTextAndArguments(e, this, "SqlDataReader.Read failed");
            }

            if (isDone) {
                break;
            }
            T nextRow;
            var lastColumnRead = -1;
            try {
                nextRow = unpacker(reader, out lastColumnRead);
            } catch (Exception e) {
                throw cmd.CreateExceptionWithTextAndArguments(e, this, ParameterizedSqlObjectMapper.UnpackingErrorMessage<T>(reader, lastColumnRead));
            }
            var nullableVerifier = NonNullableFieldVerifier.VerificationDelegate<T>();
            var nullablityError = nullableVerifier(nextRow);
            if (nullablityError != null) {
                throw new(nullablityError.JoinStrings("\n"));
            }
            yield return nextRow; //cannot yield in try-catch block
        }
    }
}

public readonly record struct TuplesSqlCommand<
    [MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    T
>(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<T[], TuplesSqlCommand<T>>
    where T : struct, IStructuralEquatable, ITuple
{
    public EnumeratedTuplesSqlCommand<T> ToLazilyEnumeratedCommand()
        => new(Sql, CommandTimeout);

    public TuplesSqlCommand<T> WithTimeout(CommandTimeout commandTimeout)
        => this with { CommandTimeout = commandTimeout, };

    public T[] Execute(SqlConnection conn)
    {
        using var cmd = this.ReusableCommand(conn);
        SqlDataReader? reader;
        try {
            reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "ExecuteReader failed");
        }
        using var disposeReader = reader;
        TRowReader<SqlDataReader, T> unpacker;
        try {
            unpacker = ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.Tuples<T>.GetRowReader(reader);
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "DataReaderToSingleRowUnpacker failed");
        }
        return ParameterizedSqlObjectMapper.ReaderToArray(this, reader, unpacker, cmd);
    }
}

public readonly record struct EnumeratedTuplesSqlCommand<T>(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<IEnumerable<T>, EnumeratedTuplesSqlCommand<T>>
    where T : struct, IStructuralEquatable, ITuple
{
    public EnumeratedTuplesSqlCommand<T> WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout, };

    [UsefulToKeep("lib method")]
    public TuplesSqlCommand<T> ToEagerlyEnumeratedCommand()
        => new(Sql, CommandTimeout);

    public IEnumerable<T> Execute(SqlConnection conn)
    {
        using var cmd = this.ReusableCommand(conn);
        SqlDataReader? reader;
        try {
            reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "ExecuteReader failed");
        }
        using var disposeReader = reader;
        TRowReader<SqlDataReader, T> unpacker;
        try {
            unpacker = ParameterizedSqlObjectMapper.DataReaderSpecialization<SqlDataReader>.Tuples<T>.GetRowReader(reader);
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, this, "DataReaderToSingleRowUnpacker failed");
        }

        while (true) {
            bool isDone;
            try {
                isDone = !reader.Read();
            } catch (Exception e) {
                throw cmd.CreateExceptionWithTextAndArguments(e, this, "SqlDataReader.Read failed");
            }

            if (isDone) {
                break;
            }
            T nextRow;
            var lastColumnRead = -1;
            try {
                nextRow = unpacker(reader, out lastColumnRead);
            } catch (Exception e) {
                throw cmd.CreateExceptionWithTextAndArguments(e, this, ParameterizedSqlObjectMapper.UnpackingErrorMessage<T>(reader, lastColumnRead));
            }
            yield return nextRow; //cannot yield in try-catch block
        }
    }
}
