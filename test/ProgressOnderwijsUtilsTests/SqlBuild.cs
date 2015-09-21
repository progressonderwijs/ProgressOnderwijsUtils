using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using Progress.Business;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsTests
{
    public class SqlBuild : TestSuiteBase
    {
        [Test]
        public void EenmaligeScripts()
        {
            var names = SQL($@"
				select Naam
				from SqlBuild.EenmaligScript
				where DatumControle > DatumUitvoerProductie")
                .ReadPlain<string>(conn);

            Assert.That(names, Is.Empty, "Er zijn eenmalige scripts die op productie zijn uitgevoerd waarvan de scripts nog steeds draaien.");
        }
    }
}
