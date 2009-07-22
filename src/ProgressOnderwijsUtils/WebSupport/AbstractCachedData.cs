using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProgressOnderwijsUtils.Extensions;
using System.IO.Compression;

namespace ProgressOnderwijsUtils.WebSupport
{
	public abstract class AbstractCachedData<T> : IDisposable
	{
		T cachedItem;
		bool isItemUpToDate = false;
		FileSystemWatcher watcher;
		protected readonly DirectoryInfo dirToWatch;
		protected readonly string filter;

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

		public T Data { get { if (!isItemUpToDate) Reload(); return cachedItem; } protected set { cachedItem = value;  isItemUpToDate = true; lastWrite = WatchedFiles.Select(fileInfo => fileInfo.LastWriteTimeUtc).Max(); } }

		protected IEnumerable<FileInfo> WatchedFiles { get { return dirToWatch.DescendantFiles(filter); } }

		public DateTime LastWriteTime { get { if (!isItemUpToDate) Reload(); return lastWrite; } }


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


	public class CompressedUtf8String
	{
		static byte[] ReadFully(Stream stream) //from:http://www.yoda.arachsys.com/csharp/readbinary.html
		{
			byte[] buffer = new byte[32768];
			using (MemoryStream ms = new MemoryStream())
			{
				while (true)
				{
					int read = stream.Read(buffer, 0, buffer.Length);
					if (read <= 0)
						return ms.ToArray();
					ms.Write(buffer, 0, read);
				}
			}
		}

		public string StringData
		{
			get
			{
				using (var gzipStream = new GZipStream(new MemoryStream(GzippedData), CompressionMode.Decompress, false))
					return Encoding.UTF8.GetString(ReadFully(gzipStream));
			}
		}
		public readonly byte[] GzippedData;
		public CompressedUtf8String(string stringData)
		{
			using (MemoryStream compressedData = new MemoryStream())
			{
				var encodedData = Encoding.UTF8.GetBytes(stringData);
				using (var gzipStream = new GZipStream(compressedData, CompressionMode.Compress))
					gzipStream.Write(encodedData,0,encodedData.Length);

				GzippedData = compressedData.ToArray();
			}

		}
	}

}
