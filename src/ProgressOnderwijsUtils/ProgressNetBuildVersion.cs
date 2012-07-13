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
			var assemblies = new[] { Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly() }.Where(ass => ass != null);
			var dirs = new[] { HttpRuntime.AppDomainAppPath }.Concat(assemblies.Select(ass => ass.Location));

			var dirsWithAncestors = Utils.TransitiveClosure(dirs, currentDirSet => currentDirSet.Where(Directory.Exists).Select(Path.GetDirectoryName));//find all ancestor paths
			var lines =
				dirsWithAncestors
				.Where(Directory.Exists)
					.Select(dir => Path.Combine(dir, "ProgressVersion.Info.Generated"))
					.Where(File.Exists)
					.OrderByDescending(File.GetLastWriteTimeUtc) //try newest one first
					.Select(File.ReadAllLines)
					.FirstOrDefault();

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
	}
}
