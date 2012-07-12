using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace ProgressOnderwijsUtils
{

	/// <summary>
	/// Om dit te laten werken moet een build scriptje in de EXECUTING assembly toegevoegd worden!  (zie code)
	/// </summary>
	public static class ProgressNetBuildVersion
	{
		/*  Nodig build scriptje ergens in de applicatie:
  <Target Name="AfterBuild">
    <Exec Command="&quot;$(SolutionDir)binary-tooling\svn-tools\SubWCRev.exe&quot; &quot;$(SolutionDir).&quot; &quot;$(ProjectDir)SubWCRev.Info.template&quot; &quot;$(ProjectDir)ProgressVersion.Info.Generated&quot;" />
    <WriteLinesToFile File="ProgressVersion.Info.Generated" Lines="ComputerName:$(COMPUTERNAME)" Overwrite="false" Encoding="UTF-8" />
    <WriteLinesToFile File="ProgressVersion.Info.Generated" Lines="JobName:$(JOB_NAME)" Overwrite="false" Encoding="UTF-8" />
    <WriteLinesToFile File="ProgressVersion.Info.Generated" Lines="BuildId:$(BUILD_ID)" Overwrite="false" Encoding="UTF-8" />
  </Target>
*/

		public static readonly string WCREV, WCRANGE, ComputerName, JobName, BuildId;

		public static readonly DateTime WCNOWUTC, WCDATEUTC;
		public static readonly bool WCMODS;

		static ProgressNetBuildVersion()
		{

			var dirGenerators = new Func<string>[] { () => HttpRuntime.AppDomainAppPath, () => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), () => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) };
			var dirs = dirGenerators.SelectMany(dirGen => { try { return new[] { dirGen() }; } catch { return new string[0]; } });

			var lines =
				Utils.TransitiveClosure( //find all ancestor paths
					dirs,
					dir => { try { if (Directory.Exists(dir))return new[] { Path.GetDirectoryName(dir) }; } catch { } return new string[0]; })
				.SelectMany(path => { //find any versioninfo files in ancestor paths
					try
					{
						var revInfoFile = Path.Combine(path, "ProgressVersion.Info.Generated");
						if (File.Exists(revInfoFile))
							return new[] { revInfoFile };
					}
					catch { }
					return new string[0];
				})
				.OrderByDescending(File.GetLastWriteTimeUtc) //try newest one first
				.Select(dir => {
					try
					{
						var revInfoFile = Path.Combine(dir, "ProgressVersion.Info.Generated");
						if (File.Exists(revInfoFile))
							return File.ReadAllLines(revInfoFile);
					}
					catch { }
					return null;
				}).FirstOrDefault(revInfoLines => revInfoLines != null);

			if (lines == null) return;
			var svninfo = lines.Select(s => s.Trim()).Where(s => s.Length > 0).ToDictionary(s => s.Substring(0, s.IndexOf(':')), s => s.Substring(s.IndexOf(':') + 1));
			WCREV = svninfo["WCREV"];
			WCRANGE = svninfo["WCRANGE"];
			WCNOWUTC = DateTime.Parse(svninfo["WCNOWUTC"], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			WCDATEUTC = DateTime.Parse(svninfo["WCDATEUTC"], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			WCMODS = bool.Parse(svninfo["WCMODS"]);

			ComputerName = svninfo["ComputerName"].Trim();
			JobName = svninfo["JobName"].Trim();
			BuildId = svninfo["BuildId"].Trim();
		}

		static string GetResource(string resourceName)
		{
			using (var stream = typeof(ProgressNetBuildVersion).Assembly.GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}
	}
}
