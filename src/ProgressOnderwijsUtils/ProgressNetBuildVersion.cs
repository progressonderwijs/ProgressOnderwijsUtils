using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class ProgressNetBuildVersion
	{

		public static readonly string WCREV, WCRANGE, ComputerName, JobName, BuildId;

		public static readonly DateTime WCNOWUTC, WCDATEUTC;
		public static readonly bool WCMODS;


		static ProgressNetBuildVersion()
		{
			var svninfo = GetResource("ProgressOnderwijsUtils.Properties.SubWCRev.Info.Generated").Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToDictionary(s => s.Substring(0, s.IndexOf(':')), s => s.Substring(s.IndexOf(':') + 1));
			WCREV = svninfo["WCREV"];
			WCRANGE = svninfo["WCRANGE"];
			WCNOWUTC = DateTime.Parse(svninfo["WCNOWUTC"], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			WCDATEUTC = DateTime.Parse(svninfo["WCDATEUTC"], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			WCMODS = bool.Parse(svninfo["WCMODS"]);

			ComputerName = GetResource("ProgressOnderwijsUtils.Properties.Environment.ComputerName.Generated").Trim();
			JobName = GetResource("ProgressOnderwijsUtils.Properties.Environment.JobName.Generated").Trim().Substring("Job:".Length);
			BuildId = GetResource("ProgressOnderwijsUtils.Properties.Environment.BuildId.Generated").Trim().Substring("BuildId:".Length);
		}

		static string GetResource(string resourceName)
		{
			using (var stream = typeof(ProgressNetBuildVersion).Assembly.GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}
	}
}
