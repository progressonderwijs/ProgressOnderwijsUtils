using ProgressOnderwijsUtils.Tests.Data;

namespace ProgressOnderwijsUtils.Tests;

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
    public void NonUnderlyingIntsConvertToEnum_can_work()
        => PAssert.That(() => DbValueConverter.FromDb<DayOfWeek>((short)2) == DayOfWeek.Tuesday);

    [Fact]
    public void DoublesConvertToInt_crashes()
        => Assert.Throws<InvalidCastException>(() => DbValueConverter.FromDb<int>(3.0) == 3);

    [Fact]
    public void TrivialConverterPassesValueTypesThrough_FromDb()
        => PAssert.That(() => DbValueConverter.FromDb<TrivialValue<int>>(3).Value == 3);

    [Fact]
    public void TrivialConverterPassesValueTypesThrough_ToDb()
        => PAssert.That(() => DbValueConverter.ToDb<int>(new TrivialValue<int>(3)) == 3);

    [Fact]
    public void NastyDoubleNullabilityToDbIsReasonable()
    {
        PAssert.That(() => DbValueConverter.ToDb<int>(new TrivialValue<int?>(3)) == 3);
        PAssert.That(() => DbValueConverter.ToDb<int?>(new TrivialValue<int>(3)) == 3);
        PAssert.That(() => DbValueConverter.ToDb<int?>(new TrivialValue<int?>(3)) == 3);
        PAssert.That(() => DbValueConverter.ToDb<int?>(default(TrivialValue<int?>?)) == null);
    }

    [Fact]
    public void ToDb_is_NoOp_when_already_of_type()
    {
        PAssert.That(() => DbValueConverter.ToDb<int>(3) == 3);
        PAssert.That(() => DbValueConverter.ToDb<int?>(3) == 3);
        PAssert.That(() => DbValueConverter.ToDb<TrivialValue<int>>(new TrivialValue<int>(3)).Value == 3);
        PAssert.That(() => DbValueConverter.ToDb<TrivialValue<int>?>(new TrivialValue<int>(3)).Value.Value == 3);
        PAssert.That(() => DbValueConverter.ToDb<DayOfWeek>(DayOfWeek.Wednesday) == DayOfWeek.Wednesday);
        PAssert.That(() => DbValueConverter.ToDb<DayOfWeek?>(DayOfWeek.Wednesday) == DayOfWeek.Wednesday);
    }

    [Fact]
    public void NullableTrivialConverterConvertsNullableValueIntoUnwrappedNull_ToDb()
        => PAssert.That(() => DbValueConverter.ToDb<int?>(default(TrivialValue<int>?)) == null);

    [Fact]
    public void EnumConvertibleTypesCanNotConvertFomInt()
        => Assert.Throws<InvalidCastException>(() => DbValueConverter.FromDb<TrivialValue<DayOfWeek>>(3).Value);

    [Fact]
    public void IntConvertibleTypesCanConvertFomEnum()
        => PAssert.That(() => DbValueConverter.FromDb<TrivialValue<int>>(DayOfWeek.Thursday).Value == 4, "weird, probably not a good idea, but current behavior nontheless");

    [Fact]
    public void IntConvertibleTypesCanConvertFomShort()
        => PAssert.That(() => DbValueConverter.FromDb<TrivialValue<int>>((short)37).Value == 37, "weird, probably not a good idea, but current behavior nontheless");

    [Fact]
    public void TrivialConverterPassesRefTypesThrough()
        => PAssert.That(() => DbValueConverter.FromDb<TrivialValue<string>>("asdf").Value == "asdf");

    [Fact]
    public void NullableTrivialConverterConvertsNullableValueIntoUnwrappedNull()
    {
        PAssert.That(() => DbValueConverter.FromDb<TrivialValue<int?>?>(null) == null);
        PAssert.That(() => !Equals(DbValueConverter.FromDb<TrivialValue<int?>?>(null), new TrivialValue<int?>(null)));
    }

    [Fact]
    public void NullableTrivialConverterConvertsNullableRefIntoUnwrappedNull()
    {
        PAssert.That(() => DbValueConverter.FromDb<TrivialValue<string>?>(null) == null);
        PAssert.That(() => !Equals(DbValueConverter.FromDb<TrivialValue<string>?>(null), new TrivialValue<string?>(null)));
    }

    [Fact]
    public void CanCastToNullableNonConvertible()
        => PAssert.That(() => DbValueConverter.FromDb<int?>(3) == 3);

    [Fact]
    public void CanCastToNullableFromConvertible()
        => PAssert.That(() => 3 == DbValueConverter.ToDb<int?>(new TrivialValue<int?>(3)));

    [Fact]
    public void CanCastToNullableFromEnum()
        => PAssert.That(() => 3 == DbValueConverter.ToDb<int?>(DayOfWeek.Wednesday));

    [Fact]
    public void CanCastToByteArrayFromULong()
        => PAssert.That(() => DbValueConverter.ToDb<byte[]>(ulong.MaxValue).SequenceEqual(Enumerable.Repeat((byte)255, 8)));

    [Fact]
    public void CanCastToByteArrayFromNullableULong()
        => PAssert.That(() => null == DbValueConverter.ToDb<byte[]?>(default(ulong?)));

    [Fact]
    public void CanCastFromByteArrayToNullableULong()
    {
        var allBitsOn = Enumerable.Repeat((byte)255, 8).ToArray();
        PAssert.That(() => null == DbValueConverter.FromDb<ulong?>(default(byte[]?)));
        PAssert.That(() => ulong.MaxValue == DbValueConverter.FromDb<ulong?>(allBitsOn));
    }

    [Fact]
    public void CanCastToNullableConvertibleOfReferenceType()
        => PAssert.That(() => DbValueConverter.FromDb<TrivialValue<string>?>(new TrivialValue<string>("asdf")).AssertNotNull().Value == "asdf");

    [Fact]
    public void CanDynamicCastToNullableConvertibleOfReferenceType()
        => PAssert.That(() => new TrivialValue<string>("asdf").Equals(DbValueConverter.DynamicCast(new TrivialValue<string>("asdf"), typeof(TrivialValue<string>?))));

    [Fact]
    public void CanDynamicCastToPlainStringFromConvertibleReferenceType()
        => PAssert.That(() => "asdf" == (string?)DbValueConverter.DynamicCast(new TrivialValue<string>("asdf"), typeof(string)));

    [Fact]
    public void CanDynamicCastFromPlainValueToConvertibleType()
    {
        PAssert.That(() => "asdf" == ((TrivialValue<string>?)DbValueConverter.DynamicCast("asdf", typeof(TrivialValue<string>))).AssertNotNull().Value);
        PAssert.That(() => 42 == ((TrivialValue<int>?)DbValueConverter.DynamicCast(42, typeof(TrivialValue<int>))).AssertNotNull().Value);
    }

    [Fact]
    public void CanDynamicCastFromPlainValueToNullableConvertibleType()
    {
        PAssert.That(() => "asdf" == ((TrivialValue<string>?)DbValueConverter.DynamicCast("asdf", typeof(TrivialValue<string>?))).AssertNotNull().Value);
        PAssert.That(() => 42 == ((TrivialValue<int>?)DbValueConverter.DynamicCast(42, typeof(TrivialValue<int>?))).AssertNotNull().Value);
        PAssert.That(() => null == DbValueConverter.DynamicCast(null, typeof(TrivialValue<string>?)));
        PAssert.That(() => null == DbValueConverter.DynamicCast(null, typeof(TrivialValue<int>?)));
        PAssert.That(() => null == DbValueConverter.DynamicCast(null, typeof(TrivialValue<int?>?)));
    }

    [Fact]
    public void CanNotCastNullToNonNullableConvertibleContainingNullable()
        => Assert.ThrowsAny<Exception>(() => null == DbValueConverter.DynamicCast(null, typeof(TrivialValue<int?>)));

    [Fact]
    public void CanDynamicCastBetweenEnumsAndInts()
    {
        PAssert.That(() => 3 == (int?)DbValueConverter.DynamicCast(DayOfWeek.Wednesday, typeof(int)));
        PAssert.That(() => 3 == (int?)DbValueConverter.DynamicCast(DayOfWeek.Wednesday, typeof(int?)));
        PAssert.That(() => DayOfWeek.Wednesday == (DayOfWeek?)DbValueConverter.DynamicCast(3, typeof(DayOfWeek)));
        PAssert.That(() => DayOfWeek.Wednesday == (DayOfWeek?)DbValueConverter.DynamicCast(3, typeof(DayOfWeek?)));
    }

    [Fact]
    public void CanConvertIntsFromDbToEnums()
    {
        PAssert.That(() => DayOfWeek.Wednesday == DbValueConverter.FromDb<DayOfWeek?>(3));
        PAssert.That(() => DayOfWeek.Wednesday == DbValueConverter.FromDb<DayOfWeek>(3));
        PAssert.That(() => null == DbValueConverter.FromDb<DayOfWeek?>(null));
        PAssert.That(() => 3 == DbValueConverter.ToDb<int>(DayOfWeek.Wednesday));
        PAssert.That(() => 3 == DbValueConverter.ToDb<int?>(DayOfWeek.Wednesday));
        PAssert.That(() => null == DbValueConverter.ToDb<int?>(null));
    }

    [Fact]
    public void CanDynamicCastFromNull()
        => PAssert.That(() => null == (int?)DbValueConverter.DynamicCast(null, typeof(int?)));

    [Fact]
    public void CanDynamicCastFromBetweenConvertiblesRegarlessOfNullability()
        => PAssert.That(() => "asdf" == ((TrivialValue<string>?)DbValueConverter.DynamicCast(new TrivialValue<string>("asdf"), typeof(TrivialValue<string>?))).AssertNotNull().Value);

    [Fact]
    public void CanCastFromConvertibleOfReferenceType()
        => PAssert.That(() => DbValueConverter.ToDb<string>(new TrivialValue<string>("asdf")) == "asdf");

    [Fact]
    public void CanCastToNullableConvertibleOfValueType()
        => PAssert.That(() => DbValueConverter.FromDb<TrivialValue<int>?>(new TrivialValue<int>(-123)).AssertNotNull().Value == -123);
}
