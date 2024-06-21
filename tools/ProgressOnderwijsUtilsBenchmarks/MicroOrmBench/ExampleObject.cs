namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench;

public sealed class ExampleObject : IWrittenImplicitly
{
    public int? A { get; set; }
    public int B { get; set; }
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? C { get; set; }
    public bool? D { get; set; }
    public int E { get; set; }
    public long Arg { get; set; }

    static readonly FormattableString formattableQueryString = $@"
            select top ({0}) 
                a.x+{2} as a
                , b.x as b
                , c.x as c
                , d.x as d
                , e.x as e
                , {default(long)} as Arg
            from       (select 0 as x union all select 1 union all select null) a
            cross join (select 0 as x union all select 1 union all select 2) b
            cross join (select N'abracadabra fee fi fo fum' as x union all select {"hehe"} union all select N'quick brown fox') c
            cross join (select cast(1 as bit) as x union all select cast(0 as bit) union all select null) d
            cross join (select 0 as x union all select 1 union all select 2) e
            cross join (select 0 as x union all select 1 union all select 2 union all select 3) f
            cross join (select 0 as x union all select 1 union all select 2 union all select 3) g
        ";

    static readonly string formatString = formattableQueryString.Format;
    public static readonly string RawQueryString = string.Format(formatString, "@Top", "@Num2", "@Arg", "@Hehe");

    public static ParameterizedSql ParameterizedSqlForRows(int rows)
        => ParameterizedSql.FromSqlInterpolated(InterpolatedQuery(rows));

    public static FormattableString InterpolatedQuery(int rows)
        => FormattableStringFactory.Create(formatString, rows, 2, someInt64Value, "hehe");

    static readonly FormattableString formattableSqliteQueryString = $@"
            select
                ex.a + {2} as a
                , ex.b
                , coalesce(ex.c, {"hehe"}) as c
                , ex.d
                , ex.e
                , {default(long)} as Arg
            from example ex
            limit {0}
        ";

    static readonly string formatSqliteString = formattableSqliteQueryString.Format;
    public static readonly string RawSqliteQueryString = string.Format(formatSqliteString, "@Num2", "@Hehe", "@Arg", "@Top");
    public static readonly long someInt64Value = int.MaxValue * (long)short.MinValue;

    public static ParameterizedSql ParameterizedSqliteForRows(int rows)
        => ParameterizedSql.FromSqlInterpolated(FormattableStringFactory.Create(formatSqliteString, 2, "hehe", someInt64Value, rows));
}
