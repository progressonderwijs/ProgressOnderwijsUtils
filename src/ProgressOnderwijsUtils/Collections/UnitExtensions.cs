namespace ProgressOnderwijsUtils.Collections;

public static class UnitExtensions
{
    public static Func<Unit> ToUnitReturningFunc(this Action action)
        => () => {
            action();
            return Unit.Value;
        };

    public static Func<TA, Unit> ToUnitReturningFunc<TA>(this Action<TA> action)
        => a => {
            action(a);
            return Unit.Value;
        };

    public static Func<TA, TB, Unit> ToUnitReturningFunc<TA, TB>(this Action<TA, TB> action)
        => (a, b) => {
            action(a, b);
            return Unit.Value;
        };

    public static Func<TA, TB, TC, Unit> ToUnitReturningFunc<TA, TB, TC>(this Action<TA, TB, TC> action)
        => (a, b, c) => {
            action(a, b, c);
            return Unit.Value;
        };

    public static Func<TA, TB, TC, TD, Unit> ToUnitReturningFunc<TA, TB, TC, TD>(this Action<TA, TB, TC, TD> action)
        => (a, b, c, d) => {
            action(a, b, c, d);
            return Unit.Value;
        };

    public static Func<TA, TB, TC, TD, TE, Unit> ToUnitReturningFunc<TA, TB, TC, TD, TE>(this Action<TA, TB, TC, TD, TE> action)
        => (a, b, c, d, e) => {
            action(a, b, c, d, e);
            return Unit.Value;
        };
}
