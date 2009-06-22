using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProgressOnderwijsUtils.Extensions;

namespace ProgressOnderwijsUtils.WebSupport
{
	public abstract class AbstractCachedData<T> : IDisposable
	{
		T cachedItem;
		bool isItemUpToDate = false;
		FileSystemWatcher watcher;
		DirectoryInfo dirToWatch;
		string filter;
		DateTime lastWrite;
		protected AbstractCachedData(DirectoryInfo dirToWatch, string filter, bool enableWatcher)
		{
			this.dirToWatch = dirToWatch;
			this.filter = filter;
			if (enableWatcher)
			{
				watcher = new FileSystemWatcher(dirToWatch.FullName, filter);
				watcher.IncludeSubdirectories = true;
				watcher.Changed += filesystemUpdated;
				watcher.Created += filesystemUpdated;
				watcher.Renamed += filesystemUpdated;
				watcher.Deleted += filesystemUpdated;
				watcher.EnableRaisingEvents = true;
			}
		}

		protected virtual void filesystemUpdated(object sender, FileSystemEventArgs e) { InvalidateData(); }


		public void InvalidateData() { isItemUpToDate = false; cachedItem = default(T); }

		public T Data { get { if (!isItemUpToDate) Reload(); return cachedItem; } protected set { cachedItem = value; isItemUpToDate = true; lastWrite = dirToWatch.DescendantFiles(filter).Select(fileInfo => fileInfo.LastWriteTimeUtc).Max(); } }

		public DateTime LastWriteTime { get { return lastWrite; } }

		//we need to dispose the connection.  Use the design pattern in
		//http://msdn.microsoft.com/en-us/library/b1yfkh5e.aspx
		//to do so.  This design pattern is resilient to subclassing.

		protected abstract void Reload();

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		bool disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !disposed)
				watcher.Dispose();// Free other state (managed objects).
			disposed = true;
		}
		//no finalizer since no unmanaged state.

	}
}
