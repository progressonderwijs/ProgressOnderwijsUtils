using System.Net.Http;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils;

public static class UriExtensions
{
    public static Uri Combine(this Uri baseUri, string relativePath)
        => new(baseUri, relativePath);

    public static bool RefersToDirectory(this Uri uri)
        => uri.AbsolutePath.EndsWith("/", StringComparison.Ordinal);

    public static bool RefersToExistingLocalDirectory(this Uri uri)
        => uri.IsFile && uri.RefersToDirectory() && Directory.Exists(uri.LocalPath);

    public static bool RefersToExistingLocalFile(this Uri uri)
        => uri.IsFile && !uri.RefersToDirectory() && File.Exists(uri.LocalPath);

    public static Uri EnsureRefersToDirectory(this Uri uri)
        => uri.RefersToDirectory() ? uri : new UriBuilder(uri) { Path = $"{uri.AbsolutePath}/", }.Uri;

    public static Uri ReplaceRootWith(this Uri originalPath, Uri newRoot)
        => new(newRoot, originalPath.PathRelativeToRoot());

    public static Uri PathRelativeToRoot(this Uri path)
        => path.Combine("/").MakeRelativeUri(path);

    public static Uri SpecialFolderUri(this Environment.SpecialFolder folder)
        => new(Environment.GetFolderPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

    public static string WindowsFileShareFromUri(this Uri networkUri)
        => !networkUri.IsFile || string.IsNullOrEmpty(networkUri.Host)
            ? throw new InvalidOperationException($"The uri {networkUri} is not a windows file share")
            : networkUri.Combine("/").LocalPath.TrimEnd('\\');

    static readonly HttpClient client = new();

    public static async Task<bool> IsWorkingHttpUri(this Uri uri, CancellationToken cancel)
    {
        try {
            return (await client.SendAsync(new(HttpMethod.Head, uri), cancel)).IsSuccessStatusCode;
        } catch {
            return false;
        }
    }

    /// <summary>
    /// NTFS deletions can be async, unfortunately, and worse: they only are under certain corner cases and specific loads, so code that appears correct (by assuming deletions are synchronous) may only fail sometimes.
    /// This method deletes a folder, and waits until it's not found any longer.
    /// </summary>
    public static void DeleteLocalFolderRecursivelyAndWait(this Uri path)
    {
        var delayer = Stopwatch.StartNew();
        DeleteFolderRecursively(path);
        while (path.RefersToExistingLocalDirectory()) {
            if (delayer.Elapsed < TimeSpan.FromSeconds(10)) {
                Console.WriteLine($"Directory deletion did not succeed after {delayer.Elapsed.TotalSeconds:f2}s; will retry...");
            } else {
                throw new("directory deletion has not asynchronously completed after 10 seconds?");
            }
            DeleteFolderRecursively(path);
            Thread.Sleep(1); //NTFS can be async, e.g. if explorer is open.
        }
    }

    static void DeleteFolderRecursively(Uri path)
    {
        if (!path.RefersToDirectory()) {
            throw new ArgumentException("must pass a directory path (with trailing slash), not a file path (without trailing slash): " + path.LocalPath, nameof(path));
        }
        try {
            Directory.Delete(path.LocalPath, true);
        } catch {
            //ignore failures and try yet another hack
        }
        if (!path.RefersToExistingLocalDirectory()) {
            return;
            //Directory.Delete(path, true) can fail when there are readonly files, and the workaround is worse than this:https://ehikioya.com/delete-folder-with-read-only-contents-csharp/
        }

        _ = new ProcessStartSettings {
            ExecutableName = "cmd",
            Arguments = "/c rmdir \"" + path.LocalPath + "\" /s /q",
            WorkingDirectory = path.Combine("../").LocalPath,
        }.RunProcessWithoutRedirection();

        if (!path.RefersToExistingLocalDirectory()) {
            return;
            //rmdir can fail when when the path is longer than 260 chars.
        }

        var tempDirPath = new Uri(Path.GetTempPath()).EnsureRefersToDirectory().Combine(Path.GetRandomFileName());
        var tempDir = Directory.CreateDirectory(tempDirPath.LocalPath);
        try {
            _ = new ProcessStartSettings {
                ExecutableName = "robocopy",
                Arguments = $"/purge \"{tempDir.FullName.TrimEnd('\\')}\" \"{path.LocalPath.TrimEnd('\\')}\"",
                WorkingDirectory = path.Combine("../").LocalPath,
            }.RunProcessWithoutRedirection();
            Directory.Delete(path.LocalPath);
        } finally {
            tempDir.Delete();
        }
        //now we pray.
    }
}
