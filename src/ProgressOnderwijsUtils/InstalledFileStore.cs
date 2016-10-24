using System;
using System.IO;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public static class InstalledFileStore
    {
#if DEBUG
        const string PrivateBinPath = @"bin;bin\Debug";
#else
        const string PrivateBinPath = @"bin;bin\Release";
#endif

        public static string FileLocation(string name)
        {
            var searchPaths = new[] { string.Empty }
                .Concat((AppDomain.CurrentDomain.RelativeSearchPath ?? PrivateBinPath).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                .ToArray();
            foreach (var path in searchPaths) {
                var result = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path, name);
                if (File.Exists(result)) {
                    return result;
                }
            }

            throw new FileNotFoundException("file location not found", name);
        }
    }
}
