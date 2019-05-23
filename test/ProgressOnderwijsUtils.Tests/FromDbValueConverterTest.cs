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
            => PAssert.That(() => DbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, 1));

        [Fact]
        public void Int_1_is_equal_to_Monday()
            => PAssert.That(() => DbValueConverter.EqualsConvertingIntToEnum(1, DayOfWeek.Monday));

        [Fact]
        public void Monday_is_not_equal_to_int_2()
            => PAssert.That(() => !DbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, 2));

        [Fact]
        public void Int_2_is_not_equal_to_Monday()
            => PAssert.That(() => !DbValueConverter.EqualsConvertingIntToEnum(2, DayOfWeek.Monday));

        [Fact]
        public void Monday_is_not_equal_to_null()
            => PAssert.That(() => !DbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, null));

        [Fact]
        public void Null_is_not_equal_to_Monday()
            => PAssert.That(() => !DbValueConverter.EqualsConvertingIntToEnum(null, DayOfWeek.Monday));

        [Fact]
        public void Int_1_is_equal_to_int_1()
            => PAssert.That(() => DbValueConverter.EqualsConvertingIntToEnum(1, 1));

        [Fact]
        public void Int_1_is_not_equal_to_int_2()
            => PAssert.That(() => !DbValueConverter.EqualsConvertingIntToEnum(1, 2));

        [Fact]
        public void Monday_is_equal_to_Monday()
            => PAssert.That(() => DbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, DayOfWeek.Monday));

        [Fact]
        public void Monday_is_not_equal_to_Sunday()
            => PAssert.That(() => !DbValueConverter.EqualsConvertingIntToEnum(DayOfWeek.Monday, DayOfWeek.Sunday));

        [Fact]
        public void DbNullConvertsToNullableInt()
            => PAssert.That(() => DbValueConverter.FromDb<int?>(DBNull.Value).HasValue == false);

        [Fact]
        public void NullConvertsToNullableInt()
            => PAssert.That(() => DbValueConverter.FromDb<int?>(default(string)).HasValue == false);

        [Fact]
        public void NullConvertsToString()
            => PAssert.That(() => DbValueConverter.FromDb<string>(default(string)) == null);

        [Fact]
        public void DbNullConvertsToString()
            => PAssert.That(() => DbValueConverter.FromDb<string>(DBNull.Value) == null);

        [Fact]
        public void StringsGetPassedThrough()
            => PAssert.That(() => DbValueConverter.FromDb<string>("hello") == "hello");

        [Fact]
        public void BytesGetPassedThrough()
            => PAssert.That(() => DbValueConverter.FromDb<byte>((byte)128) == 128);

        [Fact]
        public void IntsGetPassedThrough()
            => PAssert.That(() => DbValueConverter.FromDb<int>(42) == 42);

        [Fact]
        public void IntsConvertToShorts_crashes()
            => Assert.Throws<InvalidCastException>(() => DbValueConverter.FromDb<short>(42) == 42);

        [Fact]
        public void EnumsConvertToInt()
            => PAssert.That(() => DbValueConverter.FromDb<int>(DayOfWeek.Tuesday) == 2);

        [Fact]
        public void EnumsConvertToNonUnderlyingType_crashes()
            => Assert.Throws<InvalidCastException>(() => DbValueConverter.FromDb<short>(DayOfWeek.Tuesday) == 2);

        [Fact]
        public void IntsConvertToEnum()
            => PAssert.That(() => DbValueConverter.FromDb<DayOfWeek>(2) == DayOfWeek.Tuesday);

        [Fact]
        public void NonUnderlyingIntsConvertToEnum_crashes()
            => Assert.Throws<InvalidCastException>(() => DbValueConverter.FromDb<DayOfWeek>((short)2) == DayOfWeek.Tuesday);

        [Fact]
        public void DoublesConvertToInt_crashes()
            => Assert.Throws<InvalidCastException>(() => DbValueConverter.FromDb<int>(3.0) == 3);

        [Fact]
        public void TrivialConverterPassesValueTypesThrough_FromDb()
            => PAssert.That(() => DbValueConverter.FromDb<TrivialConvertibleValue<int>>(3).Value == 3);

        [Fact]
        public void TrivialConverterPassesValueTypesThrough_ToDb()
            => PAssert.That(() => DbValueConverter.ToDb<int>(new TrivialConvertibleValue<int>(3)) == 3);

        [Fact]
        public void NastyDoubleNullabilityToDbIsReasonable()
        {
            PAssert.That(() => DbValueConverter.ToDb<int>(new TrivialConvertibleValue<int?>(3)) == 3);
            PAssert.That(() => DbValueConverter.ToDb<int?>(new TrivialConvertibleValue<int>(3)) == 3);
            PAssert.That(() => DbValueConverter.ToDb<int?>(new TrivialConvertibleValue<int?>(3)) == 3);
            PAssert.That(() => DbValueConverter.ToDb<int?>(default(TrivialConvertibleValue<int?>?)) == null);
        }

        [Fact]
        public void NullableTrivialConverterConvertsNullableValueIntoUnwrappedNull_ToDb()
            => PAssert.That(() => DbValueConverter.ToDb<int?>(default(TrivialConvertibleValue<int>?)) == null);

        [Fact]
        public void EnumConvertibleTypesCanNotConvertFomInt()
            => Assert.Throws<InvalidCastException>(() => DbValueConverter.FromDb<TrivialConvertibleValue<DayOfWeek>>(3).Value);

        [Fact]
        public void IntConvertibleTypesCanConvertFomEnum()
            => PAssert.That(() => DbValueConverter.FromDb<TrivialConvertibleValue<int>>(DayOfWeek.Thursday).Value == 4, "weird, probably not a good idea, but current behavior nontheless");

        [Fact]
        public void IntConvertibleTypesCanConvertFomShort()
            => PAssert.That(() => DbValueConverter.FromDb<TrivialConvertibleValue<int>>((short)37).Value == 37, "weird, probably not a good idea, but current behavior nontheless");

        [Fact]
        public void TrivialConverterPassesRefTypesThrough()
            => PAssert.That(() => DbValueConverter.FromDb<TrivialConvertibleValue<string>>("asdf").Value == "asdf");

        [Fact]
        public void NullableTrivialConverterConvertsNullableValueIntoUnwrappedNull()
        {
            PAssert.That(() => DbValueConverter.FromDb<TrivialConvertibleValue<int?>?>(null) == null);
            PAssert.That(() => !Equals(DbValueConverter.FromDb<TrivialConvertibleValue<int?>?>(null), new TrivialConvertibleValue<int?>(null)));
        }

        [Fact]
        public void NullableTrivialConverterConvertsNullableRefIntoUnwrappedNull()
        {
            PAssert.That(() => DbValueConverter.FromDb<TrivialConvertibleValue<string>?>(null) == null);
            PAssert.That(() => !Equals(DbValueConverter.FromDb<TrivialConvertibleValue<string>?>(null), new TrivialConvertibleValue<string>(null)));
        }

        [Fact]
        public void CanCastToNullableNonConvertible()
            => PAssert.That(() => DbValueConverter.FromDb<int?>(3) == 3);

        [Fact]
        public void CanCastToNullableFromConvertible()
            => PAssert.That(() => DbValueConverter.ToDb<int?>(new TrivialConvertibleValue<int?>(3)) == 3);

        [Fact]
        public void CanCastToNullableConvertibleOfReferenceType()
            => PAssert.That(() => DbValueConverter.FromDb<TrivialConvertibleValue<string>?>(new TrivialConvertibleValue<string>("asdf")).Value.Value == "asdf");

        [Fact]
        public void CanDynamicCastToNullableConvertibleOfReferenceType()
            => PAssert.That(() => new TrivialConvertibleValue<string>("asdf").Equals(DbValueConverter.DynamicCast(new TrivialConvertibleValue<string>("asdf"), typeof(TrivialConvertibleValue<string>?))));

        [Fact]
        public void CanDynamicCastToPlainStringFromConvertibleReferenceType()
            => PAssert.That(() => "asdf" == (string)DbValueConverter.DynamicCast(new TrivialConvertibleValue<string>("asdf"), typeof(string)));

        [Fact]
        public void CanDynamicCastFromPlainValueToConvertibleType()
        {
            PAssert.That(() => "asdf" == ((TrivialConvertibleValue<string>)DbValueConverter.DynamicCast("asdf", typeof(TrivialConvertibleValue<string>))).Value);
            PAssert.That(() => 42 == ((TrivialConvertibleValue<int>)DbValueConverter.DynamicCast(42, typeof(TrivialConvertibleValue<int>))).Value);
        }

        [Fact]
        public void CanDynamicCastFromPlainValueToNullableConvertibleType()
        {
            PAssert.That(() => "asdf" == ((TrivialConvertibleValue<string>)DbValueConverter.DynamicCast("asdf", typeof(TrivialConvertibleValue<string>?))).Value);
            PAssert.That(() => 42 == ((TrivialConvertibleValue<int>)DbValueConverter.DynamicCast(42, typeof(TrivialConvertibleValue<int>?))).Value);
            PAssert.That(() => null == DbValueConverter.DynamicCast(null, typeof(TrivialConvertibleValue<string>?)));
            PAssert.That(() => null == DbValueConverter.DynamicCast(null, typeof(TrivialConvertibleValue<int>?)));
            PAssert.That(() => null == DbValueConverter.DynamicCast(null, typeof(TrivialConvertibleValue<int?>)));
            PAssert.That(() => null == DbValueConverter.DynamicCast(null, typeof(TrivialConvertibleValue<int?>?)));
        }

        [Fact]
        public void CanDynamicCastBetweenEnumsAndInts()
        {
            PAssert.That(() => 3 == (int)DbValueConverter.DynamicCast(DayOfWeek.Wednesday, typeof(int)));
            PAssert.That(() => 3 == (int)DbValueConverter.DynamicCast(DayOfWeek.Wednesday, typeof(int?)));
            PAssert.That(() => DayOfWeek.Wednesday == (DayOfWeek)DbValueConverter.DynamicCast(3, typeof(DayOfWeek)));
            PAssert.That(() => DayOfWeek.Wednesday == (DayOfWeek)DbValueConverter.DynamicCast(3, typeof(DayOfWeek?)));
        }

        [Fact]
        public void CanDynamicCastFromBetweenConvertiblesRegarlessOfNullability()
            => PAssert.That(() => "asdf" == ((TrivialConvertibleValue<string>)DbValueConverter.DynamicCast(new TrivialConvertibleValue<string>("asdf"), typeof(TrivialConvertibleValue<string>?))).Value);

        [Fact]
        public void CanCastFromConvertibleOfReferenceType()
            => PAssert.That(() => DbValueConverter.ToDb<string>(new TrivialConvertibleValue<string>("asdf")) == "asdf");

        [Fact]
        public void CanCastToNullableConvertibleOfValueType()
            => PAssert.That(() => DbValueConverter.FromDb<TrivialConvertibleValue<int>?>(new TrivialConvertibleValue<int>(-123)).Value.Value == -123);
    }
}
