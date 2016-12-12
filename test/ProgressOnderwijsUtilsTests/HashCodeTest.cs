using System;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [PullRequestTest]
    public class HashCodeTest
    {
        [Test]
        public void IsConsistent()
        {
            PAssert.That(() => HashCodeHelper.ComputeHash("test", 3, null, DateTime.MinValue) != HashCodeHelper.ComputeHash("test", 3, null, DateTime.MinValue, null));
            //extra null matters
            PAssert.That(() => HashCodeHelper.ComputeHash("test", 3, null, DateTime.MinValue) != HashCodeHelper.ComputeHash("test", 3, DateTime.MinValue, null));
            //order matters
        }
    }
}
