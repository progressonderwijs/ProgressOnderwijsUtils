using System;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class EqualsConvertingIntToEnumTest
    {
        [Test]
        public void Monday_is_equal_to_int_1()
        {
            PAssert.That(() => DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, 1));
        }

        [Test]
        public void Int_1_is_equal_to_Monday()
        {
            PAssert.That(() => DBNullRemover.EqualsConvertingIntToEnum(1, DayOfWeek.Monday));
        }

        [Test]
        public void Monday_is_not_equal_to_int_2()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, 2));
        }

        [Test]
        public void Int_2_is_not_equal_to_Monday()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(2, DayOfWeek.Monday));
        }

        [Test]
        public void Monday_is_not_equal_to_null()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, null));
        }

        [Test]
        public void Null_is_not_equal_to_Monday()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(null, DayOfWeek.Monday));
        }

        [Test]
        public void Int_1_is_equal_to_int_1()
        {
            PAssert.That(() => DBNullRemover.EqualsConvertingIntToEnum(1, 1));
        }

        [Test]
        public void Int_1_is_not_equal_to_int_2()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(1, 2));
        }

        [Test]
        public void Monday_is_equal_to_Monday()
        {
            PAssert.That(() => DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, DayOfWeek.Monday));
        }

        [Test]
        public void Monday_is_not_equal_to_Sunday()
        {
            PAssert.That(() => !DBNullRemover.EqualsConvertingIntToEnum(DayOfWeek.Monday, DayOfWeek.Sunday));
        }
    }
}
