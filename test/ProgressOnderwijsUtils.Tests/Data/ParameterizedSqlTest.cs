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
            a1 = ParameterizedSql.RawSql_PotentialForSqlInjection("a"),
            a2 = ParameterizedSql.RawSql_PotentialForSqlInjection("aa"[1..]); //substring to avoid reference equality

        PAssert.That(() => a1 == a2);
        PAssert.That(() => !(a1 != a2));
        PAssert.That(() => a1.Equals(a2));
        PAssert.That(() => Equals(a1, a2));
    }

    [Fact]
    public void NonIdenticalDynamicSqlAreNotEquals()
    {
        ParameterizedSql
            a = ParameterizedSql.RawSql_PotentialForSqlInjection("a"),
            b = ParameterizedSql.RawSql_PotentialForSqlInjection("b");

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
            withParEnum_1_2 = SQL($"a param: {[(DayOfWeek)1, (DayOfWeek)2,]}"),
            withParString_1_2 = SQL($"a param: {["1", "2",]}"),
            withParString_1_2b = SQL($"a param: {["1", "2",]}")
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
    public void EscapedSqlObjectName_works()
    {
        PAssert.That(() => ParameterizedSql.EscapedSqlObjectName("foo").CommandText() == "[foo]");
        PAssert.That(() => ParameterizedSql.EscapedSqlObjectName("foo 'bar").CommandText() == "[foo 'bar]");
        PAssert.That(() => ParameterizedSql.EscapedSqlObjectName("test [ this ]").CommandText() == "[test [ this ]]]");
    }

    [Fact]
    public void EscapedLiteralString_works()
    {
        PAssert.That(() => ParameterizedSql.EscapedLiteralString("foo").CommandText() == "'foo'");
        PAssert.That(() => ParameterizedSql.EscapedLiteralString("foo [%bar").CommandText() == "'foo [%bar'");
        PAssert.That(() => ParameterizedSql.EscapedLiteralString("test ' this '").CommandText() == "'test '' this '''");
    }

    [Fact]
    public void LiteralSqlNumericString_works()
    {
        PAssert.That(() => ParameterizedSql.LiteralSqlNumericString(0).CommandText() == "0");
        PAssert.That(() => ParameterizedSql.LiteralSqlNumericString(-100_000).CommandText() == "-100000");
        PAssert.That(() => ParameterizedSql.LiteralSqlNumericString(23456).CommandText() == "23456");
    }

    [Fact]
    public void ParameterizedSqlValidation()
        // ReSharper disable once NullableWarningSuppressionIsUsed
        => _ = Assert.Throws<ArgumentNullException>(() => _ = ParameterizedSql.RawSql_PotentialForSqlInjection(null!));

    [Fact]
    public void ValidIdentifierCharsOnly_ThrowsOnInvalid()
    {
        _ = ParameterizedSql.UnescapedSqlIdentifier("a$b"); //assert does not throw
        _ = ParameterizedSql.UnescapedSqlIdentifier("#tempTable"); //assert does not throw
        _ = ParameterizedSql.UnescapedSqlIdentifier("bla"); //assert does not throw
        _ = ParameterizedSql.UnescapedSqlIdentifier("bla0"); //assert does not throw

        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier(""));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier("--\n\n drop bobby tables"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier(" bla"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier("bl a"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier("0bla"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier("!iets"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier("(expression)"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier("a.b"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier("$ab"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.UnescapedSqlIdentifier("[ab]"));
    }

    [Fact]
    public void AssertQualifiedSqlIdentifier_ThrowsOnInvalid()
    {
        _ = ParameterizedSql.AssertQualifiedSqlIdentifier("a.b"); //assert does not throw
        _ = ParameterizedSql.AssertQualifiedSqlIdentifier("a$b.c$d"); //assert does not throw
        _ = ParameterizedSql.AssertQualifiedSqlIdentifier("#temptable.col"); //assert does not throw
        _ = ParameterizedSql.AssertQualifiedSqlIdentifier("hmm.#dubious"); //assert does not throw, but not dangerous
        _ = ParameterizedSql.AssertQualifiedSqlIdentifier("bla0.col"); //assert does not throw, but not dangerous

        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier(""));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("--\n\n drop bobby tables"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("#tempTable"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier(" bla"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("bl a"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("bla"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("0bla"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("bla0"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("!iets"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("(expression)"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("$ab"));

        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("q. bla"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("q.bl a"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("q.0bla"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("q.!iets"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("q.(expression)"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("q.$ab"));

        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier(" bla.col"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("bl a.col"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("0bla.col"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("!iets.col"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("(expression).col"));
        _ = Assert.Throws<Exception>(() => _ = ParameterizedSql.AssertQualifiedSqlIdentifier("$ab.col"));
    }

    [Fact]
    public void OperatorAndReturnsSqlWhenTrue()
    {
        var trueCondition = true;
        var falseCondition = false;

        var testSql = SQL($"test");
        PAssert.That(() => (trueCondition && testSql) == testSql);
        PAssert.That(() => ParameterizedSql.TruthyEmpty != EmptySql);
        PAssert.That(() => falseCondition == EmptySql);
        PAssert.That(() => trueCondition == ParameterizedSql.TruthyEmpty);
        var test2Sql = SQL($"test2");
        PAssert.That(() => (falseCondition && testSql || test2Sql) == test2Sql);
        var maybeWithNested = SQL($"maybe-{falseCondition && testSql}");
        var maybeJustDash = SQL($"maybe-");
        PAssert.That(() => maybeWithNested == maybeJustDash);
        PAssert.That(() => (trueCondition || testSql) == ParameterizedSql.TruthyEmpty);
        PAssert.That(() => (trueCondition || testSql) != EmptySql);
        PAssert.That(() => (falseCondition || testSql) == testSql);
        var whenFalseSql = SQL($"WhenFalse");
        PAssert.That(() => (testSql + falseCondition || whenFalseSql) == testSql);
        var nestedEmptySql = SQL($"{ParameterizedSql.TruthyEmpty}");
        PAssert.That(() => nestedEmptySql == EmptySql);

        var nestedConditionalTest = SQL($"{trueCondition && testSql}");
        PAssert.That(() => nestedConditionalTest == testSql);
        var nestedParamTrue = SQL($"{trueCondition}");
        PAssert.That(() => nestedParamTrue == ParameterizedSql.Param(true));
    }

    [Fact]
    public void PrependingEmptyHasNoEffect()
    {
        var abcSql = SQL($"abc");
        PAssert.That(() => EmptySql + abcSql == abcSql);
    }

    [Fact]
    public void EmptyParameterizedSql()
    {
        //we want to check that various subtly different ways of making empty ParameterizedSqls
        //all behave as expected.
        var qEmpty0 = default(ParameterizedSql);
        var qEmpty1 = EmptySql;
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
        var debugText = badQuery.DebugText();
        PAssert.That(() => debugText == "A{x1}Z");
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
                , c = {{SQL($"[{'a'}{"$@"}]")}}
            ;
            """
        );

        using var cmd = result.CreateSqlCommand(new(), CommandTimeout.WithoutTimeout);

        var expectedCommandText = @"select a=@par0
    , b = 'consider {}'
    , c = [@par1@par2]
;";
        PAssert.That(() => cmd.Command.CommandText == expectedCommandText);
        var generatedParameters = cmd.Command.Parameters.Cast<SqlParameter>().Select(p => (p.Value, p.SqlDbType)).ToArray();
        var expectedParameters = new[] {
            (Value: (object)42, SqlDbType: SqlDbType.Int),
            ('a', SqlDbType.NVarChar),
            ("$@", SqlDbType.NVarChar),
        };
        PAssert.That(() => generatedParameters.SequenceEqual(expectedParameters));
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
        var sql = SQL($"select {default(int?)}");
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

    [Fact]
    public void GenerateUniqueQueryAlias_is_in_fact_unique_within_query()
    {
        var basicQuery = "select 1";
        var alias = ParameterizedSqlSubQueries.GenerateUniqueQueryAlias(42, basicQuery).CommandText();
        PAssert.That(() => alias == "_cavklvebpn", "De query alias moet determinisch zijn");

        var sneakyQuery = "select cavklvebpn = 1";
        var sneakyAlias = ParameterizedSqlSubQueries.GenerateUniqueQueryAlias(42, sneakyQuery).CommandText();
        PAssert.That(() => sneakyAlias == "_wtlrahmndzh", "De query alias neemt een tweede keuze in geval van conflict");

        var sneakyQuery2 = "select cavklvebpn = 1, wtlrahmndzh = 2 ";
        var sneakyAlias2 = ParameterizedSqlSubQueries.GenerateUniqueQueryAlias(42, sneakyQuery2).CommandText();
        PAssert.That(() => sneakyAlias2 == "_nfvmwjxxfkoz", "De query alias neemt een tweede keuze in geval van conflict");
    }
}

[AttributeUsage(AttributeTargets.Enum)]
sealed class TestNotLiteralAttribute : Attribute, IEnumShouldBeParameterizedInSqlAttribute { }
