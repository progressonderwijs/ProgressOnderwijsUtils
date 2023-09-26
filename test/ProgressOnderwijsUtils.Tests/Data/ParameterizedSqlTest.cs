namespace ProgressOnderwijsUtils.Tests.Data;

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
            a2 = ParameterizedSql.CreateDynamic("aa"[1..]); //substring to avoid reference equality

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

        PAssert.That(() => new[] { intPar, intPar2, enumIntPar, enumIntPar2, longPar, stringPar, stringPar2, noPar, }.Distinct().Count() == 5);
    }

    [Fact]
    public void EqualsChecksTableValuedParametersInDepth()
    {
        ParameterizedSql
            withPar_1_2 = SQL($"a param: {new[] { 1, 2, }}"),
            withPar_1_2b = SQL($"a param: {new[] { 1, 2, }}"),
            withPar_2_1 = SQL($"a param: {new[] { 2, 1, }}"),
            withParEnum_1_2 = SQL($"a param: {new[] { (DayOfWeek)1, (DayOfWeek)2, }}"),
            withParString_1_2 = SQL($"a param: {new[] { "1", "2", }}"),
            withParString_1_2b = SQL($"a param: {new[] { "1", "2", }}")
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

        PAssert.That(() => a + (a + c) + b == a + a + (c + b));
        PAssert.That(() => a + (a + a + c) + b != a + a + c + b);
    }

    [Fact]
    public void ParameterizedSqlValidation()
    {
        // ReSharper disable once NotAccessedVariable
        ParameterizedSql ignore;
        // ReSharper disable once AssignNullToNotNullAttribute
        _ = Assert.Throws<ArgumentNullException>(() => ignore = ParameterizedSql.CreateDynamic(null!));
    }

    [Fact]
    public void OperatorAndReturnsSqlWhenTrue()
    {
        var trueCondition = true;
        var falseCondition = false;

        var testSql = SQL($"test");
        PAssert.That(() => (trueCondition && testSql) == testSql);
        PAssert.That(() => ParameterizedSql.TruthyEmpty != ParameterizedSql.Empty);
        PAssert.That(() => falseCondition == ParameterizedSql.Empty);
        PAssert.That(() => trueCondition == ParameterizedSql.TruthyEmpty);
        var test2Sql = SQL($"test2");
        PAssert.That(() => (falseCondition && testSql || test2Sql) == test2Sql);
        var maybeWithNested = SQL($"maybe-{falseCondition && testSql}");
        var maybeJustDash = SQL($"maybe-");
        PAssert.That(() => maybeWithNested == maybeJustDash);
        PAssert.That(() => (trueCondition || testSql) == ParameterizedSql.TruthyEmpty);
        PAssert.That(() => (trueCondition || testSql) != ParameterizedSql.Empty);
        PAssert.That(() => (falseCondition || testSql) == testSql);
        var whenFalseSql = SQL($"WhenFalse");
        PAssert.That(() => (testSql + falseCondition || whenFalseSql) == testSql);
        var nestedEmptySql = SQL($"{ParameterizedSql.TruthyEmpty}");
        PAssert.That(() => nestedEmptySql == ParameterizedSql.Empty);

        var nestedConditionalTest = SQL($"{trueCondition && testSql}");
        PAssert.That(() => nestedConditionalTest == testSql);
        var nestedParamTrue = SQL($"{trueCondition}");
        PAssert.That(() => nestedParamTrue == ParameterizedSql.Param(true));
    }

    [Fact]
    public void PrependingEmptyHasNoEffect()
    {
        var abcSql = SQL($"abc");
        PAssert.That(() => ParameterizedSql.Empty + abcSql == abcSql);
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

        var abcSql = SQL($"abc");
        PAssert.That(() => abcSql + qEmpty2 == abcSql);
        PAssert.That(() => abcSql + qEmpty3 == abcSql);
        PAssert.That(() => (abcSql + qEmpty2).GetHashCode() == abcSql.GetHashCode());
        PAssert.That(() => (abcSql + qEmpty3).GetHashCode() == abcSql.GetHashCode());
    }

    [Fact]
    public void DealsWithApparentlyNestedParameterPlaceholders()
    {
        var badQuery = SQL($@"A{{x{1}}}Z");
        _ = Assert.ThrowsAny<Exception>(() => badQuery.DebugText());
        using var conn = new SqlConnection();
        _ = Assert.ThrowsAny<Exception>(() => badQuery.CreateSqlCommand(conn, CommandTimeout.WithoutTimeout));
    }

    [Fact]
    public void SupportsNestedParameterizedSql()
    {
        var result = SQL($@"A{0}{SQL($@"[{1}{0}]")}Z");

        using var cmd = result.CreateSqlCommand(new(), CommandTimeout.WithoutTimeout);

        var commandText = @"A@par0[@par1@par0]Z";
        PAssert.That(() => cmd.Command.CommandText == commandText);
        PAssert.That(() => cmd.Command.Parameters.Cast<SqlParameter>().Select(p => p.Value).SequenceEqual(new object[] { 0, 1, }));
    }

    [Fact]
    public void SupportsRawLiteralInterpolationsWithCurlyBraces()
    {
        var result = SQL(
            $$"""
              select a={{42}}
                  , b = 'consider {}'
                  , c = {{
                      SQL($"[{'a'}{"$@"}]")
                  }}
              ;
              """
        );

        using var cmd = result.CreateSqlCommand(new(), CommandTimeout.WithoutTimeout);

        var expectedCommandText = @"select a=@par0
    , b = 'consider {}'
    , c = [@par1@par2]
;";
        PAssert.That(() => cmd.Command.CommandText == expectedCommandText);
        PAssert.That(() => cmd.Command.Parameters.Cast<SqlParameter>().Select(p => p.Value).SequenceEqual(new object[] { 42, 'a', "$@", }));
    }

    [Fact]
    public void ParameterizedSqlToStringIsClearForEnumParams()
    {
        var sql = SQL($"select {42}, {DayOfWeek.Tuesday}");
        PAssert.That(() => sql.ToString() == "*/Pseudo-sql (with parameter values inlined!):/*\nselect 42, 2/*DayOfWeek.Tuesday*/");
    }

    [Fact]
    public void ParameterizedSqlUsesLiteralsForValidEnumConstants()
    {
        var sql = SQL($"select {(DayOfWeek)42}, {DayOfWeek.Tuesday}");
        PAssert.That(() => sql.CommandText() == "select @par0, 2/*DayOfWeek.Tuesday*/");
    }

    [Fact]
    public void ParameterizedSqlUsesLiteralsForBooleanConstants()
    {
        var sql = SQL($"select {true}, {false}");
        PAssert.That(() => sql.CommandText() == "select cast(1 as bit), cast(0 as bit)");
    }

    [Fact]
    public void ParameterizedSqlSupportsNullParameters()
    {
        var sql = SQL($"select {null}");
        PAssert.That(() => sql.CommandText() == "select NULL");
    }

    [Fact]
    public void ParameterizedSqlDoesNotUseLiteralsEnumsMarked_IEnumShouldBeParameterizedInSqlAttribute()
    {
        var sql = SQL($"select {ExampleNonLiteralEnum.SomeValue}");
        PAssert.That(() => sql.CommandText() == "select @par0");
    }

    [TestNotLiteral]
    enum ExampleNonLiteralEnum { SomeValue = 1, }
}

[AttributeUsage(AttributeTargets.Enum)]
sealed class TestNotLiteralAttribute : Attribute, IEnumShouldBeParameterizedInSqlAttribute { }
