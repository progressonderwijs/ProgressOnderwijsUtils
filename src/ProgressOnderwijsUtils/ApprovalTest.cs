namespace ProgressOnderwijsUtils;

public sealed class ApprovalTest
{
    public readonly string ApprovalPath;

    ApprovalTest(string approvalPath)
        => ApprovalPath = approvalPath;

    static string ToApprovalPath(SourceLocation sourceLocation)
    {
        var filename = Path.GetFileNameWithoutExtension(sourceLocation.FilePath);
        var filedir = Path.GetDirectoryName(sourceLocation.FilePath);
        var approvalPath = $"{filedir}\\{filename}.{sourceLocation.MemberName}.approved.txt";
        return approvalPath;
    }

    public static ApprovalTest CreateHere([CallerLineNumber] int linenumber = -1, [CallerFilePath] string filepath = "", [CallerMemberName] string membername = "")
        => Create(new(membername, filepath, linenumber));

    public static ApprovalTest Create(SourceLocation sourceLocation)
        => CreateForApprovedPath(ToApprovalPath(sourceLocation));

    public static ApprovalTest CreateForApprovedPath(string path)
        => new(path);

    public void AssertUnchangedAndSave(string[] lines)
        => AssertUnchangedAndSave(lines.Select(line => $"{line}\n").JoinStrings());

    public void AssertUnchangedAndSave(string text)
    {
        if (UpdateIfChangedFrom(text, out var diff)) {
            throw new($"Approval {Path.GetFileName(ApprovalPath)} changed: {diff}");
        }
    }

    [Pure]
    public bool IsChangedFrom(string text, out string diff)
    {
        if (File.Exists(ApprovalPath)) {
            var approved = File.ReadAllText(ApprovalPath);
            if (approved != text) {
                var x = approved.Zip(text).SkipWhile(c => c.First == c.Second).Take(50).ToArray();
                diff = $"'{new(x.ArraySelect(d => d.First))}' »» '{new(x.ArraySelect(d => d.Second))}'";
                return true;
            } else {
                diff = "";
                return false;
            }
        } else {
            diff = "was not yet approved";
            return true;
        }
    }

    [MustUseReturnValue]
    public bool UpdateIfChangedFrom(string text, out string diff)
    {
        var isChangedFrom = IsChangedFrom(text, out diff);
        if (isChangedFrom) {
            File.WriteAllText(ApprovalPath, text);
        }
        return isChangedFrom;
    }
}
