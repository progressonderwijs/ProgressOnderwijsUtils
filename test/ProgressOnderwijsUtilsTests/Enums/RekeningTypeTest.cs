using NUnit.Framework;
using Progress.Business.Studenten.Financieel;

namespace ProgressOnderwijsUtilsTests.Enums
{
    class RekeningTypeTest
    {
        [Test]
        public void Bankrekeningnummers()
        {
            Assert.That(RekeningTypeHelper.Rekeningtype(null), Is.EqualTo(RekeningType.Onbekend));
            Assert.That(RekeningTypeHelper.Rekeningtype(""), Is.EqualTo(RekeningType.Onbekend));
            Assert.That(RekeningTypeHelper.Rekeningtype("123"), Is.EqualTo(RekeningType.Giro));
            Assert.That(RekeningTypeHelper.Rekeningtype("150864167"), Is.EqualTo(RekeningType.Bank));
            Assert.That(RekeningTypeHelper.Rekeningtype("150864168"), Is.EqualTo(RekeningType.Onbekend));
            Assert.That(RekeningTypeHelper.Rekeningtype(123), Is.EqualTo(RekeningType.Giro));
            Assert.That(RekeningTypeHelper.Rekeningtype(150864167), Is.EqualTo(RekeningType.Bank));
            Assert.That(RekeningTypeHelper.Rekeningtype(150864168), Is.EqualTo(RekeningType.Onbekend));
        }
    }
}
