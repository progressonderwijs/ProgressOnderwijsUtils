using System;
using System.IO;
using System.Net;
using System.Web;

namespace ProgressOnderwijsUtils
{
	public static class DatabaseVersionAuto
	{
		static DirectoryInfo GetDeploymentDir()
		{
			var httpContext = HttpContext.Current;
			if(httpContext != null)
				return new DirectoryInfo(httpContext.Server.MapPath("."));
			else
				return
					new FileInfo(
						new Uri(
							typeof(ServerLocationAuto).Assembly
								.EscapedCodeBase
							).LocalPath
						).Directory;
		}

		public static DatabaseVersion ByDeploymentDirectory(DatabaseVersion developerFallbackDb)
		{
			DirectoryInfo dir = GetDeploymentDir();
			return DatabaseVersionFromPath(dir, developerFallbackDb);
		}

		public static DatabaseVersion DatabaseVersionFromPath(DirectoryInfo dir, DatabaseVersion developerFallbackDb)
		{
			while(dir != null)
			{
				var result = VersionIdentifiers.DatabaseVersionFromDirPrefix(dir.Name);
				if(result != null)
					return result.Value;
				dir = dir.Parent;
			}
			if(ServerLocationAuto.DeterminedByDns == ServerLocation.MachineInLAN && (Dns.GetHostName().StartsWith("PC") || RegisterTestingProgressTools.ShouldUseTestLocking))
				return developerFallbackDb;
			return DatabaseVersion.Undefined;
		}
	}
}