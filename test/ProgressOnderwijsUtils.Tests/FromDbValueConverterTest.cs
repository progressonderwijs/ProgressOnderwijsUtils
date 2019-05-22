using System;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Tests.Data;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class FromDbValueConverterTest
    {
        [Fact]
        public void Monday_is_equal_to_int_1()
            => PAssert.That(() => FromDbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, 1));

        [Fact]
        public void Int_1_is_equal_to_Monday()
            => PAssert.That(() => FromDbValueConverter.EqualsConvertingIntToEnum(1, DayOfWeek.Monday));

        [Fact]
        public void Monday_is_not_equal_to_int_2()
            => PAssert.That(() => !FromDbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, 2));

        [Fact]
        public void Int_2_is_not_equal_to_Monday()
            => PAssert.That(() => !FromDbValueConverter.EqualsConvertingIntToEnum(2, DayOfWeek.Monday));

        [Fact]
        public void Monday_is_not_equal_to_null()
            => PAssert.That(() => !FromDbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, null));

        [Fact]
        public void Null_is_not_equal_to_Monday()
            => PAssert.That(() => !FromDbValueConverter.EqualsConvertingIntToEnum(null, DayOfWeek.Monday));

        [Fact]
        public void Int_1_is_equal_to_int_1()
            => PAssert.That(() => FromDbValueConverter.EqualsConvertingIntToEnum(1, 1));

        [Fact]
        public void Int_1_is_not_equal_to_int_2()
            => PAssert.That(() => !FromDbValueConverter.EqualsConvertingIntToEnum(1, 2));

        [Fact]
        public void Monday_is_equal_to_Monday()
            => PAssert.That(() => FromDbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, DayOfWeek.Monday));

        [Fact]
        public void Monday_is_not_equal_to_Sunday()
            => PAssert.That(() => !FromDbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, DayOfWeek.Sunday));

        [Fact]
        public void DbNullConvertsToNullableInt()
            => PAssert.That(() => FromDbValueConverter.Cast<int?>(DBNull.Value).HasValue == false);

        [Fact]
        public void NullConvertsToNullableInt()
            => PAssert.That(() => FromDbValueConverter.Cast<int?>(default(string)).HasValue == false);

        [Fact]
        public void NullConvertsToString()
            => PAssert.That(() => FromDbValueConverter.Cast<string>(default(string)) == null);

        [Fact]
        public void DbNullConvertsToString()
            => PAssert.That(() => FromDbValueConverter.Cast<string>(DBNull.Value) == null);

        [Fact]
        public void StringsGetPassedThrough()
            => PAssert.That(() => FromDbValueConverter.Cast<string>("hello") == "hello");

        [Fact]
        public void BytesGetPassedThrough()
            => PAssert.That(() => FromDbValueConverter.Cast<byte>((byte)128) == 128);

        [Fact]
        public void IntsGetPassedThrough()
            => PAssert.That(() => FromDbValueConverter.Cast<int>(42) == 42);

        [Fact]
        public void IntsConvertToShorts_crashes()
            => Assert.Throws<InvalidCastException>(() => FromDbValueConverter.Cast<short>(42) == 42);

        [Fact]
        public void EnumsConvertToInt()
            => PAssert.That(() => FromDbValueConverter.Cast<int>(DayOfWeek.Tuesday) == 2);

        [Fact]
        public void EnumsConvertToNonUnderlyingType_crashes()
            => Assert.Throws<InvalidCastException>(() => FromDbValueConverter.Cast<short>(DayOfWeek.Tuesday) == 2);

        [Fact]
        public void IntsConvertToEnum()
            => PAssert.That(() => FromDbValueConverter.Cast<DayOfWeek>(2) == DayOfWeek.Tuesday);

        [Fact]
        public void NonUnderlyingIntsConvertToEnum_crashes()
            => Assert.Throws<InvalidCastException>(() => FromDbValueConverter.Cast<DayOfWeek>((short)2) == DayOfWeek.Tuesday);

        [Fact]
        public void DoublesConvertToInt_crashes()
            => Assert.Throws<InvalidCastException>(() => FromDbValueConverter.Cast<int>(3.0) == 3);

        [Fact]
        public void TrivialConverterPassesValueTypesThrough()
            => PAssert.That(() => FromDbValueConverter.Cast<TrivialConvertibleValue<int>>(3).Value == 3);

        [Fact]
        public void EnumConvertibleTypesCanNotConvertFomInt()
            => Assert.Throws<InvalidCastException>(() => FromDbValueConverter.Cast<TrivialConvertibleValue<DayOfWeek>>(3).Value);

        [Fact]
        public void IntConvertibleTypesCanConvertFomEnum()
            => PAssert.That(() => FromDbValueConverter.Cast<TrivialConvertibleValue<int>>(DayOfWeek.Thursday).Value == 4, "weird, probably not a good idea, but current behavior nontheless");

        [Fact]
        public void IntConvertibleTypesCanConvertFomShort()
            => PAssert.That(() => FromDbValueConverter.Cast<TrivialConvertibleValue<int>>((short)37).Value == 37, "weird, probably not a good idea, but current behavior nontheless");

        [Fact]
        public void TrivialConverterPassesRefTypesThrough()
            => PAssert.That(() => FromDbValueConverter.Cast<TrivialConvertibleValue<string>>("asdf").Value == "asdf");

        [Fact]
        public void NullableTrivialConverterConvertsNullableValueIntoUnwrappedNull()
        {
            PAssert.That(() => FromDbValueConverter.Cast<TrivialConvertibleValue<int?>?>(null) == null);
            PAssert.That(() => !Equals(FromDbValueConverter.Cast<TrivialConvertibleValue<int?>?>(null), new TrivialConvertibleValue<int?>(null)));
        }

        [Fact]
        public void NullableTrivialConverterConvertsNullableRefIntoUnwrappedNull()
        {
            PAssert.That(() => FromDbValueConverter.Cast<TrivialConvertibleValue<string>?>(null) == null);
            PAssert.That(() => !Equals(FromDbValueConverter.Cast<TrivialConvertibleValue<int?>?>(null), new TrivialConvertibleValue<string>(null)));
        }
    }
}
