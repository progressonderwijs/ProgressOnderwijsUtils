﻿using System;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Inschrijvingen;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class SmartEnumTest : TestsWithBusinessConnection
    {
        struct Inschrijving : IMetaObject
        {
            public SmartPeriodeStudiejaar PeriodeStudiejaar { get; set; }
        }

        [Test]
        public void SmartEnum_can_be_read_using_QueryBuilder()
        {
            var inschrijving = SafeSql.SQL($@"
                    select top 1
                        si.periodestudiejaar
                    from studentinschrijving si
                    where 1=1
                        and si.periodestudiejaar = {SmartPeriodeStudiejaar.C2015}
                ").ReadMetaObjects<Inschrijving>(conn).Single();

            PAssert.That(() => inschrijving.PeriodeStudiejaar == SmartPeriodeStudiejaar.C2015);
        }

        sealed class TestEnum : SmartEnum
        {
            TestEnum(int id, ITranslatable text)
                : base(id, text) { }

            public static readonly TestEnum A = new TestEnum(0, Translatable.Raw("A"));
            public static readonly TestEnum B = new TestEnum(1, Translatable.Raw("B"));
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

        sealed class BrokenEnum : SmartEnum
        {
            BrokenEnum(int id, ITranslatable text)
                : base(id, text) { }

            // ReSharper disable UnusedMember.Local
            public static readonly BrokenEnum A = new BrokenEnum(0, Translatable.Raw("A"));
            public static readonly BrokenEnum B = new BrokenEnum(0, Translatable.Raw("B"));
            // ReSharper restore UnusedMember.Local
        }

        [Test]
        public void SmartEnums_cannot_have_duplicate_ids()
        {
            Assert.Throws<TypeInitializationException>(() => SmartEnum.GetValues<BrokenEnum>());
        }
    }
}
