using System;
using NUnit.Framework;
using Progress.Business.Data.GenericEdit;
using Progress.Business.Test;
using Progress.Business.Text;
using Progress.Business.Tools;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class ExceptionTests
    {
        [Test]
        public void ClonerTest()
        {
            Assert.Throws<ArgumentException>(() => SerializationCloner.Clone(new Bla()));
        }

        sealed class Bla { }

        [Test]
        public void Exceptions()
        {
            Assert.AreEqual("bla", Assert.Throws<GeenRechtException>(() => { throw new GeenRechtException("bla"); }).Message);

            Assert.Throws<ParameterizedSqlExecutionException>(() => { throw new ParameterizedSqlExecutionException(); });
            Assert.AreEqual("bla", Assert.Throws<ParameterizedSqlExecutionException>(() => { throw new ParameterizedSqlExecutionException("bla"); }).Message);
            Assert.AreEqual("bla2", Assert.Throws<ParameterizedSqlExecutionException>(() => { throw new ParameterizedSqlExecutionException("bla", new ProgressNetException("bla2")); }).InnerException.Message);

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
