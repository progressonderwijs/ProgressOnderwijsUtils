using NUnit.Framework;
using Progress.Business;
using Progress.Business.DomainUnits;
using Progress.Business.Test;
using Progress.Business.Text;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class SelectItemListTest : TestsWithBusinessConnection
    {
        [Test]
        public void GetItemByValue()
        {
            var sut = Applicatie.KoppelTabel(conn, KoppelTabel.landmetdata, RootOrganisatie.Dummy.ToOrganisatieId(), Taal.NL, RootOrganisatie.Dummy);
            Assert.That(sut.GetItem((int)Land.Nederland).Value, Is.EqualTo((int?)(int)Land.Nederland));
        }

        [Test]
        public void GetItemByText()
        {
            var sut = Applicatie.KoppelTabel(conn, KoppelTabel.landmetdata, RootOrganisatie.Dummy.ToOrganisatieId(), Taal.NL, RootOrganisatie.Dummy);
            Assert.That(sut.GetItem(Taal.NL, "Nederland").Value, Is.EqualTo((int?)(int)Land.Nederland));
        }
    }
}
