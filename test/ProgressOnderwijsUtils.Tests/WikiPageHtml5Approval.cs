using System.Linq;
using Xunit;
using ProgressOnderwijsUtils.Html;

namespace ProgressOnderwijsUtils.Tests
{
    public class WikiPageHtml5Approval
    {
        [Fact]
        public void ApproveWikiHtml5Page()
        {
            ApprovalTest.Verify(WikiPageHtml5.MakeHtml().SerializeToString());
        }
    }
}
