#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProgressOnderwijsUtils
{
    public struct SourceLocation
    {
        public readonly string MemberName;
        public readonly string FilePath;
        public readonly int LineNumber;

        public SourceLocation(string memberName, string filePath, int lineNumber)
        {
            MemberName = memberName;
            FilePath = filePath;
            LineNumber = lineNumber;
        }

        [Pure]
        public static SourceLocation Here([CallerLineNumber] int linenumber = -1, [CallerFilePath] string filepath = null, [CallerMemberName] string membername = null)
            => new SourceLocation(membername, filepath, linenumber);
    }
}
