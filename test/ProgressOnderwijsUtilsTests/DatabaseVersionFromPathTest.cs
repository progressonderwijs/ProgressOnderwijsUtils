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
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"\\service1\Service\Ontwikkel-taken")) == DatabaseVersion.OntwikkelDB);
        }

        [Test]
        public void CanResolveTestServiceDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"\\service1\Service\Test-taken")) == DatabaseVersion.TestDB);
        }

        [Test]
        public void CanResolveNightlyServiceDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"\\service1\Service\Nightly-taken")) == DatabaseVersion.OntwikkelDB);
        }

        [Test]
        public void CanResolveAcceptatieGadgetsDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"C:\inetpub\Progress.NET\acceptatie-gadgets")) == DatabaseVersion.AcceptatieDB);
        }

        [Test]
        public void CanResolveAcceptatieGadgetsBinDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"C:\inetpub\Progress.NET\acceptatie-gadgets\bin\")) == DatabaseVersion.AcceptatieDB);
        }

        [Test]
        public void CanResolveProductieGadgetsDir()
        {
            PAssert.That(() => DatabaseVersionAuto.DatabaseVersionFromPath(new DirectoryInfo(@"C:\inetpub\Progress.NET\productie-gadgets")) == DatabaseVersion.ProductieDB);
        }
    }
}
