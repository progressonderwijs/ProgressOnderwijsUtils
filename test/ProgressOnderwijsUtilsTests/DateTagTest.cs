using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class DateTagTest
    {
        [Fact]
        public void NoCollisionsSoon()
        {
            var start = DateTime.Now; //slightly evil in unit test;
            var seconds = (int)TimeSpan.FromDays(162).TotalSeconds;
            var dates = Enumerable.Range(0, seconds).Select(s => start + TimeSpan.FromSeconds(s));
            PAssert.That(() => dates.Count() > 10000000);
            PAssert.That(() => !dates.Select(DateTimeShortAgeTag.ToAgeTagCaseInsensitive).ContainsDuplicates());
            PAssert.That(() => !dates.Select(DateTimeShortAgeTag.ToAgeTag).ContainsDuplicates());
        }
    }
}
