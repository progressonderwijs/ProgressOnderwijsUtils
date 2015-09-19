using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;
using Progress.WebFramework.Internal;

namespace ProgressOnderwijsUtilsTests
{
    class TempTextFileTest : AbstractCachedData<string>
    {
        public readonly FileInfo file;

        public TempTextFileTest(FileInfo tempfile)
            : base(tempfile.Directory, tempfile.Name, true)
        {
            file = tempfile;
        }

        public TempTextFileTest()
            : this(new FileInfo(Path.GetTempFileName())) { }

        protected override string Load()
        {
            file.Refresh();
            return file.Exists ? File.ReadAllText(file.FullName) : null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (file != null) {
                file.Delete();
            }
        }
    }

    public class AbstractCachedDataTest
    {
        [Test]
        public void PlainWorks()
        {
            FileInfo oldfile;
            using (var t = new TempTextFileTest()) {
                oldfile = new FileInfo(t.file.FullName);
                PAssert.That(() => oldfile.Exists && t.Data == "");

                PAssert.That(
                    () => t.WatchedFilesAsRelativeUris(oldfile.Directory.Parent).Single().ToString()
                        .StartsWith(oldfile.Directory.Name + "/"));

                PAssert.That(() => t.LastWriteTimeUtc <= DateTime.UtcNow && t.LastWriteTimeUtc >= DateTime.UtcNow - TimeSpan.FromMinutes(1.0));
            }
            oldfile.Refresh();
            PAssert.That(() => !oldfile.Exists);
        }

        [Test]
        public void NeedsRealDir()
        {
            Assert.Throws<ArgumentException>(() => { using (var t = new TempTextFileTest(new FileInfo(@"A:\b\c\d\e"))) { } });
        }

        [Test]
        public void DeleteRecreateWorks()
        {
            FileInfo oldfile;
            using (var t = new TempTextFileTest()) {
                oldfile = new FileInfo(t.file.FullName);
                oldfile.Delete();
                PAssert.That(() => !oldfile.Exists && t.Data == null);
                File.WriteAllText(oldfile.FullName, "Hello World!");
                var sw = Stopwatch.StartNew();
                while (t.Data == null && sw.ElapsedMilliseconds < 500) {
                    Thread.Sleep(20);
                }
                PAssert.That(() => t.Data == "Hello World!");
                oldfile.Refresh();
                PAssert.That(() => oldfile.Exists);
            }
            oldfile.Refresh();
            PAssert.That(() => !oldfile.Exists);
        }

        [Test]
        public void ModifyWorks()
        {
            using (var t = new TempTextFileTest()) {
                PAssert.That(() => t.file.Exists && t.Data == "");
                File.WriteAllText(t.file.FullName, "1, 2, 3, 4");
                var sw = Stopwatch.StartNew();
                while (t.Data == "" && sw.ElapsedMilliseconds < 500) {
                    Thread.Sleep(20);
                }
                PAssert.That(() => t.file.Exists && t.Data == "1, 2, 3, 4");
            }
        }
    }
}
