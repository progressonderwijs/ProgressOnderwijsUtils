using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests;

public sealed class MaybeAsyncExtensionsTest
{
    static void TestAction() { }

    static bool TestFunc(bool value)
        => value;

    static Maybe<Unit, bool> TestMaybeAction(bool value)
        => Maybe.Either(value, () => TestAction(), () => TestFunc(value));

    static Maybe<bool, bool> TestMaybeFunc(bool value)
        => Maybe.Either(value, TestFunc(value), TestFunc(value));

    static async Task AsyncTestAction()
        => await Task.Yield();

    static async Task<bool> AsyncTestFunc(bool value)
    {
        await Task.Yield();
        return TestFunc(value);
    }

    static async Task<Maybe<Unit, bool>> AsyncTestMaybeAction(bool value)
    {
        await Task.Yield();
        return TestMaybeAction(value);
    }

    static async Task<Maybe<bool, bool>> AsyncTestMaybeFunc(bool value)
    {
        await Task.Yield();
        return TestMaybeFunc(value);
    }

    [Fact]
    public async Task AsyncMaybeWhenOkAction()
    {
        var result = await AsyncTestMaybeFunc(true).WhenOk(_ => TestAction());

        result.AssertOk();
    }

    [Fact]
    public async Task AsyncMaybeWhenOkFunc()
    {
        var result = await AsyncTestMaybeFunc(true).WhenOk(value => TestFunc(value));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task SyncMaybeWhenOkAsynAction()
    {
        var result = await TestMaybeFunc(true).WhenOkAsync(async _ => await AsyncTestAction());

        result.AssertOk();
    }

    [Fact]
    public async Task SyncMaybeWhenOkAsynFunc()
    {
        var result = await TestMaybeFunc(true).WhenOkAsync(async value => await AsyncTestFunc(value));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task AsyncMaybeWhenOkAsyncAction()
    {
        var result = await AsyncTestMaybeFunc(true).WhenOkAsync(async _ => await AsyncTestAction());

        result.AssertOk();
    }

    [Fact]
    public async Task AsyncMaybeWhenOkAsyncFunc()
    {
        var result = await AsyncTestMaybeFunc(true).WhenOkAsync(async value => await AsyncTestFunc(value));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task SyncMaybeActionWhenOkTryAsyncFunc()
    {
        var result = await TestMaybeAction(true).WhenOkTryAsync(async () => await AsyncTestMaybeFunc(true));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task SyncMaybeFuncWhenOkTryAsyncFunc()
    {
        var result = await TestMaybeFunc(true).WhenOkTryAsync(async value => await AsyncTestMaybeFunc(value));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task AsyncMaybeActionWhenOkTrySyncFunc()
    {
        var result = await AsyncTestMaybeAction(true).WhenOkTry(_ => TestMaybeFunc(true));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task AsyncMaybeFuncWhenOkTryAsyncFunc()
    {
        var result = await AsyncTestMaybeFunc(true).WhenOkTryAsync(async value => await AsyncTestMaybeFunc(value));

        PAssert.That(() => result.AssertOk());
    }
}
