using System;
using System.IO;

namespace ProgressOnderwijsUtils
{
    public static class UriExtensions
    {
        public static Uri Combine(this Uri baseUri, string relativePath)
            => new Uri(baseUri, relativePath);

        public static bool RefersToDirectory(this Uri uri)
            => uri.AbsolutePath.EndsWith("/", StringComparison.Ordinal);

        public static bool RefersToExistingLocalDirectory(this Uri uri)
            => uri.IsFile && uri.RefersToDirectory() && Directory.Exists(uri.LocalPath);

        public static bool RefersToExistingLocalFile(this Uri uri)
            => uri.IsFile && !uri.RefersToDirectory() && File.Exists(uri.LocalPath);

        public static Uri EnsureRefersToDirectory(this Uri uri)
            => uri.RefersToDirectory() ? uri : new UriBuilder(uri) { Path = uri.AbsolutePath + "/" }.Uri;

        public static Uri ReplaceRootWith(this Uri originalPath, Uri newRoot)
            => new Uri(newRoot, originalPath.PathRelativeToRoot());

        public static Uri PathRelativeToRoot(this Uri path)
            => path.Combine("/").MakeRelativeUri(path);

        public static Uri SpecialFolderUri(this Environment.SpecialFolder folder)
            => new Uri(Environment.GetFolderPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

        public static string WindowsFileShareFromUri(this Uri networkUri)
            => !networkUri.IsFile || string.IsNullOrEmpty(networkUri.Host)
                ? throw new InvalidOperationException("The uri " + networkUri + " is not a windows file share")
                : networkUri.Combine("/").LocalPath.TrimEnd('\\');
    }
}
