namespace ProgressOnderwijsUtils.Tests;

public sealed class WikiPageHtml5Approval
{
    [Fact]
    public void ApproveWikiHtml5Page()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(WikiPageHtml5.MakeHtml().ToStringWithDoctype());
}
