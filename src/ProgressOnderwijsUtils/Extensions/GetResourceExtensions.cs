using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public static class GetResourceExtensions
    {
        public static Stream GetResource(this Type type, string filename) { return type.Assembly.GetManifestResourceStream(type, filename); }
    }
}
