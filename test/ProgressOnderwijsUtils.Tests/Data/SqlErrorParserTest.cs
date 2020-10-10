using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class SqlErrorParserTest : TransactedLocalConnection
    {
        [Fact]
        public void Parse_returns_KeyConstraintViolation_when_single_column_unique_key_constraint_is_violated_with_value_containing_parentheses()
        {
            SQL($@"
                    create table #T (
                        Id int identity primary key,
                        C nchar(4) not null constraint uc_T_C unique
                    )
                ").ExecuteNonQuery(Connection);
            SQL($"insert #T (C) values ('A(1)')").ExecuteNonQuery(Connection);

            var exception = Assert.Throws<ParameterizedSqlExecutionException>(() => SQL($"insert #T (C) values ('A(1)')").ExecuteNonQuery(Connection));

            var exceptionError = exception.FirstContainedSqlErrorOrNull() ?? throw new Exception("Expected an inner SqlException with error");
            var violation = exceptionError.Parse() as KeyConstraintViolation? ?? throw new Exception("expected KeyConstraintViolation");

            PAssert.That(() => violation.ConstraintType == "UNIQUE KEY");
            PAssert.That(() => violation.ConstraintName == "uc_T_C");
            PAssert.That(() => violation.ObjectName == "dbo.#T");
            PAssert.That(() => violation.DuplicateKeyValue.SequenceEqual(new[] { "A(1)" }));
        }

        [Fact]
        public void Parse_returns_DuplicateKeyUniqueIndex_when_duplicate_value_is_inserted_in_unique_index()
        {
            SQL($@"
                    create table #T (
                        Id int identity primary key,
                        C nchar(1) not null
                    )
                    create unique index ix_T on #T (C)
                ").ExecuteNonQuery(Connection);
            SQL($"insert #T (C) values ('A')").ExecuteNonQuery(Connection);

            var exception = Assert.Throws<ParameterizedSqlExecutionException>(() => SQL($"insert #T (C) values ('A')").ExecuteNonQuery(Connection));

            var exceptionError = exception.FirstContainedSqlErrorOrNull() ?? throw new Exception("Expected an inner SqlException with error");
            var violation = exceptionError.Parse() as DuplicateKeyUniqueIndex? ?? throw new Exception("expected DuplicateKeyUniqueIndex");

            PAssert.That(() => violation.IndexName == "ix_T");
            PAssert.That(() => violation.ObjectName == "dbo.#T");
            PAssert.That(() => violation.DuplicateKeyValue.SequenceEqual(new[] { "A" }));
        }

        [Fact]
        public void Parse_returns_KeyConstraintViolation_when_single_column_primary_key_constraint_is_violated()
        {
            SQL($@"
                    create table #T (
                        Id int constraint pk_T primary key
                    )
                ").ExecuteNonQuery(Connection);
            SQL($"insert #T (Id) values (1)").ExecuteNonQuery(Connection);
            var exception = Assert.Throws<ParameterizedSqlExecutionException>(() => SQL($"insert #T (Id) values (1)").ExecuteNonQuery(Connection));

            var exceptionError = exception.FirstContainedSqlErrorOrNull() ?? throw new Exception("Expected an inner SqlException with error");
            var violation = exceptionError.Parse() as KeyConstraintViolation? ?? throw new Exception("expected KeyConstraintViolation");

            PAssert.That(() => violation.ConstraintType == "PRIMARY KEY");
            PAssert.That(() => violation.ConstraintName == "pk_T");
            PAssert.That(() => violation.ObjectName == "dbo.#T");
            PAssert.That(() => violation.DuplicateKeyValue.SequenceEqual(new[] { "1" }));
        }

        [Fact]
        public void Parse_returns_KeyConstraintViolation_when_multi_column_unique_key_constraint_is_violated()
        {
            SQL($@"
                    create table #T (
                        Id int identity primary key,
                        C1 nchar(1) not null,
                        C2 nchar(1) not null,
                        constraint uc_T_C1_C2 unique (C1, C2)
                    )
                ").ExecuteNonQuery(Connection);
            SQL($"insert #T (C1, C2) values ('A', 'B')").ExecuteNonQuery(Connection);
            var exception = Assert.Throws<ParameterizedSqlExecutionException>(() => SQL($"insert #T (C1, C2) values ('A', 'B')").ExecuteNonQuery(Connection));

            var exceptionError = exception.FirstContainedSqlErrorOrNull() ?? throw new Exception("Expected an inner SqlException with error");
            var violation = exceptionError.Parse() as KeyConstraintViolation? ?? throw new Exception("expected KeyConstraintViolation");

            PAssert.That(() => violation.ConstraintType == "UNIQUE KEY");
            PAssert.That(() => violation.ConstraintName == "uc_T_C1_C2");
            PAssert.That(() => violation.ObjectName == "dbo.#T");
            PAssert.That(() => violation.DuplicateKeyValue.SequenceEqual(new[] { "A", "B" }));
        }

        [Fact]
        public void Parse_returns_GenericConstraintViolation_when_check_constraint_is_violated_using_insert()
        {
            SQL($@"
                    create table T1 (
                        Id int identity primary key,
                        C int not null constraint ck_T_C check (C <> 1)
                    )
                ").ExecuteNonQuery(Connection);
            var exception = Assert.Throws<ParameterizedSqlExecutionException>(() => SQL($"insert T1 (C) values (1)").ExecuteNonQuery(Connection));

            var exceptionError = exception.FirstContainedSqlErrorOrNull() ?? throw new Exception("Expected an inner SqlException with error");
            var violation = exceptionError.Parse() as GenericConstraintViolation? ?? throw new Exception("expected GenericConstraintViolation");

            PAssert.That(() => violation.StatementType == "INSERT");
            PAssert.That(() => violation.ConstraintType == "CHECK");
            PAssert.That(() => violation.ConstraintName == "ck_T_C");
            PAssert.That(() => violation.ColumnName == "C");
            PAssert.That(() => violation.TableName == "dbo.T1");
            PAssert.That(() => violation.DatabaseName == "master");
        }

        [Fact]
        public void Parse_returns_GenericConstraintViolation_when_check_constraint_is_violated_using_update()
        {
            SQL($@"
                    create table #T (
                        Id int identity primary key,
                        C int not null constraint ck_T_C check (C <> 1)
                    )
                ").ExecuteNonQuery(Connection);
            SQL($"insert #T (C) values (2)").ExecuteNonQuery(Connection);
            var exception = Assert.Throws<ParameterizedSqlExecutionException>(() => SQL($"update #T set C = 1").ExecuteNonQuery(Connection));

            var exceptionError = exception.FirstContainedSqlErrorOrNull() ?? throw new Exception("Expected an inner SqlException with error");
            var violation = exceptionError.Parse() as GenericConstraintViolation? ?? throw new Exception("expected GenericConstraintViolation");

            PAssert.That(() => violation.StatementType == "UPDATE");
            PAssert.That(() => violation.ConstraintType == "CHECK");
            PAssert.That(() => violation.ConstraintName == "ck_T_C");
            PAssert.That(() => violation.ColumnName == "C");
            //tablename: nothing useful!
            PAssert.That(() => violation.DatabaseName == "tempdb");
        }

        [Fact]
        public void Parse_returns_GenericConstraintViolation_when_reference_constraint_is_violated()
        {
            // Reference constraints on temporary tables aren't enforced or something.
            SQL($@"
                    create table T1 (
                        Id int identity primary key
                    )
                    create table T2 (
                        Id int identity primary key,
                        C int not null constraint fk_T2_T1 references T1
                    )
                ").ExecuteNonQuery(Connection);
            var exception = Assert.Throws<ParameterizedSqlExecutionException>(() => SQL($"insert T2 (C) values (1)").ExecuteNonQuery(Connection));

            var exceptionError = exception.FirstContainedSqlErrorOrNull() ?? throw new Exception("Expected an inner SqlException with error");
            var violation = exceptionError.Parse() as GenericConstraintViolation? ?? throw new Exception("expected GenericConstraintViolation");

            PAssert.That(() => violation.StatementType == "INSERT");
            PAssert.That(() => violation.ConstraintType == "FOREIGN KEY");
            PAssert.That(() => violation.ConstraintName == "fk_T2_T1");
            PAssert.That(() => violation.ColumnName == "Id");
            PAssert.That(() => violation.TableName == "dbo.T1");
            PAssert.That(() => violation.DatabaseName == "master");
        }

        [Fact]
        public void Parse_returns_CannotInsertNull_when_attempting_to_insert_null_value_in_not_null_column()
        {
            SQL($@"
                    create table #T (
                        Id int identity primary key,
                        C int not null
                    )
                ").ExecuteNonQuery(Connection);
            var exception = Assert.Throws<ParameterizedSqlExecutionException>(() => SQL($"insert #T (C) values (null)").ExecuteNonQuery(Connection));

            var exceptionError = exception.FirstContainedSqlErrorOrNull() ?? throw new Exception("Expected an inner SqlException with error");
            var violation = exceptionError.Parse() as CannotInsertNull? ?? throw new Exception("expected CannotInsertNull");

            PAssert.That(() => violation.TableName.StartsWith("tempdb.dbo.#T", StringComparison.Ordinal));
            PAssert.That(() => violation.ColumnName == "C");
            PAssert.That(() => violation.StatementType == "INSERT");
        }
    }
}
