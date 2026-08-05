using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class ParameterizedSqlObjectMapperAsyncTest : TransactedLocalConnection
{
    [Fact]
    public async Task ReadScalarAsync_returns_scalar_value()
    {
        var result = await SQL($"select 42").ReadScalarAsync<int>(Connection, TestContext.Current.CancellationToken);
        PAssert.That(() => result == 42);
    }

    [Fact]
    public async Task ReadScalarAsync_returns_null_for_null_value()
    {
        var result = await SQL($"select cast(null as nvarchar(10))").ReadScalarAsync<string>(Connection, TestContext.Current.CancellationToken);
        PAssert.That(() => result == null);
    }

    [Fact]
    public async Task ReadScalarAsync_returns_string_value()
    {
        var result = await SQL($"select {"hello"}").ReadScalarAsync<string>(Connection, TestContext.Current.CancellationToken);
        PAssert.That(() => result == "hello");
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_executes_without_error()
    {
        await SQL($"select 1").ExecuteNonQueryAsync(Connection, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ReadPocosAsync_returns_pocos()
    {
        var results = await SQL($"select Id = 1, Name = N'test' union all select 2, N'other'").ReadPocosAsync<SimpleRow>(Connection, TestContext.Current.CancellationToken);
        PAssert.That(() => results.Length == 2);
        PAssert.That(() => results[0].Id == 1 && results[0].Name == "test");
        PAssert.That(() => results[1].Id == 2 && results[1].Name == "other");
    }

    [Fact]
    public async Task ReadPlainAsync_returns_values()
    {
        var results = await SQL($"select 1 union all select 2 union all select 3").ReadPlainAsync<int>(Connection, TestContext.Current.CancellationToken);
        PAssert.That(() => results.Length == 3);
        PAssert.That(() => results[0] == 1 && results[1] == 2 && results[2] == 3);
    }

    [Fact]
    public async Task ReadTuplesAsync_returns_tuples()
    {
        var results = await SQL($"select 1, N'a' union all select 2, N'b'").ReadTuplesAsync<(int, string)>(Connection, TestContext.Current.CancellationToken);
        PAssert.That(() => results.Length == 2);
        PAssert.That(() => results[0].Item1 == 1 && results[0].Item2 == "a");
        PAssert.That(() => results[1].Item1 == 2 && results[1].Item2 == "b");
    }

    sealed record SimpleRow : IWrittenImplicitly
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}

