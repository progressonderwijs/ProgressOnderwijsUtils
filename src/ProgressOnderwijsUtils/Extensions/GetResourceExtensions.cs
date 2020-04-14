using System;
using System.IO;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class GetResourceExtensions
    {
        [Pure]
        public static Stream? GetResource(this Type type, string filename)
            => type.Assembly.GetManifestResourceStream(type, filename);
    }
}
