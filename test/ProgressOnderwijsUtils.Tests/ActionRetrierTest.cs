namespace ProgressOnderwijsUtils.Tests;

public sealed class ActionRetrierTest
{
    [Fact]
    public void DoesNotRetryOnSuccess()
    {
        var immediateRetry = new ActionRetrier((e, n) => null);
        var count = 0;
        _ = immediateRetry.ExecuteWithRetries(() => count++, CancellationToken.None);
        PAssert.That(() => count == 1);
    }

    [Fact]
    public void RetriesNoMoreThanRequestedOnSuccess()
    {
        var immediateRetry = new ActionRetrier((e, n) => n < 3 ? TimeSpan.Zero : null);
        var count = 0;
        try {
            _ = immediateRetry.ExecuteWithRetries<Unit>(
                () => {
                    count++;
                    throw new();
                },
                CancellationToken.None
            );
        } catch { //by design
        }
        PAssert.That(() => count == 3);
    }

    [Fact]
    public void CanFilterExceptions()
    {
        var immediateRetry = new ActionRetrier((e, n) => e is InvalidOperationException ? TimeSpan.Zero : null);
        var count = 0;
        try {
            _ = immediateRetry.ExecuteWithRetries<Unit>(
                () => {
                    count++;
                    if (count < 42) {
                        throw new InvalidOperationException();
                    }
                    throw new();
                },
                CancellationToken.None
            );
        } catch { //by design
        }
        PAssert.That(() => count == 42);
    }

    [Fact]
    public void ActuallyWaitsToo()
    {
        var immediateRetry = new ActionRetrier((e, n) => TimeSpan.FromSeconds(0.5));
        var sw = Stopwatch.StartNew();

        var count = 0;
        _ = immediateRetry.ExecuteWithRetries(
            () => {
                count++;
                if (count < 2) {
                    throw new();
                }
                return Unit.Value;
            },
            CancellationToken.None
        );
        var took = sw.Elapsed;
        PAssert.That(() => TimeSpan.FromSeconds(0.1) < took);
    }

    [Fact]
    public void DoesntRetryWhenCancelled()
    {
        var immediateRetry = new ActionRetrier((e, n) => TimeSpan.Zero);

        var count = 0;
        var ex = Assert.ThrowsAny<ApplicationException>(
            (Action)(
                () => {
                    _ = immediateRetry.ExecuteWithRetries(
                        () => {
                            count++;
                            if (count < 10) {
                                throw new ApplicationException($"not caught: {count}");
                            }
                            return Unit.Value;
                        },
                        new(true)
                    );
                    throw new("not reached");
                })
        );
        PAssert.That(() => count == 1 && ex.Message == "not caught: 1");
    }
}
