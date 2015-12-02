using System;
using System.IO;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class FileInfoExtensionTest
    {
        FileInfo sut;
        FileInfo other;

        [SetUp]
        public void SetUp()
        {
            sut = new FileInfo(Path.GetTempFileName());
            other = new FileInfo(Path.GetTempFileName());
        }

        [TearDown]
        public void TearDown()
        {
            try {
                sut.Delete();
            } finally {
                other.Delete();
            }
        }

        [Test]
        public void SameContentsNull()
        {
            Assert.Catch<ArgumentNullException>(() => { var ignore = sut.SameContents(null); });
        }

        [Test]
        public void SameContentsSame()
        {
            Assert.That(sut.SameContents(sut));
            Assert.That(sut.SameContents(new FileInfo(sut.FullName)));
        }

        [Test]
        public void SameContentsEmpty()
        {
            Assert.That(sut.SameContents(other));
        }

        [Test]
        public void SameContentsFilled()
        {
            using (FileStream fs1 = sut.OpenWrite())
                fs1.WriteByte(0);
            using (FileStream fs2 = other.OpenWrite())
                fs2.WriteByte(0);
            Assert.That(sut.SameContents(other));
        }

        [Test]
        public void DifferentLength()
        {
            using (FileStream fs1 = sut.OpenWrite()) {
                fs1.WriteByte(0);
                fs1.WriteByte(0);
            }
            using (FileStream fs2 = other.OpenWrite())
                fs2.WriteByte(0);
            PAssert.That(() => !sut.SameContents(other));
        }

        [Test]
        public void ReadToEnd()
        {
            File.WriteAllText(sut.FullName, @"Hello World!");
            PAssert.That(() => sut.ReadToEnd() == @"Hello World!");
        }

        [Test]
        public void DifferentContents()
        {
            using (FileStream fs1 = sut.OpenWrite())
                fs1.WriteByte(0);
            using (FileStream fs2 = other.OpenWrite())
                fs2.WriteByte(1);
            Assert.That(!sut.SameContents(other));
        }
    }
}
