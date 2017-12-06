﻿using System;
using ExpressionToCodeLib;
using Xunit;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class EqualsConvertingIntToEnumTest
    {
        [Fact]
        public void Monday_is_equal_to_int_1()
        {
            PAssert.That(() => DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, 1));
        }

        [Fact]
        public void Int_1_is_equal_to_Monday()
        {
            PAssert.That(() => DBNullRemover.EqualsConvertingIntToEnum(1, DayOfWeek.Monday));
        }

        [Fact]
        public void Monday_is_not_equal_to_int_2()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, 2));
        }

        [Fact]
        public void Int_2_is_not_equal_to_Monday()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(2, DayOfWeek.Monday));
        }

        [Fact]
        public void Monday_is_not_equal_to_null()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, null));
        }

        [Fact]
        public void Null_is_not_equal_to_Monday()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(null, DayOfWeek.Monday));
        }

        [Fact]
        public void Int_1_is_equal_to_int_1()
        {
            PAssert.That(() => DBNullRemover.EqualsConvertingIntToEnum(1, 1));
        }

        [Fact]
        public void Int_1_is_not_equal_to_int_2()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(1, 2));
        }

        [Fact]
        public void Monday_is_equal_to_Monday()
        {
            PAssert.That(() => DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, DayOfWeek.Monday));
        }

        [Fact]
        public void Monday_is_not_equal_to_Sunday()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, DayOfWeek.Sunday));
        }
    }
}
