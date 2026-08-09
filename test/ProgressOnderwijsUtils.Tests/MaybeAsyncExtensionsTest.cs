using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests;

public sealed class MaybeAsyncExtensionsTest
{
    static void TestAction() { }

    static bool TestFunc(bool value)
        => value;

    static Maybe<bool, bool> TestMaybe(bool value)
        => Maybe.Either(value, TestFunc(value), TestFunc(value));

    static async Task AsyncTestAction()
        => await Task.Yield();

    static async Task<bool> AsyncTestFunc(bool value)
    {
        await Task.Yield();
        return TestFunc(value);
    }

    static async Task<Maybe<bool, bool>> AsyncTestMaybe(bool value)
    {
        await Task.Yield();
        return TestMaybe(value);
    }

    [Fact]
    public async Task AsyncMaybeWhenOkAction()
    {
        var result = await AsyncTestMaybe(true).WhenOk(_ => TestAction());

        result.AssertOk();
    }

    [Fact]
    public async Task AsyncMaybeWhenOkFunc()
    {
        var result = await AsyncTestMaybe(true).WhenOk(value => TestFunc(value));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task SyncMaybeWhenOkAsynAction()
    {
        var result = await TestMaybe(true).WhenOkAsync(async _ => await AsyncTestAction());

        result.AssertOk();
    }

    [Fact]
    public async Task SyncMaybeWhenOkAsynFunc()
    {
        var result = await TestMaybe(true).WhenOkAsync(async value => await AsyncTestFunc(value));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task AsyncMaybeWhenOkAsyncAction()
    {
        var result = await AsyncTestMaybe(true).WhenOkAsync(async _ => await AsyncTestAction());

        result.AssertOk();
    }

    [Fact]
    public async Task AsyncMaybeWhenOkAsyncFunc()
    {
        var result = await AsyncTestMaybe(true).WhenOkAsync(async value => await AsyncTestFunc(value));

        PAssert.That(() => result.AssertOk());
    }
}
