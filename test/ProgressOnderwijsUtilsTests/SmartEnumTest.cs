using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Inschrijvingen;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;
using ValueUtils;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class SmartEnumTest : TestsWithBusinessConnection
    {
        struct MetaObjectWithNonNullableField : IMetaObject
        {
            public SmartStudiejaar PeriodeStudiejaarId { get; set; }
        }

        [Test]
        public void SmartEnum_can_be_read_using_QueryBuilder()
        {
            var result = SafeSql.SQL($@"
                    select top 1
                        psj.periodestudiejaarid
                    from periodestudiejaar psj
                    where 1=1
                        and psj.periodestudiejaarid = {SmartStudiejaar.C2015}
                ").ReadMetaObjects<MetaObjectWithNonNullableField>(conn).Single();

            PAssert.That(() => result.PeriodeStudiejaarId == SmartStudiejaar.C2015);
        }

        struct MetaObjectWithNullableField : IMetaObject
        {
            public SmartStudiejaar? PeriodeStudiejaarId { get; set; }
        }

        [Test]
        public void Nullable_SmartEnum_can_be_read_using_QueryBuilder()
        {
            var result = SafeSql.SQL($@"
                    select top 1
                        psj.periodestudiejaarid
                    from periodestudiejaar psj
                    where 1=1
                        and psj.periodestudiejaarid = {SmartStudiejaar.C2015}
                ").ReadMetaObjects<MetaObjectWithNullableField>(conn).Single();

            PAssert.That(() => result.PeriodeStudiejaarId == SmartStudiejaar.C2015);
        }

        sealed class TestEnum : ISmartEnum
        {
            public int Id { get; private set; }
            public ITranslatable Text { get; private set; }

            [SmartEnumMember]
            public static readonly TestEnum A = new TestEnum { Id = 0, Text = Translatable.Raw("A") };

            [SmartEnumMember]
            public static readonly TestEnum B = new TestEnum { Id = 1, Text = Translatable.Raw("B") };
        }

        [Test]
        public void SmartEnum_GetValues_returns_all_values()
        {
            PAssert.That(() => SmartEnum.GetValues<TestEnum>().SetEqual(new[] { TestEnum.A, TestEnum.B, }));
        }

        [Test]
        public void SmartEnum_GetId_returns_correct_value()
        {
            PAssert.That(() => SmartEnum.GetById<TestEnum>(1) == TestEnum.B);
        }

        sealed class BrokenEnum : ISmartEnum
        {
            public int Id { get; private set; }
            public ITranslatable Text { get; private set; }

            [SmartEnumMember]
            public static readonly BrokenEnum A = new BrokenEnum { Id = 0, Text = Translatable.Raw("A") };

            [SmartEnumMember]
            public static readonly BrokenEnum B = new BrokenEnum { Id = 0, Text = Translatable.Raw("B") };
        }

        [Test]
        public void SmartEnums_cannot_have_duplicate_ids()
        {
            Assert.Throws<TypeInitializationException>(() => SmartEnum.GetValues<BrokenEnum>());
        }
    }

    public sealed class AllSmartEnumsTest
    {
        static readonly Type[] allSmartEnumTypes = typeof(BusinessAssembly).Assembly
            .GetTypes()
            .Where(typeof(ISmartEnum).IsAssignableFrom)
            .ToArray();

        static bool InvokeGenericCheck(string name, Type type)
        {
            var method = typeof(AllSmartEnumsTest)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(m => m.Name == name && m.IsGenericMethod)
                .MakeGenericMethod(type);

            return (bool)method.Invoke(null, null);
        }

        [Test]
        public void All_SmartEnums_implement_equals_correctly()
        {
            var smartEnumTypesThatImplementEqualsIncorrectly = allSmartEnumTypes
                .Where(t => !InvokeGenericCheck(nameof(AllSmartEnumMembersAreOnlyEqualToThemselves), t));

            PAssert.That(() => smartEnumTypesThatImplementEqualsIncorrectly.None());
        }

        [UsedImplicitly]
        static bool AllSmartEnumMembersAreOnlyEqualToThemselves<T>()
            where T : ISmartEnum
        {
            // ReSharper disable once EqualExpressionComparison
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return SmartEnum
                .GetValues<T>()
                .All(v1 => Equals(v1, v1) && SmartEnum.GetValues<T>().Count(v2 => Equals(v1, v2)) == 1);
        }

        [Test]
        public void All_serializable_SmartEnums_deserialize_correctly()
        {
            var smartEnumsTypesThatDeserializeIncorrectly = allSmartEnumTypes
                .Where(t => t.GetCustomAttribute<SerializableAttribute>() != null)
                .Where(t => !InvokeGenericCheck(nameof(AllSmartEnumMembersRemainEqualAfterDeserialization), t));

            PAssert.That(() => smartEnumsTypesThatDeserializeIncorrectly.None());
        }

        [UsedImplicitly]
        static bool AllSmartEnumMembersRemainEqualAfterDeserialization<T>()
            where T : ISmartEnum
        {
            return SmartEnum
                .GetValues<T>()
                .Select(v => new { Original = v, Deserialized = (T)SerializeAndDeserialize(v) })
                .All(vals => Equals(vals.Original, vals.Deserialized) &&
                    FieldwiseEquality.AreEqual(vals.Original, vals.Deserialized) /*Test fieldwise in case Equals only checks id*/);
        }

        static object SerializeAndDeserialize(object obj)
        {
            using (var stream = new MemoryStream()) {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                return formatter.Deserialize(stream);
            }
        }
    }
}
