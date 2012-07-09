using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net.Layout;

namespace ProgressOnderwijsUtils.Log4Net
{
	public class RollingFileAppender : log4net.Appender.RollingFileAppender
	{
		private const string PATH = @"C:\inetpub\logs\Progress.NET";

		private static readonly IDictionary<DatabaseVersion, string> PATHS = new Dictionary<DatabaseVersion, string>
		{
			{ DatabaseVersion.Undefined, "." },
			{ DatabaseVersion.ProductieDB, "productie" },
			{ DatabaseVersion.TestDB, "test" },
			{ DatabaseVersion.DevTestDB, "ontwikkel" },
			{ DatabaseVersion.KetenTestDB, "ketentest" },
			{ DatabaseVersion.BronHODB, "bronho" },
		}; 

		public RollingFileAppender()
		{
			Encoding = Encoding.UTF8;
			RollingStyle = RollingMode.Size;
			AppendToFile = true;
			MaximumFileSize = "256MB";
			MaxSizeRollBackups = 9;
			Layout = new PatternLayout("%utcdate{ISO8601} [%-4thread] %-5level %-50.50logger: %message%newline");
		}

		public override string File
		{
			get
			{
				return base.File;
			}
			set
			{
				string path = PATH;
				if (Path.IsPathRooted(value))
				{
					path = Path.GetDirectoryName(value);
				}
				base.File = Path.GetFullPath(Path.Combine(path, Context, Path.GetFileName(value)));
			}
		}

		private string Context
		{
			get
			{
				return PATHS[DatabaseVersionAuto.ByDeploymentDirectory()];
			}
		}
	}
}
