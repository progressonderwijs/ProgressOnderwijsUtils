using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProgressOnderwijsUtils
{
	public class SvnUtils
	{
		[XmlRoot("log")]
		public sealed class SvnLog : XmlSerializableBase<SvnLog>
		{
			[XmlElement("logentry")]
			public SvnLogEntry[] logentry;
		}
		public sealed class SvnLogEntry
		{
			[XmlAttribute]
			public int revision;
			public DateTime date;
			public string author;
			public string msg;
			public string ToShortString() { return author + "@" + date.ToShortDateString() + ": " + StringMeasurement.LimitDisplayLength(msg.Replace("\n", " "), 80); }
		}


		public static IEnumerable<SvnLogEntry> LastLogMessages(string path, int? limit = null)
		{
			var svnlogExecResult = WinProcessUtil.ExecuteProcessSynchronously("svn.exe", "log -l " + (limit ?? 3) + " --xml \"" + path + "\"", null);
			try
			{
				return SvnLog.Deserialize(XDocument.Parse(svnlogExecResult.StandardOutputContents)).logentry;
			}
			catch (XmlException xe)
			{
				throw new ProgressNetException(path, xe);
				//return Enumerable.Empty<SvnLogEntry>(); 
			}
		}

		public sealed class SvnInfoEntry : XmlSerializableBase<SvnInfoEntry>
		{
			[XmlAttribute]
			public string kind;
			[XmlAttribute]
			public string path;
			[XmlAttribute]
			public int revision;

			public Uri url;

			public SvnCommitInfo commit;
		}

		public sealed class SvnCommitInfo
		{
			[XmlAttribute]
			public int revision;
			[XmlAttribute]
			public string author;
			[XmlAttribute]
			public DateTime date;
		}

		public static SvnInfoEntry Info(string path)
		{

			var svninfoExecResult = WinProcessUtil.ExecuteProcessSynchronously("svn.exe", "info --xml \"" + path + "\"", null);
			XDocument svninfoOutput = XDocument.Parse(svninfoExecResult.StandardOutputContents);
			var entryElement = svninfoOutput.Element("info").Element("entry");
			return SvnInfoEntry.Deserialize(entryElement);
		}
	}
}
