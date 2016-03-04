using ExpressionToCodeLib;
using static ProgressOnderwijsUtils.FileData;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace Progress.Business.Test.Tools
{
    [Continuous]
    public class FileDataTest
    {
        [Test]
        public void KeepsShortNamesIntact()
        {
            PAssert.That(() => TrimNameToLength("test.file.name.txt", 20) == "test.file.name.txt");
        }

        [Test]
        public void ChopsOffParentPaths()
        {
            PAssert.That(() => TrimNameToLength(@"C:\dir\test.file.name.txt", 20) == "test.file.name.txt");
            PAssert.That(() => TrimNameToLength(@"/dir/test.file.name.txt", 60) == "test.file.name.txt");
            PAssert.That(() => TrimNameToLength(@"http://example.com/dir/test.file.name.txt", 30) == "test.file.name.txt");
        }

        [Test]
        public void OnlyRemovesAsMuchAsNecessary()
        {
            PAssert.That(() => TrimNameToLength(@"bla\123456789", 4) == "1234");
        }

        [Test]
        public void AvoidsAlteringExtension()
        {
            PAssert.That(() => TrimNameToLength(@"C:\dir\12345678901234567890.txt", 20) == "1234567890123456.txt");
        }

        [Test]
        public void DropsExtensionIfItsTooLong()
        {
            PAssert.That(() => TrimNameToLength(@"short.verylongextension", 9) == "short");
        }

        [Test]
        public void NeverCausesNewExtension_SECURITY()
        {
            PAssert.That(() => TrimNameToLength(@"short.exe.verylongextension", 9) == "short_exe");
        }
    }
}
