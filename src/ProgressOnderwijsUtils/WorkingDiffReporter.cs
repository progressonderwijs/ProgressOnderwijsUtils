using ApprovalTests.Core;
using ApprovalTests.Reporters;
/*
 * Workaround voor https://github.com/approvals/ApprovalTests.Net/issues/91
 * zie ook: https://github.com/PowerAssert/PowerAssert.Net/commit/2aa7b64808c6c7bdc01449795fb56362c95f0731
 * werkt omdat: visual studio diff reporter niet wordt gebruikt
 */
namespace ProgressOnderwijsUtils
{
    public class WorkinDiffReporter : FirstWorkingReporter
    {
        public WorkinDiffReporter()
            : base(
            (IEnvironmentAwareReporter)CodeCompareReporter.INSTANCE,
            (IEnvironmentAwareReporter)BeyondCompareReporter.INSTANCE,
            (IEnvironmentAwareReporter)TortoiseDiffReporter.INSTANCE,
            (IEnvironmentAwareReporter)AraxisMergeReporter.INSTANCE,
            (IEnvironmentAwareReporter)P4MergeReporter.INSTANCE,
            (IEnvironmentAwareReporter)WinMergeReporter.INSTANCE,
            (IEnvironmentAwareReporter)KDiffReporter.INSTANCE,
            (IEnvironmentAwareReporter)FrameworkAssertReporter.INSTANCE,
            (IEnvironmentAwareReporter)QuietReporter.INSTANCE)
        {
        }
    }
}