﻿using System;
using System.Linq;
using NUnit.Framework;
using ProgressOnderwijsUtils.Log4Net;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

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

	[TestFixture]
	public class ILogExtensionsTest
	{
		private ILog sut;

		[SetUp]
		public void SetUp()
		{
			sut = LogManager.GetLogger(typeof(ILogExtensionsTest));
		}

		private void SetLevel(Level level)
		{
			((Logger)sut.Logger).Level = level;
		}

		[Test]
		public void Debug()
		{
			SetLevel(Level.Info);
			sut.Debug(() => { throw new Exception("unexpected");});	
			sut.Debug(() => { throw new Exception("unexpected");}, new Exception());	
		}

		[Test]
		public void Info()
		{
			SetLevel(Level.Warn);
			sut.Info(() => { throw new Exception("unexpected");});	
			sut.Info(() => { throw new Exception("unexpected");}, new Exception());	
		}

		[Test]
		public void Warn()
		{
			SetLevel(Level.Error);
			sut.Warn(() => { throw new Exception("unexpected");});	
			sut.Warn(() => { throw new Exception("unexpected");}, new Exception());	
		}

		[Test]
		public void Error()
		{
			SetLevel(Level.Fatal);
			sut.Error(() => { throw new Exception("unexpected");});	
			sut.Error(() => { throw new Exception("unexpected");}, new Exception());	
		}

		[Test]
		public void Fatal()
		{
			SetLevel(Level.Off);
			sut.Fatal(() => { throw new Exception("unexpected");});	
			sut.Fatal(() => { throw new Exception("unexpected");}, new Exception());	
		}
	}
}
