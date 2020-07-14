using System;
using System.IO;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class FileInfoExtensionTest : IDisposable
    {
        readonly FileInfo sut;
        readonly FileInfo other;

        public FileInfoExtensionTest()
        {
            sut = new FileInfo(Path.GetTempFileName());
            other = new FileInfo(Path.GetTempFileName());
        }

        public void Dispose()
        {
            try {
                sut.Delete();
            } finally {
                other.Delete();
            }
        }

        [Fact]
        public void SameContentsNull()
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            => Assert.ThrowsAny<ArgumentNullException>(() => sut.SameContents(null));

        [Fact]
        public void SameContentsSame()
        {
            PAssert.That(() => sut.SameContents(sut));
            PAssert.That(() => sut.SameContents(new FileInfo(sut.FullName)));
        }

        [Fact]
        public void SameContentsEmpty()
            => PAssert.That(() => sut.SameContents(other));

        [Fact]
        public void SameContentsFilled()
        {
            using (var fs1 = sut.OpenWrite()) {
                fs1.WriteByte(0);
            }
            using (var fs2 = other.OpenWrite()) {
                fs2.WriteByte(0);
            }
            PAssert.That(() => sut.SameContents(other));
        }

        [Fact]
        public void DifferentLength()
        {
            using (var fs1 = sut.OpenWrite()) {
                fs1.WriteByte(0);
                fs1.WriteByte(0);
            }
            using (var fs2 = other.OpenWrite()) {
                fs2.WriteByte(0);
            }
            PAssert.That(() => !sut.SameContents(other));
        }

        [Fact]
        public void ReadToEnd()
        {
            File.WriteAllText(sut.FullName, @"Hello World!");
            PAssert.That(() => sut.ReadToEnd() == @"Hello World!");
        }

        [Fact]
        public void DifferentContents()
        {
            using (var fs1 = sut.OpenWrite()) {
                fs1.WriteByte(0);
            }
            using (var fs2 = other.OpenWrite()) {
                fs2.WriteByte(1);
            }
            PAssert.That(() => !sut.SameContents(other));
        }
    }
}
