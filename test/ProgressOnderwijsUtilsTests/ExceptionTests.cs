using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class ExceptionTests
	{
		[Test]
		public void Exceptions()
		{
			Assert.AreEqual("bla", Assert.Throws<GeenRechtException>(() => { throw new GeenRechtException("bla"); }).Message);

			Assert.Throws<QueryException>(() => { throw new QueryException(); });
			Assert.AreEqual("bla", Assert.Throws<QueryException>(() => { throw new QueryException("bla"); }).Message);
			Assert.AreEqual("bla2", Assert.Throws<QueryException>(() => { throw new QueryException("bla", new ProgressNetException("bla2")); }).InnerException.Message);


			Assert.Throws<TemplateException>(() => { throw new TemplateException(); });
			Assert.AreEqual("bla", Assert.Throws<TemplateException>(() => { throw new TemplateException("bla"); }).Message);
			Assert.AreEqual("bla2", Assert.Throws<TemplateException>(() => { throw new TemplateException("bla", new ProgressNetException("bla2")); }).InnerException.Message);
			Assert.AreEqual("bla2", SerializationCloner.Clone(Assert.Throws<TemplateException>(() => { throw new TemplateException("bla", new ProgressNetException("bla2")); })).InnerException.Message);

			var texc = Assert.Throws<TemplateException>(() => { throw new TemplateException(37, 42, "bla"); });
			PAssert.That(() => texc.Line == 37 && texc.Position == 42);
		}
	}
}
