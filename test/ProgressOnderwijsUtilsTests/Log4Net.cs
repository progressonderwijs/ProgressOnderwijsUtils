using System;
using System.Linq;
using NUnit.Framework;
using ProgressOnderwijsUtils.Log4Net;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class RollingFileAppenderTest
	{
		private RollingFileAppender sut;

		[SetUp]
		public void SetUp()
		{
			sut = new RollingFileAppender();
		}

		[Test]
		[TestCase("test.log", @"C:\inetpub\logs\Progress.NET\test.log")]
		[TestCase(@"C:\test.log", @"C:\test.log")]
		[TestCase(@"C:\log\test.log", @"C:\log\test.log")]
		public void File(string value, string expected)
		{
			sut.File = value;
			Assert.That(sut.File, Is.EqualTo(expected));
		}
	}
}
