using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Extensions;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class DirTests
	{
		[Test]
		public void TestAccessDeniedCase()
		{
			DirectoryInfo dir = new DirectoryInfo(@"C:\Documents and Settings");
			PAssert.That(() => dir.Exists);
			Assert.Throws<UnauthorizedAccessException>(() => dir.GetDirectories());
			Assert.Throws<UnauthorizedAccessException>(() => dir.GetFiles());
			PAssert.That(() => !dir.TryGetDirectories().Any());
			PAssert.That(() => !dir.TryGetFiles().Any());
			PAssert.That(() => !dir.TryGetFiles("*.*").Any());
		}

		[Test]
		public void TestParentDirs()
		{
			PAssert.That(() => new DirectoryInfo(@"C:\a\b\c").SelfAndParentDirs().Select(d => d.FullName).SequenceEqual(new[]{
				 @"C:\a\b\c",
				 @"C:\a\b",
				 @"C:\a",
				 @"C:\",
			}));
		}


		[Test]
		public void TestDescendantFiles()
		{
			
			FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);
			var dir = file.Directory.Parent;

			PAssert.That(() => dir.DescendantFiles().Select(fi=>fi.FullName).Contains(file.FullName));
			PAssert.That(() => dir.DescendantFiles("*.dll").Select(fi => fi.FullName).Contains(file.FullName));
		}

	}
}
