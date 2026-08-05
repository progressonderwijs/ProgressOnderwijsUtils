using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests;

public sealed class MaybeAsyncTests
{
    static async ValueTask<int> AsyncDouble(int x)
    {
        await Task.Yield();
        return x * 2;
    }

    static async ValueTask<Maybe<int, string>> AsyncValidate(int x)
    {
        await Task.Yield();
        return x > 0 ? Maybe.Ok(x).AsMaybeWithoutError<string>() : Maybe.Error("must be positive").AsMaybeWithoutValue<int>();
    }

    [Fact]
    public async ValueTask WhenOkAsync_chain_propagates_ok_through_multiple_async_steps()
    {
        var result = await Maybe.Ok(5).AsMaybeWithoutError<string>()
            .WhenOkAsync(async x => {
                    await Task.Yield();
                    return x + 1;
                }
            )
            .WhenOkAsync(async x => {
                    await Task.Yield();
                    return x * 10;
                }
            )
            .WhenOkAsync(async x => {
                    await Task.Yield();
                    return x.ToString();
                }
            );

        PAssert.That(() => result.AssertOk() == "60");
    }

    [Fact]
    public async ValueTask WhenOkAsync_chain_shortcircuits_on_error()
    {
        var callCount = 0;
        var result = await Maybe.Error("fail").AsMaybeWithoutValue<int>()
            .WhenOkAsync(async x => {
                    callCount++;
                    await Task.Yield();
                    return x + 1;
                }
            )
            .WhenOkAsync(async x => {
                    callCount++;
                    await Task.Yield();
                    return x * 10;
                }
            );

        PAssert.That(() => result.IsError());
        PAssert.That(() => result.AssertError() == "fail");
        PAssert.That(() => callCount == 0);
    }

    [Fact]
    public async ValueTask WhenOkTryAsync_chain_with_validation()
    {
        var result = await Maybe.Ok(3).AsMaybeWithoutError<string>()
            .WhenOkTryAsync(async x => {
                    await Task.Yield();
                    return await AsyncValidate(x);
                }
            )
            .WhenOkAsync(async x => await AsyncDouble(x));

        PAssert.That(() => result.AssertOk() == 6);
    }

    [Fact]
    public async ValueTask WhenOkTryAsync_chain_stops_at_first_error()
    {
        var result = await Maybe.Ok(-1).AsMaybeWithoutError<string>()
            .WhenOkTryAsync(async x => {
                    await Task.Yield();
                    return await AsyncValidate(x);
                }
            )
            .WhenOkAsync(async x => await AsyncDouble(x));

        PAssert.That(() => result.AssertError() == "must be positive");
    }

    [Fact]
    public async ValueTask Mixed_WhenOk_and_WhenError_async_chain()
    {
        var result = await Maybe.Ok(10).AsMaybeWithoutError<string>()
            .WhenOkAsync(async x => {
                    await Task.Yield();
                    return x + 5;
                }
            )
            .WhenOkAsync(x => x * 2)
            .WhenErrorAsync(async err => {
                    await Task.Yield();
                    return $"Error: {err}";
                }
            );

        PAssert.That(() => result.AssertOk() == 30);
    }

    [Fact]
    public async ValueTask WhenErrorAsync_transforms_error_in_chain()
    {
        var result = await Maybe.Error("oops").AsMaybeWithoutValue<int>()
            .WhenOkAsync(async x => {
                    await Task.Yield();
                    return x + 1;
                }
            )
            .WhenErrorAsync(TransformError);

        PAssert.That(() => result.AssertError() == "transformed: oops");

        static async Task<string> TransformError(string err)
        {
            await Task.Yield();
            return $"transformed: {err}";
        }
    }

    [Fact]
    public async ValueTask Full_async_chain_with_multiple_WhenOkTry_steps()
    {
        var result = await Maybe.Ok(2).AsMaybeWithoutError<string>()
            .WhenOkTryAsync(async x => {
                    await Task.Yield();
                    return await AsyncValidate(x);
                }
            )
            .WhenOkAsync(async x => await AsyncDouble(x))
            .WhenOkTryAsync(async x => {
                    await Task.Yield();
                    return await AsyncValidate(x);
                }
            )
            .WhenOkAsync(async x => await AsyncDouble(x));

        PAssert.That(() => result.AssertOk() == 8);
    }

    [Fact]
    public async ValueTask ExtractAsync_on_task_maybe()
    {
        var result = await Maybe.Ok(7).AsMaybeWithoutError<string>()
            .WhenOkAsync(async x => {
                    await Task.Yield();
                    return x * 3;
                }
            )
            .ExtractAsync(ok => $"got {ok}", err => $"error: {err}");

        PAssert.That(() => result == "got 21");
    }

    [Fact]
    public async ValueTask ExtractAsync_on_error_task_maybe()
    {
        var result = await Maybe.Error("bad").AsMaybeWithoutValue<int>()
            .WhenOkAsync(async x => {
                    await Task.Yield();
                    return x * 3;
                }
            )
            .ExtractAsync(ok => $"got {ok}", err => $"error: {err}");

        PAssert.That(() => result == "error: bad");
    }

    [Fact]
    public async ValueTask Sync_WhenOkTryAsync_on_task_maybe()
    {
        var result = await Maybe.Ok(5).AsMaybeWithoutError<string>()
            .WhenOkAsync(async x => {
                    await Task.Yield();
                    return x + 5;
                }
            )
            .WhenOkTryAsync(x => x > 8 ? Maybe.Ok(x).AsMaybeWithoutError<string>() : Maybe.Error("too small").AsMaybeWithoutValue<int>());

        PAssert.That(() => result.AssertOk() == 10);
    }
}
