using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests;

public sealed class MaybeAsyncExtensionsTest
{
    static Maybe<Unit, bool> TestMaybeAction(bool value)
        => Maybe.Either(value, Unit.Value, value);

    static Maybe<bool, bool> TestMaybeFunc(bool value)
        => Maybe.Either(value, value, value);

    static Maybe<bool, Unit> TestMaybeFuncUnitError(bool value)
        => Maybe.Either(value, value, Unit.Value);

    static async Task AsyncTestAction()
        => await Task.Yield();

    static async Task<bool> AsyncTestFunc(bool value)
    {
        await Task.Yield();
        return value;
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

    static async Task<Maybe<bool, Unit>> AsyncTestMaybeFuncUnitError(bool value)
    {
        await Task.Yield();
        return TestMaybeFuncUnitError(value);
    }

    [Fact]
    public async Task AsyncMaybeWhenOkAction()
    {
        var result = await AsyncTestMaybeFunc(true).WhenOk(_ => Unit.Value);

        result.AssertOk();
    }

    [Fact]
    public async Task AsyncMaybeWhenOkFunc()
    {
        var result = await AsyncTestMaybeFunc(true).WhenOk(value => value);

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
        var result = await AsyncTestMaybeAction(true).WhenOkTry(() => TestMaybeFunc(true));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task AsyncMaybeFuncWhenOkTryAsyncFunc()
    {
        var result = await AsyncTestMaybeFunc(true).WhenOkTryAsync(async value => await AsyncTestMaybeFunc(value));

        PAssert.That(() => result.AssertOk());
    }

    // WhenError

    [Fact]
    public async Task AsyncMaybeFuncWhenErrorFunc()
    {
        var result = await AsyncTestMaybeFunc(false).WhenError(value => !value);

        PAssert.That(() => result.AssertError());
    }

    [Fact]
    public async Task SyncMaybeFuncWhenErrorAsyncFunc()
    {
        var result = await TestMaybeFunc(false).WhenErrorAsync(async value => await AsyncTestFunc(!value));

        PAssert.That(() => result.AssertError());
    }

    [Fact]
    public async Task AsyncMaybeFuncWhenErrorAsyncFunc()
    {
        var result = await AsyncTestMaybeFunc(false).WhenErrorAsync(async value => await AsyncTestFunc(!value));

        PAssert.That(() => result.AssertError());
    }

    [Fact]
    public async Task SyncMaybeFuncWhenErrorAsyncAction()
    {
        var result = await TestMaybeFunc(false).WhenErrorAsync(async _ => await AsyncTestAction());

        result.AssertError();
    }

    [Fact]
    public async Task AsyncMaybeFuncWhenErrorAsyncAction()
    {
        var result = await AsyncTestMaybeFunc(false).WhenErrorAsync(async _ => await AsyncTestAction());

        result.AssertError();
    }

    [Fact]
    public async Task AsyncMaybeFuncWhenErrorAction()
    {
        var result = await AsyncTestMaybeFunc(false).WhenError(_ => { });

        result.AssertError();
    }

    // WhenErrorTry

    [Fact]
    public async Task SyncMaybeFuncWhenErrorTryAsyncFunc()
    {
        var result = await TestMaybeFunc(false).WhenErrorTryAsync(async value => await AsyncTestMaybeFunc(!value));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task SyncMaybeFuncUnitErrorWhenErrorTryAsyncFunc()
    {
        var result = await TestMaybeFuncUnitError(false).WhenErrorTryAsync(async () => await AsyncTestMaybeFunc(true));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task AsyncMaybeFuncWhenErrorTryAsyncFunc()
    {
        var result = await AsyncTestMaybeFunc(false).WhenErrorTryAsync(async value => await AsyncTestMaybeFunc(!value));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task AsyncMaybeFuncWhenErrorTrySyncFunc()
    {
        var result = await AsyncTestMaybeFunc(false).WhenErrorTry(value => TestMaybeFunc(!value));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task AsyncMaybeFuncUnitErrorWhenErrorTryAsyncFunc()
    {
        var result = await AsyncTestMaybeFuncUnitError(false).WhenErrorTryAsync(async () => await AsyncTestMaybeFunc(true));

        PAssert.That(() => result.AssertOk());
    }

    [Fact]
    public async Task AsyncMaybeFuncUnitErrorWhenErrorTrySyncFunc()
    {
        var result = await AsyncTestMaybeFuncUnitError(false).WhenErrorTry(() => TestMaybeFunc(true));

        PAssert.That(() => result.AssertOk());
    }

    // Extract

    [Fact]
    public async Task SyncMaybeFuncExtractAsyncBothAsync()
    {
        var result = await TestMaybeFunc(true).ExtractAsync(async ok => await AsyncTestFunc(ok), async err => await AsyncTestFunc(err));

        PAssert.That(() => result);
    }

    [Fact]
    public async Task SyncMaybeFuncExtractAsyncAsyncOkSyncError()
    {
        var result = await TestMaybeFunc(true).ExtractAsync(async ok => await AsyncTestFunc(ok), err => err);

        PAssert.That(() => result);
    }

    [Fact]
    public async Task SyncMaybeFuncExtractAsyncSyncOkAsyncError()
    {
        var result = await TestMaybeFunc(false).ExtractAsync(ok => ok, async err => await AsyncTestFunc(!err));

        PAssert.That(() => result);
    }

    [Fact]
    public async Task AsyncMaybeFuncExtractBothSync()
    {
        var result = await AsyncTestMaybeFunc(true).Extract(ok => ok, err => err);

        PAssert.That(() => result);
    }

    [Fact]
    public async Task AsyncMaybeFuncExtractAsyncBothAsync()
    {
        var result = await AsyncTestMaybeFunc(true).ExtractAsync(async ok => await AsyncTestFunc(ok), async err => await AsyncTestFunc(err));

        PAssert.That(() => result);
    }

    [Fact]
    public async Task AsyncMaybeFuncExtractAsyncAsyncOkSyncError()
    {
        var result = await AsyncTestMaybeFunc(true).ExtractAsync(async ok => await AsyncTestFunc(ok), err => err);

        PAssert.That(() => result);
    }

    [Fact]
    public async Task AsyncMaybeFuncExtractAsyncSyncOkAsyncError()
    {
        var result = await AsyncTestMaybeFunc(false).ExtractAsync(ok => ok, async err => await AsyncTestFunc(!err));

        PAssert.That(() => result);
    }
}
