using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class DistinctArrayTest
    {
        [Test]
        public void Created_DistinctArray_is_distinct()
        {
            var sut = new DistinctArray<int>(new [] { 1, 1, 2});

            PAssert.That(() => sut.Count == 2);
            PAssert.That(() => sut.SetEqual(new[] { 1, 2 }));
        }

        [Test]
        public void ToDistinctArray_is_distinct()
        {
            var sut = new[] { 1, 1, 2 }.ToDistinctArray();

            PAssert.That(() => sut.Count == 2);
            PAssert.That(() => sut.SetEqual(new[] { 1, 2 }));
        }

        [Test]
        public void Indexing_all_items_gives_equals_set()
        {
            var sut = new DistinctArray<int>(new[] { 1, 1, 2 });

            PAssert.That(() => sut.Select((item, i) => sut[i]).SetEqual(sut));
        }
    }
}
