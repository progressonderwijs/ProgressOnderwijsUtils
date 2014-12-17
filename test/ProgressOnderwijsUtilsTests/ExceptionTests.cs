using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class ExceptionTests
    {
        [Test]
        public void ClonerTest() { Assert.Throws<ArgumentException>(() => SerializationCloner.Clone(new Bla())); }

        sealed class Bla { }

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
            Assert.AreEqual(
                "bla2",
                SerializationCloner.Clone(Assert.Throws<TemplateException>(() => { throw new TemplateException("bla", new ProgressNetException("bla2")); }))
                    .InnerException.Message);

            TemplateException texc = Assert.Throws<TemplateException>(() => { throw new TemplateException(37, 42, "bla"); });
            PAssert.That(() => texc.Line == 37 && texc.Position == 42);

            Assert.AreEqual("bla", Assert.Throws<GenericMetaDataException>(() => { throw new GenericMetaDataException("bla"); }).Message);
            Assert.AreEqual(
                "bla2",
                Assert.Throws<GenericMetaDataException>(() => { throw new GenericMetaDataException("bla", new ProgressNetException("bla2")); }).InnerException.Message);
            Assert.AreEqual(
                "bla2",
                SerializationCloner.Clone(Assert.Throws<GenericMetaDataException>(() => { throw new GenericMetaDataException("bla", new ProgressNetException("bla2")); }))
                    .InnerException.Message);

            Assert.Throws<PNAssertException>(() => { throw new PNAssertException(); });
            Assert.AreEqual("bla", Assert.Throws<PNAssertException>(() => { throw new PNAssertException("bla"); }).Message);
            Assert.AreEqual("bla2", Assert.Throws<PNAssertException>(() => { throw new PNAssertException("bla", new ProgressNetException("bla2")); }).InnerException.Message);
            Assert.AreEqual(
                "bla2",
                SerializationCloner.Clone(Assert.Throws<PNAssertException>(() => { throw new PNAssertException("bla", new ProgressNetException("bla2")); }))
                    .InnerException.Message);

            Assert.Throws<ConverteerException>(() => { throw new ConverteerException(); });
            Assert.AreEqual("bla", Assert.Throws<ConverteerException>(() => { throw new ConverteerException("bla"); }).Message);
            Assert.AreEqual(
                "bla2",
                Assert.Throws<ConverteerException>(() => { throw new ConverteerException("bla", new ProgressNetException("bla2")); }).InnerException.Message);
            Assert.AreEqual(
                "bla2",
                SerializationCloner.Clone(Assert.Throws<ConverteerException>(() => { throw new ConverteerException("bla", new ProgressNetException("bla2")); }))
                    .InnerException.Message);
        }
    }
}
