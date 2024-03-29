namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class TestSqlParameterComponent : TransactedLocalConnection
{
    [Fact]
    public void ValidatesArgumentsOK()
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        _ = Assert.Throws<ArgumentNullException>(() => ParameterizedSql.RawSql_PotentialForSqlInjection(null!));

        PAssert.That(() => ParameterizedSql.RawSql_PotentialForSqlInjection("bla" + 0).GetHashCode() == ParameterizedSql.RawSql_PotentialForSqlInjection("bla0").GetHashCode());
        PAssert.That(() => ParameterizedSql.RawSql_PotentialForSqlInjection("bla" + 0).GetHashCode() != ParameterizedSql.RawSql_PotentialForSqlInjection("bla").GetHashCode());
        PAssert.That(() => ParameterizedSql.RawSql_PotentialForSqlInjection("bla" + 0).Equals(ParameterizedSql.RawSql_PotentialForSqlInjection("bla0")));

        PAssert.That(() => ParameterizedSql.Param("bla" + 0).GetHashCode() == ParameterizedSql.Param("bla0").GetHashCode());
        PAssert.That(() => ParameterizedSql.Param("bla" + 0).GetHashCode() != ParameterizedSql.Param("bla").GetHashCode());
        PAssert.That(() => ParameterizedSql.Param("bla" + 0).Equals(ParameterizedSql.Param("bla0")));

        var someday = new DateTime(2012, 3, 3, 23, 0, 0, DateTimeKind.Utc);
        PAssert.That(() => ParameterizedSql.Param(someday).DebugText() == "'2012-03-03T23:00:00.000Z'");
        PAssert.That(() => ParameterizedSql.Param(null).DebugText() == "NULL");
        PAssert.That(() => ParameterizedSql.Param("abc").DebugText() == "'abc'");
        PAssert.That(() => ParameterizedSql.Param("ab'c").DebugText() == "'ab''c'");
        PAssert.That(() => ParameterizedSql.Param(12345).DebugText() == "12345");
        PAssert.That(() => ParameterizedSql.Param(12345.6m).DebugText() == "12345.6");
        PAssert.That(() => ParameterizedSql.Param(new()).DebugText() == "{!System.Object!}");

        var asString = Wrap("Aap");
        PAssert.That(() => asString == ParameterizedSql.Param("Aap"), "TODO: We would like this to be conceptually different parameters");
        PAssert.That(() => asString.CommandText() == "@par0");

        var asDateTime = Wrap(new DateTime(2010, 1, 1));
        PAssert.That(() => asDateTime == ParameterizedSql.Param(new DateTime(2010, 1, 1)), "TODO: We would like this to be conceptually different parameters");
        PAssert.That(() => asDateTime.CommandText() == "@par0");

        var asNullableEnum = ParameterizedSql.Param(default(TrivialValue<DayOfWeek>?));
        PAssert.That(() => asNullableEnum == ParameterizedSql.Param(null));
        PAssert.That(() => asNullableEnum.CommandText() == "NULL");

        var asNullableString = ParameterizedSql.Param(default(TrivialValue<string>?));
        PAssert.That(() => asNullableString == ParameterizedSql.Param(null));
        PAssert.That(() => asNullableString.CommandText() == "NULL");

        var asEnum = Wrap(DayOfWeek.Friday);
        PAssert.That(() => asEnum == ParameterizedSql.Param(DayOfWeek.Friday), "TODO: We would like this to be conceptually different parameters");
        PAssert.That(() => asEnum.CommandText() == "5/*DayOfWeek.Friday*/");

        var asBool = Wrap(false);
        PAssert.That(() => asBool == ParameterizedSql.Param(false), "TODO: We would like this to be conceptually different parameters");
        PAssert.That(() => asBool.CommandText() == "cast(0 as bit)");
    }

    [Fact]
    public void DeDuplication_of_convertable_parameters()
    {
        var sql_double_3 = SQL($"select {3}, {3}");
        PAssert.That(() => sql_double_3.CommandText() == "select @par0, @par0");
        var sql_double_wrap_3 = SQL($"select {Wrap(3)}, {Wrap(3)}");
        PAssert.That(() => sql_double_wrap_3.CommandText() == "select @par0, @par0");
        var sql_3_wrap_3 = SQL($"select {3}, {Wrap(3)}");
        PAssert.That(() => sql_3_wrap_3.CommandText() == "select @par0, @par0", "TODO: We would like this to be conceptually different parameters");
        var sql_3_convertible_3 = SQL($"select {3}, {TrivialConvertibleValue.Create(3)}");
        PAssert.That(() => sql_3_convertible_3.CommandText() == "select @par0, @par0", "TODO: We would like this to be conceptually different parameters");
        var sql_wrap_3_convertible_3 = SQL($"select {Wrap(3)}, {TrivialConvertibleValue.Create(3)}");
        PAssert.That(() => sql_wrap_3_convertible_3.CommandText() == "select @par0, @par0");
    }

    [Fact]
    public void ValidatesConvertibleArgumentsOK()
    {
        var asString = Wrap("Aap");
        using var transactedLocalConnection = new TransactedLocalConnection();
        var sql = SQL($"select {asString}");
        PAssert.That(() => sql.ReadScalar<string>(transactedLocalConnection.Connection) == "Aap");
    }

    static ParameterizedSql Wrap<T>(T value)
        => ParameterizedSql.Param(TrivialConvertibleValue.Create(value));
}
