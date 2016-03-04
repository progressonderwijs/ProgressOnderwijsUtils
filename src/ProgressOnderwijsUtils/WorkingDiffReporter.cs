using ApprovalTests.Reporters;

/*
 Workaround voor https://github.com/approvals/ApprovalTests.Net/issues/91
 zie ook: https://github.com/PowerAssert/PowerAssert.Net/commit/2aa7b64808c6c7bdc01449795fb56362c95f0731
 werkt omdat: visual studio diff reporter niet wordt gebruikt
 */

namespace ProgressOnderwijsUtils
{
    [CodeThatsOnlyUsedForTests]
    public class WorkingDiffReporter : FirstWorkingReporter
    {
        public WorkingDiffReporter()
            : base(
                new GenericDiffReporter(@"C:\Program Files\TortoiseHg\lib\kdiff3.exe", "Please install KDIFF3"),
                new GenericDiffReporter(@"C:\Program Files\TortoiseHg\bin\kdiff3.exe", "Please install KDIFF3"),
                CodeCompareReporter.INSTANCE,
                BeyondCompareReporter.INSTANCE,
                TortoiseDiffReporter.INSTANCE,
                AraxisMergeReporter.INSTANCE,
                P4MergeReporter.INSTANCE,
                WinMergeReporter.INSTANCE,
                KDiffReporter.INSTANCE,
                FrameworkAssertReporter.INSTANCE,
                QuietReporter.INSTANCE) { }
    }
}
