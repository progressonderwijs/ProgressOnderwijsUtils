using System;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class GenericExtensionsFriday
    {
        [Fact]
        public void InStruct()
        {
            PAssert.That(() => DayOfWeek.Monday.In(DayOfWeek.Tuesday, DayOfWeek.Monday));
            PAssert.That(() => !DayOfWeek.Monday.In(DayOfWeek.Tuesday, DayOfWeek.Friday));
            PAssert.That(() => !default(DayOfWeek?).In(DayOfWeek.Tuesday, DayOfWeek.Friday));
            PAssert.That(() => default(DayOfWeek?).In(DayOfWeek.Tuesday, DayOfWeek.Friday, null));

            PAssert.That(() => 3.In(1, 2, 3));
            PAssert.That(() => !3.In(1, 2, 4, 8));
        }
    }
}
