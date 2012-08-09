using System;
using System.Linq;
using NUnit.Framework;
using ProgressOnderwijsUtils.Log4Net;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public sealed class RollingFileAppenderTest
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

	[TestFixture, Ignore("Does not seem to work in combination with NUnit 2.6 ???")]
	public sealed class LogExtensionsTest
	{
		private ILog sut;

		[SetUp]
		public void SetUp()
		{
			sut = LogManager.GetLogger(typeof(LogExtensionsTest));
		}

		private void SetLevel(Level level)
		{
			((Logger)sut.Logger).Level = level;
		}

		[Test]
		public void Debug()
		{
			SetLevel(Level.Debug);
			Assert.That(() => sut.Debug(() => { throw new Exception("unexpected");}), Throws.TypeOf<Exception>());	
			Assert.That(() => sut.Debug(() => { throw new Exception("unexpected");}, new Exception()), Throws.TypeOf<Exception>());	

			SetLevel(Level.Info);
			sut.Debug(() => { throw new Exception("unexpected");});	
			sut.Debug(() => { throw new Exception("unexpected");}, new Exception());	
		}

		[Test]
		public void Info()
		{
			SetLevel(Level.Info);
			Assert.That(() => sut.Info(() => { throw new Exception("unexpected");}), Throws.TypeOf<Exception>());	
			Assert.That(() => sut.Info(() => { throw new Exception("unexpected");}, new Exception()), Throws.TypeOf<Exception>());	

			SetLevel(Level.Warn);
			sut.Info(() => { throw new Exception("unexpected");});	
			sut.Info(() => { throw new Exception("unexpected");}, new Exception());	
		}

		[Test]
		public void Warn()
		{
			SetLevel(Level.Warn);
			Assert.That(() => sut.Warn(() => { throw new Exception("unexpected");}), Throws.TypeOf<Exception>());	
			Assert.That(() => sut.Warn(() => { throw new Exception("unexpected");}, new Exception()), Throws.TypeOf<Exception>());	

			SetLevel(Level.Error);
			sut.Warn(() => { throw new Exception("unexpected");});	
			sut.Warn(() => { throw new Exception("unexpected");}, new Exception());	
		}

		[Test]
		public void Error()
		{
			SetLevel(Level.Error);
			Assert.That(() => sut.Error(() => { throw new Exception("unexpected");}), Throws.TypeOf<Exception>());	
			Assert.That(() => sut.Error(() => { throw new Exception("unexpected");}, new Exception()), Throws.TypeOf<Exception>());	

			SetLevel(Level.Fatal);
			sut.Error(() => { throw new Exception("unexpected");});	
			sut.Error(() => { throw new Exception("unexpected");}, new Exception());	
		}

		[Test]
		public void Fatal()
		{
			SetLevel(Level.Fatal);
			Assert.That(() => sut.Fatal(() => { throw new Exception("unexpected");}), Throws.TypeOf<Exception>());	
			Assert.That(() => sut.Fatal(() => { throw new Exception("unexpected");}, new Exception()), Throws.TypeOf<Exception>());	

			SetLevel(Level.Off);
			sut.Fatal(() => { throw new Exception("unexpected");});	
			sut.Fatal(() => { throw new Exception("unexpected");}, new Exception());	
		}
	}
}
