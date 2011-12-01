using System;
using System.Linq;
using log4net;

namespace ProgressOnderwijsUtils.Log4Net
{
	public static class ILogExtensions
	{
		public static void Debug(this ILog log, Func<object> msg)
		{
			if (log.IsDebugEnabled) log.Debug(msg());
		}

		public static void Debug(this ILog log, Func<object> msg, Exception exception)
		{
			if (log.IsDebugEnabled) log.Debug(msg(), exception);
		}

		public static void Info(this ILog log, Func<object> msg)
		{
			if (log.IsInfoEnabled) log.Info(msg());
		}

		public static void Info(this ILog log, Func<object> msg, Exception exception)
		{
			if (log.IsInfoEnabled) log.Info(msg(), exception);
		}

		public static void Warn(this ILog log, Func<object> msg)
		{
			if (log.IsWarnEnabled) log.Warn(msg());
		}

		public static void Warn(this ILog log, Func<object> msg, Exception exception)
		{
			if (log.IsWarnEnabled) log.Warn(msg(), exception);
		}

		public static void Error(this ILog log, Func<object> msg)
		{
			if (log.IsErrorEnabled) log.Error(msg());
		}

		public static void Error(this ILog log, Func<object> msg, Exception exception)
		{
			if (log.IsErrorEnabled) log.Error(msg(), exception);
		}

		public static void Fatal(this ILog log, Func<object> msg)
		{
			if (log.IsFatalEnabled) log.Fatal(msg());
		}

		public static void Fatal(this ILog log, Func<object> msg, Exception exception)
		{
			if (log.IsFatalEnabled) log.Fatal(msg(), exception);
		}
	}
}
