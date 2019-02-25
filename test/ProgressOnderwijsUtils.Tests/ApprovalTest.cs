using System.IO;
using System.Runtime.CompilerServices;
using ApprovalTests.Approvers;
using ApprovalTests.Core;
using ApprovalTests.Reporters;
using ApprovalTests.Writers;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Tests
{
    static class ApprovalTest
    {
        sealed class SaneNamer : IApprovalNamer
        {
            public string SourcePath { get; set; }
            public string Name { get; set; }
        }

        public static void Verify(string text, [CanBeNull] object IGNORE_PAST_THIS = null, [CanBeNull] [CallerFilePath] string filepath = null, [CanBeNull] [CallerMemberName] string membername = null)
        {
            var writer = WriterFactory.CreateTextWriter(text);
            var filename = Path.GetFileNameWithoutExtension(filepath);
            var filedir = Path.GetDirectoryName(filepath);
            var namer = new SaneNamer { Name = filename + "." + membername, SourcePath = filedir };
            var reporter = new AllFailingTestsClipboardReporter();
            Approver.Verify(new FileApprover(writer, namer, true), reporter);
        }
    }
}
