using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using log4net.Layout;

namespace ProgressOnderwijsUtils.Log4Net
{

	[UsedImplicitly]//dynamically loaded as per log4net.config
	public sealed class RollingFileAppender : log4net.Appender.RollingFileAppender 
		//TODO:this isn't best practice code; the logging config is hardcoded and deployment dependant.
	{
		const string PATH = @"C:\inetpub\logs\Progress.NET";

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

		static string Context
		{
			get
			{
				var byDeploymentDirectory = ApplicationVersionAuto.ByDeploymentDirectory();
				return byDeploymentDirectory == null ? "." : byDeploymentDirectory.Value.ToString();
			}
		}
	}
}
