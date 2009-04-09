using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Xml.Linq;

namespace ProgressOnderwijsUtils.ErrorHandling
{
	public static class LocalLog
	{
#if DEBUG
		const string logdirectory = @"C:\xmllog\";//even IIS may make root directories!
		const string txtlog = "currentHttpApplication.log";  //

		static readonly string txtlogPath = Path.Combine(logdirectory, txtlog);

		static LocalLog()
		{
			System.IO.Directory.CreateDirectory(logdirectory);
			try
			{
				lock (txtlogPath) //crude synchronization to avoid file-locking issues
					using (var stream = File.Open(txtlogPath, FileMode.Create, FileAccess.Write))
						stream.Flush();//bla, no-op.
			}
			catch { }//logging should not cause an error
		}
#endif

		public static void SaveXmlLog(XElement document, string filename)
		{
#if DEBUG
			document.Save(Path.Combine(logdirectory, Path.GetFileName(filename)));
#endif
		}



		public static void DoLog(string msg)
		{
#if DEBUG
			StringBuilder fullmsg = new StringBuilder();
			fullmsg.Append(DateTime.Now);
			fullmsg.Append(", ");
			HttpContext c = HttpContext.Current;
			if (c != null)
			{
				try
				{
					fullmsg.Append("app[" + c.ApplicationInstance.GetHashCode() + "] ");
				}
				catch //generic catch-all exception handling used to permit logging as much as possible.
				{
					fullmsg.Append("<noApp> ");
				}

				fullmsg.Append("context[" + c.GetHashCode() + "] ");
				if (c.Handler != null)
					fullmsg.Append("handler[" + c.Handler.GetType().Name + "] ");

				try
				{
					if (c.Request != null)
						fullmsg.Append((c.Request.RawUrl ?? "<nullURL>") + " ");
				}
				catch //generic catch-all exception handling used to permit logging as much as possible.
				{
					fullmsg.Append("<noRequest> ");
				}
			}
			fullmsg.Append(msg ?? "<noMessage>");
			fullmsg.AppendLine();
			try
			{
				//don't use Debug.Write or Trace.Write because trace listener may be Finalized before the last message is pumped.
				lock (txtlogPath)//crude synchronization to avoid file-locking issues
					File.AppendAllText(txtlogPath, fullmsg.ToString());
			}
			catch { }//generic catch-all exception handling used to permit logging as much as possible.
#endif
		}
	}
}
