using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class SelectItemListTest : TestsWithBusinessConnection
    {

        [Test]
        public void GetItemByValue() {
            var sut = Applicatie.KoppelTabel(conn, "land", RootOrganisatie.Dummy.ToInt(),Taal.NL, RootOrganisatie.Dummy.ToInt(), null);
            Assert.That(sut.GetItem((int)Land.Nederland).Value, Is.EqualTo((int?)(int)Land.Nederland));
        }

        [Test]
        public void GetItemByText() {
            var sut = Applicatie.KoppelTabel(conn, "land", RootOrganisatie.Dummy.ToInt(), Taal.NL, RootOrganisatie.Dummy.ToInt(), null);
            Assert.That(sut.GetItem(Taal.NL, "Nederland").Value, Is.EqualTo((int?)(int)Land.Nederland));
        }
    }
}
