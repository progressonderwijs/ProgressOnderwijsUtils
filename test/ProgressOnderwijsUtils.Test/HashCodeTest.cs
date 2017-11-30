﻿using System;
using ExpressionToCodeLib;
using Xunit;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    
    public class HashCodeTest
    {
        [Fact]
        public void IsConsistent()
        {
            PAssert.That(() => HashCodeHelper.ComputeHash("test", 3, null, DateTime.MinValue) != HashCodeHelper.ComputeHash("test", 3, null, DateTime.MinValue, null));
            //extra null matters
            PAssert.That(() => HashCodeHelper.ComputeHash("test", 3, null, DateTime.MinValue) != HashCodeHelper.ComputeHash("test", 3, DateTime.MinValue, null));
            //order matters
        }
    }
}
