using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public sealed class ApprovalTest
    {
        public readonly string ApprovalPath;

        ApprovalTest(string approvalPath)
            => ApprovalPath = approvalPath;

        static string ToApprovalPath(SourceLocation sourceLocation)
        {
            var filename = Path.GetFileNameWithoutExtension(sourceLocation.FilePath);
            var filedir = Path.GetDirectoryName(sourceLocation.FilePath);
            var approvalPath = filedir + "\\" + filename + "." + sourceLocation.MemberName + ".approved.txt";
            return approvalPath;
        }

        public static ApprovalTest CreateHere([CallerLineNumber] int linenumber = -1, [CallerFilePath] string filepath = "", [CallerMemberName] string membername = "")
            => Create(new SourceLocation(membername, filepath, linenumber));

        public static ApprovalTest Create(SourceLocation sourceLocation)
            => CreateForApprovedPath(ToApprovalPath(sourceLocation));

        public static ApprovalTest CreateForApprovedPath(string path)
            => new ApprovalTest(path);

        public void AssertUnchangedAndSave(string[] line)
            => line.Select(l => l + "\r\n").JoinStrings();

        public void AssertUnchangedAndSave(string text)
        {
            if (UpdateIfChangedFrom(text)) {
                throw new Exception("Approval changed: " + Path.GetFileName(ApprovalPath));
            }
        }

        [Pure]
        public bool IsChangedFrom(string text)
            => !File.Exists(ApprovalPath) || File.ReadAllText(ApprovalPath, Encoding.UTF8) != text;

        [MustUseReturnValue]
        public bool UpdateIfChangedFrom(string text)
        {
            var isChangedFrom = IsChangedFrom(text);
            if (isChangedFrom) {
                File.WriteAllText(ApprovalPath, text, Encoding.UTF8);
            }
            return isChangedFrom;
        }
    }
}
