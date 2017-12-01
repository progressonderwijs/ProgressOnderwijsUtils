using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ProgressOnderwijsUtils.Html;

namespace ProgressOnderwijsUtilsTests
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
