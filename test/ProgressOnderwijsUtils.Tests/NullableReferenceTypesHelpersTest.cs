﻿using System;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class NullableReferenceTypesHelpersTest
    {
        [Fact]
        public void AssertNotNull_throws_when_argument_is_null()
        {
            Assert.ThrowsAny<Exception>(() => default(object).AssertNotNull());
        }

        [Fact]
        public void AssertNotNull_doesnt_throw_when_argument_is_not_null()
        {
            var unused = new object().AssertNotNull();
        }

        [Fact]
        public void PretendNullable_does_nothing()
        {
            var obj = new object();
            PAssert.That(() => obj.PretendNullable() == obj);
        }
    }
}
