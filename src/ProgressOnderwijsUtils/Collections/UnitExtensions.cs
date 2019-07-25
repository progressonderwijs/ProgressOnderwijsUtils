#nullable disable
using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    public static class UnitExtensions
    {
        [NotNull]
        public static Func<Unit> ToUnitReturningFunc(this Action action)
            => () => {
                action();
                return Unit.Value;
            };

        [NotNull]
        public static Func<T, Unit> ToUnitReturningFunc<T>(this Action<T> action)
            => x => {
                action(x);
                return Unit.Value;
            };
    }
}
