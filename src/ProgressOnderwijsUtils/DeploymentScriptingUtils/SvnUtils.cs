using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProgressOnderwijsUtils.DeploymentScriptingUtils
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
            try {
                return SvnLog.Deserialize(XDocument.Parse(svnlogExecResult.StandardOutputContents)).logentry;
            } catch (XmlException xe) {
                throw new ProgressNetException(path, xe);
                //return Enumerable.Empty<SvnLogEntry>(); 
            }
        }

        [XmlRoot("info")]
        public sealed class SvnInfo : XmlSerializableBase<SvnInfo>
        {
            public SvnInfoEntry entry;
        }

        [XmlRoot("entry")]
        public sealed class SvnInfoEntry : XmlSerializableBase<SvnInfoEntry>
        {
            [XmlAttribute]
            public string kind;

            [XmlAttribute]
            public string path;

            [XmlAttribute]
            public int revision;

            public string url;
            public SvnCommitInfo commit;
        }

        public sealed class SvnCommitInfo
        {
            [XmlAttribute]
            public int revision;

            public string author;
            public DateTime date;
        }

        public static SvnInfoEntry Info(string path)
        {
            var svninfoExecResult = WinProcessUtil.ExecuteProcessSynchronously("svn.exe", "info --xml \"" + path + "\"", null);
            XDocument svninfoOutput = XDocument.Parse(svninfoExecResult.StandardOutputContents);
            return SvnInfo.Deserialize(svninfoOutput).entry;
        }
    }
}
