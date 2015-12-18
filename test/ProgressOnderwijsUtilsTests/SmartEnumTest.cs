using System;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class SmartEnumTest : TestsWithBusinessConnection
    {
        sealed class TestEnum : ISmartEnum
        {
            public int Id { get; private set; }

            [SmartEnumMember]
            public static readonly TestEnum A = new TestEnum { Id = 0 };

            [SmartEnumMember]
            public static readonly TestEnum B = new TestEnum { Id = 1 };
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

            [SmartEnumMember]
            public static readonly BrokenEnum A = new BrokenEnum { Id = 0 };

            [SmartEnumMember]
            public static readonly BrokenEnum B = new BrokenEnum { Id = 0 };
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
    }
}
