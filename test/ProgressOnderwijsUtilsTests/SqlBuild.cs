using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using Progress.Business;

namespace ProgressOnderwijsUtilsTests
{
    public class SqlBuild : TestSuiteBase
    {
        [Test]
        public void EenmaligeScripts()
        {
            var names = QueryBuilder.Create(@"
				select Naam
				from SqlBuild.EenmaligScript
				where DatumControle > DatumUitvoerProductie")
                .ReadPlain<string>(conn);

            Assert.That(names, Is.Empty, "Er zijn eenmalige scripts die op productie zijn uitgevoerd waarvan de scripts nog steeds draaien.");
        }
    }
}
