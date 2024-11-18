using System.Threading.Tasks;

namespace ProgressOnderwijsUtils;

public sealed class GitExe
{
    static int nextToken;
    public readonly Uri WorkingTree;
    readonly Func<Maybe<string, Unit>> hashCache;

    public GitExe(Uri workingTree)
    {
        WorkingTree = workingTree;
        hashCache = Utils.Lazy(CurrentCommit_uncached);
    }

    public AsyncProcessResult Git_MayFail(string arguments)
    {
        var logPrefix = "git|" + Interlocked.Increment(ref nextToken);
        Console.WriteLine(logPrefix + " " + WorkingTree.LocalPath + ">git " + arguments);
        var gitResult = Git_NoConsoleLog(arguments);
        gitResult.WriteToConsoleWithPrefix(logPrefix);
        return gitResult;
    }

    public AsyncProcessResult Git_NoConsoleLog(string arguments)
        => new ProcessStartSettings {
            ExecutableName = "git.exe",
            Arguments = arguments,
            WorkingDirectory = WorkingTree.LocalPath,
        }.StartProcess(CancellationToken.None);

    public AsyncProcessResult Git_AssertSuccess(string arguments)
    {
        var gitMayFail = Git_MayFail(arguments);
        var gitExitCode = gitMayFail.ExitCode.GetAwaiter().GetResult();
        if (gitExitCode != 0) {
            throw new($"git {arguments} failed: " + gitExitCode);
        }
        return gitMayFail;
    }

    public bool IsAncestorOf(string possibleAncestor, string aCommit)
        => Git_MayFail($"merge-base --is-ancestor \"{possibleAncestor}\" \"{aCommit}\"").ExitCode.GetAwaiter().GetResult() == 0;

    public Maybe<string, Unit> CurrentCommit_cached()
        => hashCache();

    public Maybe<string, Unit> CurrentCommit_uncached()
    {
        //avoid GIT_COMMIT jenkins env because ghprb is buggy.
        var result = Git_MayFail("rev-parse --verify HEAD");
        return Maybe.Either(result.ExitCode.GetAwaiter().GetResult() == 0, result.StdOutput().GetAwaiter().GetResult().JoinStrings("\n").Trim(), Unit.Value);
    }

    public async Task<bool> IsUpToDateWithOriginBranch(string sourceBranch)
        => await Git_MayFail($"fetch origin +refs/heads/{sourceBranch}:refs/remotes/origin/{sourceBranch} ").ExitCode == 0
            && await Git_MayFail($"merge-base --is-ancestor origin/{sourceBranch} HEAD").ExitCode == 0;

    public bool IsRefMergeable_AndResetWorkingCopy(string gitRef)
    {
        var isExistingPrMergeable = Git_MayFail($"merge --no-commit --no-ff \"{gitRef}\"").ExitCode.GetAwaiter().GetResult() == 0;
        _ = Git_AssertSuccess("reset --hard");
        return isExistingPrMergeable;
    }

    public sealed record TemporaryWorkTree(GitExe Parent, GitExe WorkTree) : IDisposable
    {
        public void Dispose()
        {
            _ = Parent.Git_AssertSuccess($"worktree remove -f \"{WorkTree.WorkingTree.LocalPath.TrimEnd(Path.DirectorySeparatorChar)}\"");
            WorkTree.WorkingTree.DeleteLocalFolderRecursivelyAndWait();
        }
    }

    public TemporaryWorkTree EnsureWorktreeForCommitExists(string commit, Uri temporaryBuildGitPath, string? branch = null)
    {
        temporaryBuildGitPath.DeleteLocalFolderRecursivelyAndWait();
        _ = Git_AssertSuccess($"worktree add -f {(branch is null ? "" : $"-B \"{branch}\" ")}\"{temporaryBuildGitPath.LocalPath.TrimEnd(Path.DirectorySeparatorChar)}\" \"{commit}\"");
        return new(this, new(temporaryBuildGitPath));
    }
}
