namespace ProgressOnderwijsUtils;

public interface ISqlErrorParseResult { }

public struct KeyConstraintViolation : ISqlErrorParseResult
{
    public string ConstraintType { get; init; }
    public string ConstraintName { get; init; }
    public string ObjectName { get; init; }
    public string[] DuplicateKeyValue { get; init; }
}

public struct DuplicateKeyUniqueIndex : ISqlErrorParseResult
{
    public string IndexName { get; init; }
    public string ObjectName { get; init; }
    public string[] DuplicateKeyValue { get; init; }
}

public struct CannotInsertNull : ISqlErrorParseResult
{
    public string TableName { get; init; }
    public string ColumnName { get; init; }
    public string StatementType { get; init; }
}

public struct GenericConstraintViolation : ISqlErrorParseResult
{
    public string StatementType { get; init; }
    public string ConstraintType { get; init; }
    public string ConstraintName { get; init; }
    public string? DatabaseName { get; init; }
    public string? TableName { get; init; }
    public string? ColumnName { get; init; }
}

public static class SqlErrorParser
{
    // Format strings of error messages are available in sys.messages.

    // message_id 2627
    static readonly Regex keyConstraintViolationRegex = new(
        @"Violation of (?<ConstraintType>.*) constraint '(?<ConstraintName>[^']*)'\. Cannot insert duplicate key in object '(?<ObjectName>[^']*)'\. The duplicate key value is \((?<DuplicateKeyValue>.*)\)\.",
        RegexOptions.Compiled
    );

    // message_id 2601
    static readonly Regex duplicateKeyUniqueIndexRegex = new(
        @"Cannot insert duplicate key row in object '(?<ObjectName>[^']*)' with unique index '(?<IndexName>[^']*)'\. The duplicate key value is \((?<DuplicateKeyValue>.*)\)\.",
        RegexOptions.Compiled
    );

    // message_id 515
    static readonly Regex cannotInsertNullRegex = new(
        @"Cannot insert the value NULL into column '(?<ColumnName>[^']*)', table '(?<TableName>[^']*)'; column does not allow nulls\. (?<StatementType>.*) fails\.",
        RegexOptions.Compiled
    );

    // message_id 547
    static readonly Regex genericConstraintViolationRegex = new(
        @"The (?<StatementType>.*) statement conflicted with the (?<ConstraintType>.*) constraint ""(?<ConstraintName>[^""]*)""\.( The conflict occurred in database ""(?<DatabaseName>[^""]+)"", table ""(?<TableName>[^""]+)"", column '(?<ColumnName>[^']+)'\.)?",
        RegexOptions.Compiled
    );

    static ISqlErrorParseResult? TryParseKeyConstraintViolation(string errorMessage)
    {
        var match = keyConstraintViolationRegex.Match(errorMessage);
        if (match.Success) {
            return new KeyConstraintViolation {
                ConstraintType = match.Groups["ConstraintType"].Value,
                ConstraintName = match.Groups["ConstraintName"].Value,
                ObjectName = match.Groups["ObjectName"].Value,
                DuplicateKeyValue = match.Groups["DuplicateKeyValue"].Value.Split(new[] { ", ", }, StringSplitOptions.None),
            };
        }
        return null;
    }

    static ISqlErrorParseResult? TryParseDuplicateKeyUniqueIndex(string errorMessage)
    {
        var match = duplicateKeyUniqueIndexRegex.Match(errorMessage);
        if (match.Success) {
            return new DuplicateKeyUniqueIndex {
                IndexName = match.Groups["IndexName"].Value,
                ObjectName = match.Groups["ObjectName"].Value,
                DuplicateKeyValue = match.Groups["DuplicateKeyValue"].Value.Split(new[] { ", ", }, StringSplitOptions.None),
            };
        }
        return null;
    }

    static ISqlErrorParseResult? TryParseCannotInsertNull(string errorMessage)
    {
        var match = cannotInsertNullRegex.Match(errorMessage);
        if (match.Success) {
            return new CannotInsertNull {
                TableName = match.Groups["TableName"].Value,
                ColumnName = match.Groups["ColumnName"].Value,
                StatementType = match.Groups["StatementType"].Value,
            };
        }
        return null;
    }

    static ISqlErrorParseResult? TryParseGenericConstraintViolation(string errorMessage)
    {
        var match = genericConstraintViolationRegex.Match(errorMessage);
        if (match.Success) {
            return new GenericConstraintViolation {
                StatementType = match.Groups["StatementType"].Value,
                ConstraintType = match.Groups["ConstraintType"].Value,
                ConstraintName = match.Groups["ConstraintName"].Value,
                DatabaseName = match.Groups["DatabaseName"].Value.NullIfWhiteSpace(),
                TableName = match.Groups["TableName"].Value.NullIfWhiteSpace(),
                ColumnName = match.Groups["ColumnName"].Value.NullIfWhiteSpace(),
            };
        }
        return null;
    }

    public static ISqlErrorParseResult? Parse(string errorMessage)
        => TryParseKeyConstraintViolation(errorMessage)
            ?? TryParseDuplicateKeyUniqueIndex(errorMessage)
            ?? TryParseCannotInsertNull(errorMessage)
            ?? TryParseGenericConstraintViolation(errorMessage);

    public static ISqlErrorParseResult? Parse(this SqlError error)
        => Parse(error.Message);

    public static SqlError? FirstContainedSqlErrorOrNull(this Exception? e)
    {
        if (e is SqlException sqlException) {
            return sqlException.Errors[0];
        } else if (e?.InnerException != null) {
            return FirstContainedSqlErrorOrNull(e.InnerException);
        } else {
            return null;
        }
    }
}
