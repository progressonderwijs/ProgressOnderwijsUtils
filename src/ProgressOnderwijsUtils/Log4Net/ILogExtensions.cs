using System;
using JetBrains.Annotations;
using log4net;

namespace ProgressOnderwijsUtils.Log4Net
{
    public static class ILogExtensions
    {
        // ReSharper disable UnusedMember.Global
        // Utility methods, and we want people to see those as alternatives,
        // since logging might be expensive, and it's good to see the alternative
        public static void Debug([NotNull] this ILog log, Func<object> msg)
        {
            if (log.IsDebugEnabled) {
                log.Debug(msg());
            }
        }

        public static void Debug([NotNull] this ILog log, Func<object> msg, Exception exception)
        {
            if (log.IsDebugEnabled) {
                log.Debug(msg(), exception);
            }
        }

        public static void Debug([NotNull] this Lazy<ILog> log, Func<object> msg)
        {
            if (log.Value.IsDebugEnabled) {
                log.Value.Debug(msg());
            }
        }

        public static void Debug([NotNull] this Lazy<ILog> log, object msg)
        {
            log.Value.Debug(msg);
        }

        public static void DebugFormat([NotNull] this Lazy<ILog> log, string format, params object[] args)
        {
            log.Value.DebugFormat(format, args);
        }

        public static void Info([NotNull] this ILog log, Func<object> msg)
        {
            if (log.IsInfoEnabled) {
                log.Info(msg());
            }
        }

        public static void Info([NotNull] this ILog log, Func<object> msg, Exception exception)
        {
            if (log.IsInfoEnabled) {
                log.Info(msg(), exception);
            }
        }

        public static void Info([NotNull] this Lazy<ILog> log, Func<object> msg)
        {
            if (log.Value.IsInfoEnabled) {
                log.Value.Info(msg());
            }
        }

        public static void Info([NotNull] this Lazy<ILog> log, object msg)
        {
            log.Value.Info(msg);
        }

        public static void Warn([NotNull] this ILog log, Func<object> msg)
        {
            if (log.IsWarnEnabled) {
                log.Warn(msg());
            }
        }

        public static void Warn([NotNull] this ILog log, Func<object> msg, Exception exception)
        {
            if (log.IsWarnEnabled) {
                log.Warn(msg(), exception);
            }
        }

        public static void Warn([NotNull] this Lazy<ILog> log, Func<object> msg)
        {
            if (log.Value.IsWarnEnabled) {
                log.Value.Warn(msg());
            }
        }

        public static void Warn([NotNull] this Lazy<ILog> log, object msg)
        {
            log.Value.Warn(msg);
        }

        public static void WarnFormat([NotNull] this Lazy<ILog> log, string format, params object[] args)
        {
            log.Value.WarnFormat(format, args);
        }

        public static void Error([NotNull] this ILog log, Func<object> msg)
        {
            if (log.IsErrorEnabled) {
                log.Error(msg());
            }
        }

        public static void Error([NotNull] this ILog log, Func<object> msg, Exception exception)
        {
            if (log.IsErrorEnabled) {
                log.Error(msg(), exception);
            }
        }

        public static void Error([NotNull] this Lazy<ILog> log, Func<object> msg)
        {
            if (log.Value.IsErrorEnabled) {
                log.Value.Error(msg());
            }
        }

        public static void Error([NotNull] this Lazy<ILog> log, object msg)
        {
            log.Value.Error(msg);
        }

        public static void ErrorFormat([NotNull] this Lazy<ILog> log, string format, params object[] args)
        {
            log.Value.ErrorFormat(format, args);
        }

        public static void Fatal([NotNull] this ILog log, Func<object> msg)
        {
            if (log.IsFatalEnabled) {
                log.Fatal(msg());
            }
        }

        public static void Fatal([NotNull] this ILog log, Func<object> msg, Exception exception)
        {
            if (log.IsFatalEnabled) {
                log.Fatal(msg(), exception);
            }
        }

        // ReSharper restore UnusedMember.Global
    }
}
