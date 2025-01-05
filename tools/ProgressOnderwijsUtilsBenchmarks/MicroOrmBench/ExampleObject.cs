using System.Data.SQLite;
using Dapper;

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

    static readonly FormattableString formattableQueryString = $"""
        select top ({0}) 
            a.x + {2} as a
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
        """;

    static readonly string formatString = formattableQueryString.Format;
    public static readonly string RawQueryString = string.Format(formatString, "@Top", "@Num2", "@Arg", "@Hehe");

    public static readonly string RawSqliteQueryString = """
        select
            ex.a + @Num2 as A
            , ex.B
            , coalesce(ex.c, @Hehe) as C
            , ex.D
            , ex.E
            , @Arg as Arg
        from example ex
        limit @Top
        """;

    public static ParameterizedSql ParameterizedSqlForRows(int rows)
        => ParameterizedSql.FromSqlInterpolated(InterpolatedQuery(rows));

    public static FormattableString InterpolatedQuery(int rows)
        => FormattableStringFactory.Create(formatString, rows, 2, someInt64Value, "hehe");

    public static readonly long someInt64Value = int.MaxValue * (long)short.MinValue;

    public static void PrefillExampleTable(SQLiteConnection sqliteConn)
        //Sqlite isn't optimized for weird inline cross-joins, so to avoid measuring _that_
        //mostly materialize the underlying query into a table instead, and query from that
        => _ = sqliteConn.Execute(
            "create table example (key INTEGER PRIMARY KEY, a int null, b int not null, c TEXT, d BOOLEAN null, e int not null);\n"
            + "insert into example (a,b,c,d,e)\n"
            + RawQueryString
                .Replace("top (@Top)", "")
                .Replace(" + @Num2", "")
                .Replace(", @Arg as Arg", "")
                .Replace("@Hehe", "null")
                .Replace("N'", "'")
        );
}
