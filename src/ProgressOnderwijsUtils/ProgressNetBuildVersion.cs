using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public sealed class ProgressBuildInfo : IMetaObject
    {
        public string CommitHash { get; set; }
        public DateTime? UtcDate { get; set; }
        public string Branch { get; set; }
        public string ComputerName { get; set; }
        public string BuildTag { get; set; }
        public string BuildUrl { get; set; }
        public string BuildConfiguration { get; set; }
    }

    /// <summary>
    /// Om dit te laten werken moet een build scriptje in de EXECUTING assembly toegevoegd worden!  (zie code)
    /// </summary>
    public static class ProgressNetBuildVersion
    {
        public static readonly ProgressBuildInfo Current = DetermineProgressBuildInfo();

        static ProgressBuildInfo DetermineProgressBuildInfo()
        {
            var assemblies =
                new[] { Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly() }.Where(ass => ass != null);
            var httpRuntimePath = default(string);
            // ReSharper disable once EmptyGeneralCatchClause
            try {
                httpRuntimePath = HttpRuntime.AppDomainAppPath;
            } catch { }

            var dirs = new[] { httpRuntimePath }.Concat(assemblies.Select(ass => ass.Location));

            var dirsWithAncestors = Utils.TransitiveClosure(
                dirs,
                currentDirSet => currentDirSet.Where(Directory.Exists).Select(Path.GetDirectoryName));
            //find all ancestor paths
            var progressVersionInfoFilePath = dirsWithAncestors
                .Where(Directory.Exists)
                .Select(dir => Path.Combine(dir, "ProgressVersion.Info"))
                .Where(File.Exists)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (progressVersionInfoFilePath == null) {
                return new ProgressBuildInfo { CommitHash = "unknown" };
            } else {
                return XmlSerializerHelper.Deserialize<ProgressBuildInfo>(File.ReadAllText(progressVersionInfoFilePath, Encoding.UTF8));
            }
        }
    }
}
