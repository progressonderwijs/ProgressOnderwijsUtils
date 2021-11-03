using System;
using System.Collections.Concurrent;

namespace ProgressOnderwijsUtils
{
    public static class Memoizer
    {
        public static Func<TIn, TOut> ThreadSafeMemoize<TIn, TOut>(this Func<TIn, TOut> func)
            where TIn : notnull
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(TIn))) {
                throw new InvalidOperationException("Disposables zijn niet geschikt als key.");
            }
            var cache = new ConcurrentDictionary<TIn, Func<TOut>>();
            var valueFactory = Utils.F((TIn input) => Utils.Lazy(() => func(input)));
            return input => cache.GetOrAdd(input, valueFactory)();
        }
    }
}
