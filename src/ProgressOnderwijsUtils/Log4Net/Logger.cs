using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using log4net;

namespace ProgressOnderwijsUtils.Log4Net
{
    public static class LazyLog
    {
        static readonly Assembly CurrentAssembly = typeof(LazyLog).Assembly;
        static readonly ConcurrentDictionary<Type, Lazy<ILog>> dict = new ConcurrentDictionary<Type, Lazy<ILog>>();

        static class LoggerCache<T>
        {
            public static readonly Lazy<ILog> Log = For(typeof(T));
        }

        public static Lazy<ILog> For<T>() => LoggerCache<T>.Log;

        public static Lazy<ILog> For([NotNull] Type type)
        {
            return dict.GetOrAdd(
                type,
                _type =>
                    new Lazy<ILog>(
                        () =>
                            LogManager.GetLogger(CurrentAssembly, _type),
                        LazyThreadSafetyMode.PublicationOnly)
                );
        }
    }
}
