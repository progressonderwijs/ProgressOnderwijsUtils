using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public static class ThreadSafeMemoizerExtension
    {
        public static Func<T, TR> MemoizeConcurrent<T, TR>(this Func<T, TR> v)
        {
            var cache = new ConcurrentDictionary<T, TR>();
            return arg => cache.GetOrAdd(arg, v);
        }

        public static Func<TContext, TKey, TR> MemoizeConcurrent<TContext, TKey, TR>(this Func<TContext, TKey, TR> v) where TContext : IDisposable
        {
            var cache = new ConcurrentDictionary<TKey, TR>();
            return (ctx, arg) => cache.GetOrAdd(arg, _ => v(ctx, arg));
        }
    }
}
