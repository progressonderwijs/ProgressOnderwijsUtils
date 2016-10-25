using System;
using System.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.DomainUnits;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class ParameterizedSqlTest
    {
        [Test]
        public void IdenticalInterpolatedSqlWithoutParamsAreEquals()
        {
            ParameterizedSql
                a1 = SafeSql.SQL($"a"),
                a2 = SafeSql.SQL($"a");

            PAssert.That(() => a1 == a2);
            PAssert.That(() => !(a1 != a2));
            PAssert.That(() => a1.Equals(a2));
            PAssert.That(() => Equals(a1, a2));
        }

        [Test]
        public void NonIdenticalInterpolatedSqlWithoutParamsAreNotEquals()
        {
            ParameterizedSql
                a = SafeSql.SQL($"a"),
                b = SafeSql.SQL($"b");

            PAssert.That(() => !(a == b));
            PAssert.That(() => a != b);
            PAssert.That(() => !a.Equals(b));
            PAssert.That(() => !Equals(a, b));
        }

        [Test]
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

        [Test]
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

        [Test]
        public void EqualsIgnoresComponentBoundaries()
        {
            ParameterizedSql
                a = SafeSql.SQL($"a"),
                b = SafeSql.SQL($"b"),
                c = SafeSql.SQL($"c"),
                ab = SafeSql.SQL($"a b"),
                bc = SafeSql.SQL($"b c"),
                abc = SafeSql.SQL($"a b c");

            PAssert.That(() => a + b == ab);
            PAssert.That(() => a + bc == ab + c);
            PAssert.That(() => a + (b + c) == a + b + c);
            PAssert.That(() => a + b + c == abc);
        }

        [Test]
        public void EqualsRecurses()
        {
            ParameterizedSql
                a = SafeSql.SQL($"a"),
                b = SafeSql.SQL($"b"),
                c = SafeSql.SQL($"c");

            // ReSharper disable ArrangeRedundantParentheses
            PAssert.That(() => (a + (a + c)) + b == (a + a) + (c + b));
            PAssert.That(() => (a + ((a + a) + c)) + b != a + a + c + b);
            // ReSharper restore ArrangeRedundantParentheses
        }

        [Test]
        public void ParameterizedSqlValidation()
        {
            // ReSharper disable once NotAccessedVariable
            ParameterizedSql ignore;
            Assert.Throws<ArgumentNullException>(() => ignore = ParameterizedSql.CreateDynamic(null));
        }

        [Test]
        public void PrependingEmptyHasNoEffect()
        {
            PAssert.That(() => ParameterizedSql.Empty + SafeSql.SQL($"abc") == SafeSql.SQL($"abc"));
        }

        [Test]
        public void EmptyParameterizedSql()
        {
            //we want to check that various subtly different ways of making empty ParameterizedSqls
            //all behave as expected.
            var qEmpty0 = default(ParameterizedSql);
            var qEmpty1 = ParameterizedSql.Empty;
            var qEmpty2 = SafeSql.SQL($"");
            var qEmpty3 = SafeSql.SQL($"");

            //we don't want to depend on string reference equality, but as it turns out all empty strings are always reference equals:
            PAssert.That(() => ReferenceEquals(42.ToStringInvariant().Substring(42.ToStringInvariant().Length), ""));

            PAssert.That(() => qEmpty0 == qEmpty1);
            PAssert.That(() => !(qEmpty0 != qEmpty2));
            PAssert.That(() => qEmpty1 == qEmpty0);
            PAssert.That(() => qEmpty3 == qEmpty2);
            PAssert.That(() => qEmpty1 == qEmpty2);
            PAssert.That(() => qEmpty1.GetHashCode() == qEmpty2.GetHashCode());
            PAssert.That(() => qEmpty3.GetHashCode() == qEmpty2.GetHashCode());

            PAssert.That(() => SafeSql.SQL($"abc") + qEmpty2 == SafeSql.SQL($"abc"));
            PAssert.That(() => SafeSql.SQL($"abc") + qEmpty3 == SafeSql.SQL($"abc"));
            PAssert.That(() => (SafeSql.SQL($"abc") + qEmpty2).GetHashCode() == SafeSql.SQL($"abc").GetHashCode());
            PAssert.That(() => (SafeSql.SQL($"abc") + qEmpty3).GetHashCode() == SafeSql.SQL($"abc").GetHashCode());
        }

        [Test]
        public void DealsWithApparentlyNestedParameterPlaceholders()
        {
            var badQuery = SafeSql.SQL($@"A{{x{1}}}Z");
            Assert.Catch(() => badQuery.DebugText());
            using (var conn = new SqlConnection())
                Assert.Catch(() => badQuery.CreateSqlCommand(conn));
        }

        [Test]
        public void SupportsNestedParameterizedSql()
        {
            var result = SafeSql.SQL($@"A{0}{SafeSql.SQL($@"[{1}{0}]")}Z");

            var cmd = result.CreateSqlCommand(new SqlCommandCreationContext(null, 0, null));

            var commandText = @"A@par0[@par1@par0]Z";
            Assert.That(cmd.Command.CommandText, Is.EqualTo(commandText));
            PAssert.That(() => cmd.Command.Parameters.Cast<SqlParameter>().Select(p => p.Value).SequenceEqual(new object[] { 0, 1 }));
        }

        [Test]
        public void ParameterizedSqlToStringIsClearForEnumParams()
        {
            PAssert.That(() => SafeSql.SQL($"select {42}, {DayOfWeek.Tuesday}").ToString() == "*/Pseudo-sql (with parameter values inlined!):/*\r\nselect 42, 2/*DayOfWeek.Tuesday*/");
        }

        [Test]
        public void ParameterizedSqlUsesLiteralsForValidEnumConstants()
        {
            PAssert.That(() => SafeSql.SQL($"select {(DayOfWeek)42}, {DayOfWeek.Tuesday}").CommandText() == "select @par0, 2/*DayOfWeek.Tuesday*/");
        }

        [Test]
        public void ParameterizedSqlDoesNotUseLiteralsEnumsMarked_IEnumShouldBeParameterizedInSqlAttribute()
        {
            PAssert.That(() => SafeSql.SQL($"select {ExampleNonLiteralEnum.SomeValue}").CommandText() == "select @par0");
            PAssert.That(() => SafeSql.SQL($"select {Id.Organisatie.Dummy_ICT_en_Media}").CommandText() == "select @par0");
        }

        [TestNotLiteral]
        enum ExampleNonLiteralEnum
        {
            SomeValue = 1,
        }
    }

    class TestNotLiteralAttribute : Attribute, IEnumShouldBeParameterizedInSqlAttribute { }
}
