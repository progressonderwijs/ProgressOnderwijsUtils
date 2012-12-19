using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
    <Exec Command="hg id -i &gt; ProgressVersion.Info.Generated &amp;&amp; hg log -r . --template &quot;branches:{branches}\ntags:{tags}\ndate:{date|isodate}&quot; &gt;&gt; ProgressVersion.Info.Generated " />
    <WriteLinesToFile File="ProgressVersion.Info.Generated" Lines="ComputerName:$(COMPUTERNAME)" Overwrite="false" Encoding="UTF-8" />
    <WriteLinesToFile File="ProgressVersion.Info.Generated" Lines="JobName:$(JOB_NAME)" Overwrite="false" Encoding="UTF-8" />
    <WriteLinesToFile File="ProgressVersion.Info.Generated" Lines="BuildId:$(BUILD_ID)" Overwrite="false" Encoding="UTF-8" />
  </Target>
*/
		[Serializable]
		public sealed class Data : IMetaObject
		{
			public string Node { get; set; }
			public DateTime Date { get; set; }
			public string Branches { get; set; }
			public string Tags { get; set; }
			
			public string ComputerName { get; set; }
			public string JobName { get; set; }
			public string BuildId { get; set; }
			//public DateTime WcDateUtc { get; set; }
		}
		public static readonly Data Current;


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

			if (lines == null)
			{
				Current = new Data { Node = "unknown!" };
				return;
			}

			var nodeId = lines.First().Trim();
			var svninfo = lines.Skip(1).Select(s => s.Trim()).Where(s => s.Length > 0).ToDictionary(s => s.Substring(0, s.IndexOf(':')), s => s.Substring(s.IndexOf(':') + 1));

			Current = new Data {
				Node = nodeId,
				Date = DateTime.Parse(svninfo["date"], CultureInfo.InvariantCulture),
				Branches = svninfo["branches"],
				Tags = svninfo["tags"],

				ComputerName = svninfo["ComputerName"].Trim(),
				JobName = svninfo["JobName"].Trim(),
				BuildId = svninfo["BuildId"].Trim(),
			};
		}
	}
}
