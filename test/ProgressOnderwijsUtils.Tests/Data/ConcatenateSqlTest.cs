namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class ConcatenateSqlTest
{
    [Fact]
    public void ConcatenatationOfEmptySequenceIsEmpty()
    {
        PAssert.That(() => Array.Empty<ParameterizedSql>().ConcatenateSql() == EmptySql);
        var bla = SQL($"bla");
        PAssert.That(() => Array.Empty<ParameterizedSql>().ConcatenateSql(bla) == EmptySql);
    }

    [Fact]
    public void ConcatenateWithEmptySeparatorIsStillSpaced()
    {
        var een = SQL($"een");
        var twee = SQL($"twee");
        var drie = SQL($"drie");
        var expected = SQL($"een twee drie");
        PAssert.That(() => new[] { een, twee, drie, }.ConcatenateSql() == expected);
    }

    [Fact]
    public void ConcatenateWithSeparatorUsesSeparatorSpaced()
    {
        var een = SQL($"een");
        var twee = SQL($"twee");
        var drie = SQL($"drie");
        var separator = SQL($"!");
        var expected = SQL($"een ! twee ! drie");
        PAssert.That(() => new[] { een, twee, drie, }.ConcatenateSql(separator) == expected);
    }

    [Fact]
    public void ConcatenateIsFastEnoughForLargeSequences()
    {
        var someSqls = Enumerable.Range(0, 10000).Select(i => ParameterizedSql.RawSql_PotentialForSqlInjection(i.ToStringInvariant())).ToArray();
        var time = BenchTimer.Time(() => _ = someSqls.ConcatenateSql().CommandText(), 6, 1).Min;
        //At 1ns per op (equiv to approx 4 clock cycles), a quadratic implementation would use some multiple of 100 ms.  Even with an extremely low
        //scaling factor, if it's faster than 25ms, it's almost certainly better than quadratic, and in any case fast enough.
        PAssert.That(() => time.TotalMilliseconds < 25);
    }
}
