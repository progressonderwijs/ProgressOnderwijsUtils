using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Core;

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

		public static Lazy<ILog> For<T>() { return LoggerCache<T>.Log; }
		public static Lazy<ILog> For<T>(T obj) { return LoggerCache<T>.Log; }
		public static Lazy<ILog> For(Type type) {
			return dict.GetOrAdd(type, _type => 
				new Lazy<ILog>(() =>
					LogManager.GetLogger(CurrentAssembly, _type), LazyThreadSafetyMode.PublicationOnly)
				);
		}
	}

}
