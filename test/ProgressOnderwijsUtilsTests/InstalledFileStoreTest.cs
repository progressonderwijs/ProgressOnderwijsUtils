using System.IO;
using NUnit.Framework;
using Progress.Test.CodeStyle;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class InstalledFileStoreTest
    {
        [Test]
        public void Onbestaande_file_kan_niet_gevonden_worden()
        {
            Assert.Catch<FileNotFoundException>(() => InstalledFileStore.FileLocation("ajshdfchvadf.gfs"));
        }

        [Test]
        public void Test_assembly_moet_gevonden_worden()
        {
            var assembly = ProgressAssemblies.TestAssembly.ManifestModule.Name;
            var location = InstalledFileStore.FileLocation(assembly);
        }
    }
}
