using System;
using System.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class ParameterizedSqlTest
    {
        [Fact]
        public void IdenticalInterpolatedSqlWithoutParamsAreEquals()
        {
            ParameterizedSql
                a1 = SQL($"a"),
                a2 = SQL($"a");

            PAssert.That(() => a1 == a2);
            PAssert.That(() => !(a1 != a2));
            PAssert.That(() => a1.Equals(a2));
            PAssert.That(() => Equals(a1, a2));
        }

        [Fact]
        public void NonIdenticalInterpolatedSqlWithoutParamsAreNotEquals()
        {
            ParameterizedSql
                a = SQL($"a"),
                b = SQL($"b");

            PAssert.That(() => !(a == b));
            PAssert.That(() => a != b);
            PAssert.That(() => !a.Equals(b));
            PAssert.That(() => !Equals(a, b));
        }

        [Fact]
        public void IdenticalDynamicSqlAreEquals()
        {
            ParameterizedSql
                a1 = ParameterizedSql.CreateDynamic("a"),
                a2 = ParameterizedSql.CreateDynamic("aa".Substring(1)); //substring to avoid reference equality

            PAssert.That(() => a1 == a2);
            PAssert.That(() => !(a1 != a2));
            PAssert.That(() => a1.Equals(a2));
            PAssert.That(() => Equals(a1, a2));
        }

        [Fact]
        public void NonIdenticalDynamicSqlAreNotEquals()
        {
            ParameterizedSql
                a = ParameterizedSql.CreateDynamic("a"),
                b = ParameterizedSql.CreateDynamic("b");

            PAssert.That(() => !(a == b));
            PAssert.That(() => a != b);
            PAssert.That(() => !a.Equals(b));
            PAssert.That(() => !Equals(a, b));
        }

        [Fact]
        public void EqualsIgnoresComponentBoundaries()
        {
            ParameterizedSql
                a = SQL($"a"),
                b = SQL($"b"),
                c = SQL($"c"),
                ab = SQL($"a b"),
                bc = SQL($"b c"),
                abc = SQL($"a b c");

            PAssert.That(() => a + b == ab);
            PAssert.That(() => a + bc == ab + c);
            PAssert.That(() => a + (b + c) == a + b + c);
            PAssert.That(() => a + b + c == abc);
        }

        [Fact]
        public void EqualsChecksSimpleParameterValues()
        {
            ParameterizedSql
                intPar = SQL($"a param: {1}"),
                intPar2 = SQL($"a param: {1}"),
                enumIntPar = SQL($"a param: {(DayOfWeek)1}"),
                enumIntPar2 = SQL($"a param: {(DayOfWeek)1}"),
                longPar = SQL($"a param: {1L}"),
                stringPar = SQL($"a param: {"1"}"),
                stringPar2 = SQL($"a param: {"1"}"),
                noPar = SQL($"a param: 1")
                ;

            PAssert.That(() => intPar == intPar2);
            PAssert.That(() => enumIntPar == enumIntPar2);
            PAssert.That(() => stringPar == stringPar2);

            PAssert.That(() => intPar != enumIntPar);
            PAssert.That(() => intPar != longPar);
            PAssert.That(() => intPar != stringPar);
            PAssert.That(() => enumIntPar != longPar);
            PAssert.That(() => enumIntPar != stringPar);
            PAssert.That(() => longPar != stringPar);

            PAssert.That(() => intPar.GetHashCode() == intPar2.GetHashCode());
            PAssert.That(() => enumIntPar.GetHashCode() == enumIntPar2.GetHashCode());
            PAssert.That(() => stringPar.GetHashCode() == stringPar2.GetHashCode());
            PAssert.That(() => intPar.GetHashCode() != stringPar.GetHashCode());

            PAssert.That(() => intPar != noPar);
            PAssert.That(() => enumIntPar != noPar);
            PAssert.That(() => longPar != noPar);
            PAssert.That(() => stringPar != noPar);

            PAssert.That(() => new[] { intPar, intPar2, enumIntPar, enumIntPar2, longPar, stringPar, stringPar2, noPar }.Distinct().Count() == 5);
        }

        [Fact]
        public void EqualsChecksTableValuedParametersInDepth()
        {
            ParameterizedSql
                withPar_1_2 = SQL($"a param: {new[] { 1, 2 }}"),
                withPar_1_2b = SQL($"a param: {new[] { 1, 2 }}"),
                withPar_2_1 = SQL($"a param: {new[] { 2, 1 }}"),
                withParEnum_1_2 = SQL($"a param: {new[] { (DayOfWeek)1, (DayOfWeek)2 }}"),
                withParString_1_2 = SQL($"a param: {new[] { "1", "2" }}"),
                withParString_1_2b = SQL($"a param: {new[] { "1", "2" }}")
                ;

            PAssert.That(() => withPar_1_2 == withPar_1_2b);
            PAssert.That(() => withParString_1_2 == withParString_1_2b);

            PAssert.That(() => withPar_1_2 != withPar_2_1);
            PAssert.That(() => withPar_2_1 != withParString_1_2);
            PAssert.That(() => withPar_1_2 != withParEnum_1_2);
        }

        [Fact]
        public void EqualsRecurses()
        {
            ParameterizedSql
                a = SQL($"a"),
                b = SQL($"b"),
                c = SQL($"c");

            // ReSharper disable ArrangeRedundantParentheses
            PAssert.That(() => (a + (a + c)) + b == (a + a) + (c + b));
            PAssert.That(() => (a + ((a + a) + c)) + b != a + a + c + b);
            // ReSharper restore ArrangeRedundantParentheses
        }

        [Fact]
        public void ParameterizedSqlValidation()
        {
            // ReSharper disable once NotAccessedVariable
            ParameterizedSql ignore;
            Assert.Throws<ArgumentNullException>(() => ignore = ParameterizedSql.CreateDynamic(null));
        }

        [Fact]
        public void OperatorAndReturnsSqlWhenTrue()
        {
            var trueCondition = true;
            var falseCondition = false;

            PAssert.That(() => (trueCondition && SQL($"test")) == SQL($"test"));
            PAssert.That(() => ParameterizedSql.TruthyEmpty != ParameterizedSql.Empty);
            PAssert.That(() => falseCondition == ParameterizedSql.Empty);
            PAssert.That(() => trueCondition == ParameterizedSql.TruthyEmpty);
            PAssert.That(() => (falseCondition && SQL($"test") || SQL($"test2")) == SQL($"test2"));
            PAssert.That(() => SQL($"maybe-{falseCondition && SQL($"test")}") == SQL($"maybe-"));
            PAssert.That(() => (trueCondition || SQL($"test")) == ParameterizedSql.TruthyEmpty);
            PAssert.That(() => (trueCondition || SQL($"test")) != ParameterizedSql.Empty);
            PAssert.That(() => (falseCondition || SQL($"test")) == SQL($"test"));
            PAssert.That(() => (SQL($"test") + falseCondition || SQL($"WhenFalse")) == SQL($"test"));
            PAssert.That(() => SQL($"{ParameterizedSql.TruthyEmpty}") == ParameterizedSql.Empty);

            PAssert.That(() => SQL($"{trueCondition && SQL($"test")}") == SQL($"test"));
            PAssert.That(() => SQL($"{trueCondition}") == ParameterizedSql.Param(true));
        }

        [Fact]
        public void PrependingEmptyHasNoEffect()
        {
            PAssert.That(() => ParameterizedSql.Empty + SQL($"abc") == SQL($"abc"));
        }

        [Fact]
        public void EmptyParameterizedSql()
        {
            //we want to check that various subtly different ways of making empty ParameterizedSqls
            //all behave as expected.
            var qEmpty0 = default(ParameterizedSql);
            var qEmpty1 = ParameterizedSql.Empty;
            var qEmpty2 = SQL($"");
            var qEmpty3 = SQL($"");

            //we don't want to depend on string reference equality, but as it turns out all empty strings are always reference equals:
            PAssert.That(() => ReferenceEquals(42.ToStringInvariant().Substring(42.ToStringInvariant().Length), ""));

            PAssert.That(() => qEmpty0 == qEmpty1);
            PAssert.That(() => !(qEmpty0 != qEmpty2));
            PAssert.That(() => qEmpty1 == qEmpty0);
            PAssert.That(() => qEmpty3 == qEmpty2);
            PAssert.That(() => qEmpty1 == qEmpty2);
            PAssert.That(() => qEmpty1.GetHashCode() == qEmpty2.GetHashCode());
            PAssert.That(() => qEmpty3.GetHashCode() == qEmpty2.GetHashCode());

            PAssert.That(() => SQL($"abc") + qEmpty2 == SQL($"abc"));
            PAssert.That(() => SQL($"abc") + qEmpty3 == SQL($"abc"));
            PAssert.That(() => (SQL($"abc") + qEmpty2).GetHashCode() == SQL($"abc").GetHashCode());
            PAssert.That(() => (SQL($"abc") + qEmpty3).GetHashCode() == SQL($"abc").GetHashCode());
        }

        [Fact]
        public void DealsWithApparentlyNestedParameterPlaceholders()
        {
            var badQuery = SQL($@"A{{x{1}}}Z");
            Assert.ThrowsAny<Exception>(() => badQuery.DebugText());
            using (var conn = new SqlConnection())
                Assert.ThrowsAny<Exception>(() => badQuery.CreateSqlCommand(conn));
        }

        [Fact]
        public void SupportsNestedParameterizedSql()
        {
            var result = SQL($@"A{0}{SQL($@"[{1}{0}]")}Z");

            var cmd = result.CreateSqlCommand(new SqlCommandCreationContext(null, 0, null));

            var commandText = @"A@par0[@par1@par0]Z";
            PAssert.That(() => cmd.Command.CommandText == commandText);
            PAssert.That(() => cmd.Command.Parameters.Cast<SqlParameter>().Select(p => p.Value).SequenceEqual(new object[] { 0, 1 }));
        }

        [Fact]
        public void ParameterizedSqlToStringIsClearForEnumParams()
        {
            PAssert.That(() => SQL($"select {42}, {DayOfWeek.Tuesday}").ToString() == "*/Pseudo-sql (with parameter values inlined!):/*\r\nselect 42, 2/*DayOfWeek.Tuesday*/");
        }

        [Fact]
        public void ParameterizedSqlUsesLiteralsForValidEnumConstants()
        {
            PAssert.That(() => SQL($"select {(DayOfWeek)42}, {DayOfWeek.Tuesday}").CommandText() == "select @par0, 2/*DayOfWeek.Tuesday*/");
        }

        [Fact]
        public void ParameterizedSqlUsesLiteralsForBooleanConstants()
        {
            PAssert.That(() => SQL($"select {true}, {false}").CommandText() == "select cast(1 as bit), cast(0 as bit)");
        }

        [Fact]
        public void ParameterizedSqlSupportsNullParameters()
        {
            //TODO: do we want to make these literal?
            PAssert.That(() => SQL($"select {null}").CommandText() == "select @par0");
        }

        [Fact]
        public void ParameterizedSqlDoesNotUseLiteralsEnumsMarked_IEnumShouldBeParameterizedInSqlAttribute()
        {
            PAssert.That(() => SQL($"select {ExampleNonLiteralEnum.SomeValue}").CommandText() == "select @par0");
        }

        [TestNotLiteral]
        enum ExampleNonLiteralEnum
        {
            SomeValue = 1,
        }
    }

    class TestNotLiteralAttribute : Attribute, IEnumShouldBeParameterizedInSqlAttribute { }
}
