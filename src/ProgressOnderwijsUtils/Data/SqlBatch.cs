using ProgressOnderwijsUtils.RequiredFields;

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

public interface ITypedSqlCommand<out TQueryReturnValue>
{
    [UsefulToKeep("lib method")]
    [MustUseReturnValue]
    TQueryReturnValue Execute(SqlConnection conn);
}

public readonly record struct NonQuerySqlCommand(ParameterizedSql Sql, CommandTimeout CommandTimeout) : IWithTimeout<NonQuerySqlCommand>
{
    public NonQuerySqlCommand WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout };

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

/// <summary>
/// Executes a DataTable-returning query op basis van het huidige commando met de huidige parameters
/// </summary>
public readonly record struct DataTableSqlCommand(ParameterizedSql Sql, CommandTimeout CommandTimeout, MissingSchemaAction MissingSchemaAction) : ITypedSqlCommand<DataTable>, IWithTimeout<DataTableSqlCommand>
{
    public DataTableSqlCommand WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout };

    [UsefulToKeep("lib method")]
    public DataTableSqlCommand WithMissingSchemaAction(MissingSchemaAction missingSchemaAction)
        => this with { MissingSchemaAction = missingSchemaAction };

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

public readonly record struct ScalarSqlCommand<T>(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<T?>, IWithTimeout<ScalarSqlCommand<T>>
{
    public ScalarSqlCommand<T> WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout };

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

public readonly record struct BuiltinsSqlCommand<T>(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<T?[]>, IWithTimeout<BuiltinsSqlCommand<T>>
{
    public BuiltinsSqlCommand<T> WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout };

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
>(ParameterizedSql Sql, CommandTimeout CommandTimeout, FieldMappingMode FieldMapping) : ITypedSqlCommand<T[]>, IWithTimeout<PocosSqlCommand<T>>
    where T : IWrittenImplicitly
{
    [UsefulToKeep("lib method")]
    public PocosSqlCommand<T> WithFieldMappingMode(FieldMappingMode fieldMapping)
        => this with { FieldMapping = fieldMapping };

    public EnumeratedObjectsSqlCommand<T> ToLazilyEnumeratedCommand()
        => new(Sql, CommandTimeout, FieldMapping);

    public PocosSqlCommand<T> WithTimeout(CommandTimeout commandTimeout)
        => this with { CommandTimeout = commandTimeout };

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
        var readerToArray = ParameterizedSqlObjectMapper.ReaderToArray(this, reader, unpacker, cmd);
        var nullableVerifier = NonNullableFieldVerifier4.MissingRequiredProperties_FuncFactory<T>();
        foreach (var t in readerToArray) {
            var verifier = nullableVerifier(t);
            if (verifier != null) {
                throw new(verifier.JoinStrings());
            }
        }
        return readerToArray;
    }
}

public readonly record struct EnumeratedObjectsSqlCommand<T>(ParameterizedSql Sql, CommandTimeout CommandTimeout, FieldMappingMode FieldMapping) : ITypedSqlCommand<IEnumerable<T>>, IWithTimeout<EnumeratedObjectsSqlCommand<T>>
    where T : IWrittenImplicitly
{
    public EnumeratedObjectsSqlCommand<T> WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout };

    [UsefulToKeep("lib method")]
    public EnumeratedObjectsSqlCommand<T> WithFieldMappingMode(FieldMappingMode fieldMapping)
        => this with { FieldMapping = fieldMapping };

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
            yield return nextRow; //cannot yield in try-catch block
        }
    }
}

public readonly record struct TuplesSqlCommand<
    [MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    T
>(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<T[]>, IWithTimeout<TuplesSqlCommand<T>>
    where T : struct, IStructuralEquatable, ITuple
{
    public EnumeratedTuplesSqlCommand<T> ToLazilyEnumeratedCommand()
        => new(Sql, CommandTimeout);

    public TuplesSqlCommand<T> WithTimeout(CommandTimeout commandTimeout)
        => this with { CommandTimeout = commandTimeout };

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

public readonly record struct EnumeratedTuplesSqlCommand<T>(ParameterizedSql Sql, CommandTimeout CommandTimeout) : ITypedSqlCommand<IEnumerable<T>>, IWithTimeout<EnumeratedTuplesSqlCommand<T>>
    where T : struct, IStructuralEquatable, ITuple
{
    public EnumeratedTuplesSqlCommand<T> WithTimeout(CommandTimeout timeout)
        => this with { CommandTimeout = timeout };

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
