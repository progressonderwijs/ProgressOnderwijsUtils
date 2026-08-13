using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Collections;

public static class UnitExtensionsAsync
{
    public static Func<Task<Unit>> ToUnitReturningFunc(this Func<Task> action)
        => async () => {
            await action().ConfigureAwait(false);
            return Unit.Value;
        };

    public static Func<TA, Task<Unit>> ToUnitReturningFunc<TA>(this Func<TA, Task> action)
        => async a => {
            await action(a).ConfigureAwait(false);
            return Unit.Value;
        };

    public static Func<TA, TB, Task<Unit>> ToUnitReturningFunc<TA, TB>(this Func<TA, TB, Task> action)
        => async (a, b) => {
            await action(a, b).ConfigureAwait(false);
            return Unit.Value;
        };

    public static Func<TA, TB, TC, Task<Unit>> ToUnitReturningFunc<TA, TB, TC>(this Func<TA, TB, TC, Task> action)
        => async (a, b, c) => {
            await action(a, b, c).ConfigureAwait(false);
            return Unit.Value;
        };

    public static Func<TA, TB, TC, TD, Task<Unit>> ToUnitReturningFunc<TA, TB, TC, TD>(this Func<TA, TB, TC, TD, Task> action)
        => async (a, b, c, d) => {
            await action(a, b, c, d).ConfigureAwait(false);
            return Unit.Value;
        };

    public static Func<TA, TB, TC, TD, TE, Task<Unit>> ToUnitReturningFunc<TA, TB, TC, TD, TE>(this Func<TA, TB, TC, TD, TE, Task> action)
        => async (a, b, c, d, e) => {
            await action(a, b, c, d, e).ConfigureAwait(false);
            return Unit.Value;
        };
}
