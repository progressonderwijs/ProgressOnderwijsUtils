using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.GenericEdit;
using Progress.Business.Test;

namespace ProgressOnderwijsUtilsTests
{
    [PullRequestTest]
    public class FileDataTest
    {
        [Test]
        public void KeepsShortNamesIntact()
        {
            PAssert.That(() => FileData.TrimNameToLength("test.file.name.txt", 20) == "test.file.name.txt");
        }

        [Test]
        public void ChopsOffParentPaths()
        {
            PAssert.That(() => FileData.TrimNameToLength(@"C:\dir\test.file.name.txt", 20) == "test.file.name.txt");
            PAssert.That(() => FileData.TrimNameToLength(@"/dir/test.file.name.txt", 60) == "test.file.name.txt");
            PAssert.That(() => FileData.TrimNameToLength(@"http://example.com/dir/test.file.name.txt", 30) == "test.file.name.txt");
        }

        [Test]
        public void OnlyRemovesAsMuchAsNecessary()
        {
            PAssert.That(() => FileData.TrimNameToLength(@"bla\123456789", 4) == "1234");
        }

        [Test]
        public void AvoidsAlteringExtension()
        {
            PAssert.That(() => FileData.TrimNameToLength(@"C:\dir\12345678901234567890.txt", 20) == "1234567890123456.txt");
        }

        [Test]
        public void DropsExtensionIfItsTooLong()
        {
            PAssert.That(() => FileData.TrimNameToLength(@"short.verylongextension", 9) == "short");
        }

        [Test]
        public void NeverCausesNewExtension_SECURITY()
        {
            PAssert.That(() => FileData.TrimNameToLength(@"short.exe.verylongextension", 9) == "short_exe");
        }
    }
}
