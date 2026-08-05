using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class ReadScalarAsyncTest : TransactedLocalConnection
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
}
