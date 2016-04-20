using System.IO;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.AppVersion;
using Progress.Business.Data;

namespace ProgressOnderwijsUtilsTests
{
    class DatabaseVersionFromPathTest
    {
        [Test]
        public void CanResolveOntwikkelServiceDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"\\service1\Service\Ontwikkel-taken")) == PnetOmgeving.Ontwikkel);
        }

        [Test]
        public void CanResolveTestServiceDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"\\service1\Service\Test-taken")) == PnetOmgeving.Test);
        }

        [Test]
        public void CanResolveNightlyServiceDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"\\service1\Service\Nightly-taken")) == PnetOmgeving.Ontwikkel);
        }

        [Test]
        public void CanResolveAcceptatieGadgetsDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"C:\inetpub\Progress.NET\acceptatie-gadgets")) == PnetOmgeving.Acceptatie);
        }

        [Test]
        public void CanResolveAcceptatieGadgetsBinDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"C:\inetpub\Progress.NET\acceptatie-gadgets\bin\")) == PnetOmgeving.Acceptatie);
        }

        [Test]
        public void CanResolveProductieGadgetsDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"C:\inetpub\Progress.NET\productie-gadgets")) == PnetOmgeving.Productie);
        }
    }
}
