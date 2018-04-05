using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public interface ISqlErrorParseResult { }

    public struct KeyConstraintViolation : ISqlErrorParseResult
    {
        public string ConstraintType { get; set; }
        public string ConstraintName { get; set; }
        public string ObjectName { get; set; }
        public string[] DuplicateKeyValue { get; set; }
    }

    public struct DuplicateKeyUniqueIndex : ISqlErrorParseResult
    {
        public string IndexName { get; set; }
        public string ObjectName { get; set; }
        public string[] DuplicateKeyValue { get; set; }
    }

    public struct CannotInsertNull : ISqlErrorParseResult
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string StatementType { get; set; }
    }

    public struct GenericConstraintViolation : ISqlErrorParseResult
    {
        public string StatementType { get; set; }
        public string ConstraintType { get; set; }
        public string ConstraintName { get; set; }
    }

    public static class SqlErrorParser
    {
        // Format strings of error messages are available in sys.messages.

        // message_id 2627
        static readonly Regex keyConstraintViolationRegex = new Regex(
            "Violation of (?<ConstraintType>.*) constraint '(?<ConstraintName>[^']*)'\\. Cannot insert duplicate key in object '(?<ObjectName>[^']*)'\\. The duplicate key value is \\((?<DuplicateKeyValue>.*)\\)\\.",
            RegexOptions.Compiled
            );

        // message_id 2601
        static readonly Regex duplicateKeyUniqueIndexRegex = new Regex(
            "Cannot insert duplicate key row in object '(?<ObjectName>[^']*)' with unique index '(?<IndexName>[^']*)'\\. The duplicate key value is \\((?<DuplicateKeyValue>.*)\\)\\.",
            RegexOptions.Compiled
            );

        // message_id 515
        static readonly Regex cannotInsertNullRegex = new Regex(
            "Cannot insert the value NULL into column '(?<ColumnName>[^']*)', table '(?<TableName>[^']*)'; column does not allow nulls\\. (?<StatementType>.*) fails\\.",
            RegexOptions.Compiled
            );

        // message_id 547
        static readonly Regex genericConstraintViolationRegex = new Regex(
            "The (?<StatementType>.*) statement conflicted with the (?<ConstraintType>.*) constraint \"(?<ConstraintName>[^\"]*)\"\\.",
            RegexOptions.Compiled
            );

        [CanBeNull]
        static ISqlErrorParseResult TryParseKeyConstraintViolation([NotNull] SqlError error)
        {
            var match = keyConstraintViolationRegex.Match(error.Message);
            if (match.Success) {
                return new KeyConstraintViolation {
                    ConstraintType = match.Groups["ConstraintType"].Value,
                    ConstraintName = match.Groups["ConstraintName"].Value,
                    ObjectName = match.Groups["ObjectName"].Value,
                    DuplicateKeyValue = match.Groups["DuplicateKeyValue"].Value.Split(new[] { ", " }, StringSplitOptions.None),
                };
            }
            return null;
        }

        [CanBeNull]
        static ISqlErrorParseResult TryParseDuplicateKeyUniqueIndex([NotNull] SqlError error)
        {
            var match = duplicateKeyUniqueIndexRegex.Match(error.Message);
            if (match.Success) {
                return new DuplicateKeyUniqueIndex {
                    IndexName = match.Groups["IndexName"].Value,
                    ObjectName = match.Groups["ObjectName"].Value,
                    DuplicateKeyValue = match.Groups["DuplicateKeyValue"].Value.Split(new[] { ", " }, StringSplitOptions.None),
                };
            }
            return null;
        }

        [CanBeNull]
        static ISqlErrorParseResult TryParseCannotInsertNull([NotNull] SqlError error)
        {
            var match = cannotInsertNullRegex.Match(error.Message);
            if (match.Success) {
                return new CannotInsertNull {
                    TableName = match.Groups["TableName"].Value,
                    ColumnName = match.Groups["ColumnName"].Value,
                    StatementType = match.Groups["StatementType"].Value,
                };
            }
            return null;
        }

        [CanBeNull]
        static ISqlErrorParseResult TryParseGenericConstraintViolation([NotNull] SqlError error)
        {
            var match = genericConstraintViolationRegex.Match(error.Message);
            if (match.Success) {
                return new GenericConstraintViolation {
                    StatementType = match.Groups["StatementType"].Value,
                    ConstraintType = match.Groups["ConstraintType"].Value,
                    ConstraintName = match.Groups["ConstraintName"].Value,
                };
            }
            return null;
        }

        [CanBeNull]
        public static ISqlErrorParseResult Parse([NotNull] SqlError error)
            => TryParseKeyConstraintViolation(error)
                ?? TryParseDuplicateKeyUniqueIndex(error)
                    ?? TryParseCannotInsertNull(error)
                        ?? TryParseGenericConstraintViolation(error);
    }
}
