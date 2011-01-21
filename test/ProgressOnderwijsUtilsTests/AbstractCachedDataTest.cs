using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;
using ProgressOnderwijsUtils.WebSupport;
using System.IO;

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

		public TempTextFileTest() : this(new FileInfo(Path.GetTempFileName())) { }

		protected override void Reload()
		{
			file.Refresh();
			Data = file.Exists ? File.ReadAllText(file.FullName) : null;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if(file!=null) file.Delete();
		}
	}


	[TestFixture, NightlyOnly]
	public class AbstractCachedDataTest
	{
		[Test]
		public void PlainWorks()
		{
			FileInfo oldfile;
			using (var t = new TempTextFileTest())
			{
				oldfile = t.file;
				PAssert.That(() => oldfile.Exists && t.Data == "");

				PAssert.That(() => t.WatchedFilesAsRelativeUris(oldfile.Directory.Parent).Single().ToString()
					.StartsWith(oldfile.Directory.Name + "/"));

				PAssert.That(() => t.LastWriteTimeUtc <= DateTime.UtcNow && t.LastWriteTimeUtc >= DateTime.UtcNow - TimeSpan.FromMinutes(1.0));
			}
			oldfile.Refresh();
			PAssert.That(() => !oldfile.Exists);
		}

		[Test]
		public void NeedsRealDir()
		{
			Assert.Throws<ArgumentException>(()=> {
				using (var t = new TempTextFileTest(new FileInfo(@"A:\b\c\d\e")))
				{
				}
			});

		}

		[Test]
		public void DeleteRecreateWorks()
		{
			FileInfo oldfile;
			using (var t = new TempTextFileTest())
			{
				oldfile = t.file;
				oldfile.Delete();
				PAssert.That(() => !oldfile.Exists && t.Data == null);
				File.WriteAllText(oldfile.FullName, "Hello World!");
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
			using (var t = new TempTextFileTest())
			{
				PAssert.That(() => t.file.Exists && t.Data == "");
				File.WriteAllText(t.file.FullName, "1, 2, 3, 4");
				PAssert.That(() => t.file.Exists && t.Data == "1, 2, 3, 4");
			}
		}
	}
}
