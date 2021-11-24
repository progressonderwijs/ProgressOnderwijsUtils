using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class PropertyMappingTest
    {
        sealed record TestObject : ICopyable<TestObject>, IWrittenImplicitly
        {
            public DayOfWeek EnumIntProperty { get; set; }
            public DateTimeKind Kind { get; set; }
            public string? Unused { get; set; }
        }

        [Fact]
        public void MapProperties_werkt_voor_none_mappers()
        {
            var objects = new[] {
                new TestObject {
                    EnumIntProperty = DayOfWeek.Wednesday,
                },
            };

            var mappers = new PropertyMappers();
            var mapped = mappers.Map(objects);

            PAssert.That(() => mapped.Single().EnumIntProperty == DayOfWeek.Wednesday);
        }

        [Fact]
        public void MapProperties_werkt_voor_null_mapper()
        {
            var objects = new[] {
                new TestObject {
                    EnumIntProperty = DayOfWeek.Wednesday,
                },
            };

            var mappers = PropertyMapper.CreateForDictionary(new Dictionary<StringComparison, StringComparison>());
            var mapped = mappers.Map(objects);

            PAssert.That(() => mapped.Single().EnumIntProperty == DayOfWeek.Wednesday);
        }

        [Fact]
        public void MapProperties_werkt_voor_single_mapper()
        {
            var objects = new[] {
                new TestObject {
                    EnumIntProperty = DayOfWeek.Wednesday,
                },
            };

            var mapped = PropertyMapper.CreateForDictionary(new Dictionary<DayOfWeek, DayOfWeek> { [DayOfWeek.Wednesday] = DayOfWeek.Thursday, }).Map(objects);

            PAssert.That(() => mapped.Single().EnumIntProperty == DayOfWeek.Thursday);
            PAssert.That(() => objects.Single().EnumIntProperty == DayOfWeek.Wednesday);
        }

        [Fact]
        public void MapProperties_Identity_map_doet_niks()
        {
            var objects = new[] {
                new TestObject {
                    EnumIntProperty = DayOfWeek.Wednesday,
                },
            };

            var mapped = PropertyMapper.CreateForIdentityMap<DayOfWeek>().Map(objects);

            PAssert.That(() => objects.SequenceEqual(mapped));
        }

        [Fact]
        public void MapProperties_CreateForValue_ignores_input()
        {
            var objects = new[] {
                new TestObject { Kind = DateTimeKind.Utc, EnumIntProperty = DayOfWeek.Thursday },
                new TestObject { Kind = DateTimeKind.Local, EnumIntProperty = DayOfWeek.Friday, },
                new TestObject { Kind = DateTimeKind.Unspecified, EnumIntProperty = DayOfWeek.Monday, },
            };
            var expected = new[] {
                new TestObject { Kind = DateTimeKind.Unspecified, EnumIntProperty = DayOfWeek.Thursday },
                new TestObject { Kind = DateTimeKind.Unspecified, EnumIntProperty = DayOfWeek.Friday, },
                new TestObject { Kind = DateTimeKind.Unspecified, EnumIntProperty = DayOfWeek.Monday, },
            };

            var mapped = PropertyMapper.CreateForValue(DateTimeKind.Unspecified).Map(objects);

            PAssert.That(() => mapped.SequenceEqual(expected));
        }

        [Fact]
        public void MapProperties_werkt_niet_voor_multiple_mappers_van_zelfde_type()
        {
            var mappers = PropertyMapper.CreateForDictionary(new Dictionary<DayOfWeek, DayOfWeek> { [DayOfWeek.Wednesday] = DayOfWeek.Thursday, });

            _ = Assert.Throws<ArgumentException>(() => mappers.CloneWithExtraMappers(PropertyMapper.CreateForDictionary(new Dictionary<DayOfWeek, DayOfWeek> { [DayOfWeek.Thursday] = DayOfWeek.Friday, })));
        }

        sealed record NullableTestObject : ICopyable<NullableTestObject>, IWrittenImplicitly
        {
            public DayOfWeek? NullableProperty { get; set; }
        }

        [Fact]
        public void MapProperties_werkt_ook_voor_nullable_property_met_waarde()
        {
            var objects = new[] {
                new NullableTestObject {
                    NullableProperty = DayOfWeek.Wednesday,
                },
            };

            var mapped = PropertyMapper.CreateForDictionary(new Dictionary<DayOfWeek, DayOfWeek> { [DayOfWeek.Wednesday] = DayOfWeek.Thursday, }).Map(objects);

            PAssert.That(() => mapped.Single().NullableProperty == DayOfWeek.Thursday);
        }

        [Fact]
        public void MapProperties_werkt_ook_voor_nullable_property_met_null_waarde()
        {
            var objects = new[] {
                new NullableTestObject {
                    NullableProperty = null,
                },
            };

            var mapped = PropertyMapper.CreateForDictionary(new Dictionary<DayOfWeek, DayOfWeek> { [DayOfWeek.Wednesday] = DayOfWeek.Thursday, }).Map(objects);

            PAssert.That(() => mapped.Single().NullableProperty == null);
        }

        [Fact]
        public void MapProperties_werkt_ook_voor_nullable_property_met_waarde_die_naar_null_gemapped_moet_worden()
        {
            var objects = new[] {
                new NullableTestObject {
                    NullableProperty = DayOfWeek.Wednesday,
                },
            };

            var mappers = PropertyMapper.CreateForValue(default(DayOfWeek?));
            var mapped = mappers.Map(objects);

            PAssert.That(() => mapped.Single().NullableProperty == null);
        }

        [Fact]
        public void MapProperties_werkt_voor_multiple_types()
        {
            var objects = new[] {
                new TestObject {
                    EnumIntProperty = DayOfWeek.Wednesday,
                    Unused = "as",
                    Kind = DateTimeKind.Local,
                },
                new TestObject {
                    EnumIntProperty = DayOfWeek.Monday,
                    Unused = "X",
                    Kind = DateTimeKind.Unspecified,
                },
            };
            var copy = objects.ArraySelect(o => o with { });

            var mapped = PropertyMapper
                .CreateForFunc((DayOfWeek day) => (DayOfWeek)(((int)day + 1) % 7))
                .CloneWithExtraMappers(PropertyMapper.CreateForFunc((DateTimeKind kind) => (DateTimeKind)(((int)kind + 2) % 3)))
                .Map(objects);

            PAssert.That(() => objects.SequenceEqual(copy), "Original objects should not be changed");

            var expected = new[] {
                new TestObject {
                    EnumIntProperty = DayOfWeek.Thursday,
                    Unused = "as",
                    Kind = DateTimeKind.Utc,
                },
                new TestObject {
                    EnumIntProperty = DayOfWeek.Tuesday,
                    Unused = "X",
                    Kind = DateTimeKind.Local,
                },
            };

            PAssert.That(() => mapped.SequenceEqual(expected));
        }

    }
}
