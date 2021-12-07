namespace ProgressOnderwijsUtils.Tests;

public sealed class ApprovalTestTest
{
    [Fact]
    public void ApprovalWithLinesIsEquivalentToJoinedLines()
    {
        var approval = ApprovalTest.CreateHere();
        File.WriteAllText(approval.ApprovalPath, "test\r\nthis\r\n");
        Utils.TryWithCleanup(
            () => approval.AssertUnchangedAndSave(new[] { "test", "this", }),
            () => File.Delete(approval.ApprovalPath)
        );
    }

    [Fact]
    public void ApprovalWhenFileIsMissingThrows()
    {
        var approval = ApprovalTest.CreateHere();
        File.Delete(approval.ApprovalPath);
        Utils.TryWithCleanup(
            () => {
                PAssert.That(() => !File.Exists(approval.ApprovalPath));
                _ = Assert.ThrowsAny<Exception>(() => approval.AssertUnchangedAndSave("bla"));
                PAssert.That(() => File.Exists(approval.ApprovalPath));
            },
            () => File.Delete(approval.ApprovalPath)
        );
    }

    [Fact]
    public void ApprovalWhenFileIsDifferentThrows()
    {
        var approval = ApprovalTest.CreateHere();
        File.WriteAllText(approval.ApprovalPath, "hello");
        Utils.TryWithCleanup(
            () => {
                _ = Assert.ThrowsAny<Exception>(() => approval.AssertUnchangedAndSave("bla"));
                PAssert.That(() => File.Exists(approval.ApprovalPath));
                PAssert.That(() => File.ReadAllText(approval.ApprovalPath) == "bla");
            },
            () => File.Delete(approval.ApprovalPath)
        );
    }

    [Fact]
    public void ApprovalWhenFileIsSameDoesNotThrow()
    {
        var approval = ApprovalTest.CreateHere();
        File.WriteAllText(approval.ApprovalPath, "hello");
        Utils.TryWithCleanup(
            () => {
                approval.AssertUnchangedAndSave("hello");
                PAssert.That(() => File.Exists(approval.ApprovalPath));
                PAssert.That(() => File.ReadAllText(approval.ApprovalPath) == "hello");
            },
            () => File.Delete(approval.ApprovalPath)
        );
    }
}