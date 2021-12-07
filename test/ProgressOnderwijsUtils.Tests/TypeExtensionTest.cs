using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class TypeExtensionTest
    {
        [Fact]
        public void TestNullability()
        {
            PAssert.That(() => typeof(int?).CanBeNull());
            PAssert.That(() => typeof(string).CanBeNull());
            PAssert.That(() => typeof(TypeExtensionTest).CanBeNull());
            PAssert.That(() => typeof(IEnumerable<int>).CanBeNull());
            PAssert.That(() => typeof(TypeExtension).CanBeNull());
            PAssert.That(() => !typeof(int).CanBeNull());

            PAssert.That(() => !typeof(DayOfWeek).CanBeNull());
            PAssert.That(() => !typeof(KeyValuePair<int?, string>).CanBeNull());

            PAssert.That(() => typeof(Enum).CanBeNull()); //WTF???? maar goed, dit is dan ook wel een heel gek type.
        }

        class SampleBase : ArrayList { }

        // ReSharper disable once UnusedTypeParameter
        class Sample<T> : SampleBase { }

        sealed class SampleX<T> : Sample<T> { }

        [Fact]
        public void TestBases()
        {
            PAssert.That(() => typeof(int?).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
            PAssert.That(() => typeof(string).BaseTypes().SequenceEqual(new[] { typeof(object) }));
            PAssert.That(() => typeof(TypeExtensionTest).BaseTypes().SequenceEqual(new[] { typeof(object) }));
            PAssert.That(() => typeof(IEnumerable<int>).BaseTypes().SequenceEqual(new Type[] { }));
            PAssert.That(() => typeof(int).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
            PAssert.That(() => typeof(TypeExtension).BaseTypes().SequenceEqual(new[] { typeof(object) }));
            PAssert.That(() => typeof(Enum).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
            PAssert.That(() => typeof(DayOfWeek).BaseTypes().SequenceEqual(new[] { typeof(Enum), typeof(ValueType), typeof(object) }));
            PAssert.That(() => typeof(KeyValuePair<int?, string>).BaseTypes().SequenceEqual(new[] { typeof(ValueType), typeof(object) }));
            PAssert.That(() => typeof(SampleX<string>).BaseTypes().SequenceEqual(new[] { typeof(Sample<string>), typeof(SampleBase), typeof(ArrayList), typeof(object) }));
        }

        enum SampleEnum { }

        sealed class SampleClass { }

        struct SampleStruct { }

        [Fact]
        public void TestIsNullableValueType()
        {
            PAssert.That(() => typeof(int?).IsNullableValueType());
            PAssert.That(() => !typeof(int).IsNullableValueType());
            PAssert.That(() => typeof(SampleEnum?).IsNullableValueType());
            PAssert.That(() => !typeof(SampleEnum).IsNullableValueType());
            PAssert.That(() => !typeof(string).IsNullableValueType());
            PAssert.That(() => !typeof(SampleClass).IsNullableValueType());
            PAssert.That(() => !typeof(SampleStruct).IsNullableValueType());
            PAssert.That(() => typeof(SampleStruct?).IsNullableValueType());
        }

        [Fact]
        public void TestNullableGetter()
        {
            PAssert.That(() => typeof(int?).IfNullableGetNonNullableType() == typeof(int));
            PAssert.That(() => typeof(int).IfNullableGetNonNullableType() == null);
            PAssert.That(() => typeof(string).IfNullableGetNonNullableType() == null);
        }

        [Fact]
        public void TestNonGenericName()
        {
            PAssert.That(() => typeof(int?).GetNonGenericName() == "System.Nullable");
            PAssert.That(() => typeof(int).GetNonGenericName() == "System.Int32");
            PAssert.That(() => typeof(string).GetNonGenericName() == "System.String");
            PAssert.That(() => typeof(Func<Tuple<int, string>, bool>).GetNonGenericName() == "System.Func");
        }
    }
}
