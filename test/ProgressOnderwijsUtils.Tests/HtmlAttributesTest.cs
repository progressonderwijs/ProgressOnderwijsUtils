using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Html;
using Xunit;
using static ProgressOnderwijsUtils.Html.Tags;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class HtmlAttributesTest
    {
        [Fact]
        public void HtmlAttributesUsedOnMultipleThreadsWorksAsIfUnshared()
        {
            const int attrListCount = 20;
            const int maxInitAttrListLength = 4;
            const int threadCount = 2;

            var attributeLists = Enumerable.Range(0, attrListCount)
                .Select(i =>
                    Enumerable.Range(0, i % (maxInitAttrListLength + 1))
                        .Aggregate(HtmlAttributes.Empty,
                            (attrs, k) => attrs.Add("X", "value")
                        )
                )
                .ToArray();

            var perThreadLists = Enumerable.Repeat(0, threadCount).Select(_ => attributeLists.ToArray()).ToArray();

            var errors = new ConcurrentQueue<string>();

            using (var barrier = new Barrier(threadCount)) {
                var tasks = perThreadLists.Select((lists, threadI) => Task.Factory.StartNew(() => {
                    barrier.SignalAndWait();
                    var threadName = "thread" + threadI;
                    for (var i = 0; i < lists.Length; i++) {
                        lists[i] = lists[i]
                                .Add(threadName, "value")
                                .Add(threadName, "value")
                            ;
                    }
                    barrier.SignalAndWait();
                    for (var i = 0; i < lists.Length; i++) {
                        var attrList = lists[i];
                        var initLength = i % (maxInitAttrListLength + 1);

                        if (attrList.Count != initLength + 2
                            || !attrList.Take(initLength).All(attr => attr.Name == "X" && attr.Value == "value")
                            || !attrList.Skip(initLength).All(attr => attr.Name == threadName && attr.Value == "value")) {
                            errors.Enqueue($"{threadI} / attrList[{i}]: expected {initLength + 2} attrs, have {attrList.Count}; names: {attrList.Select(attr => attr.Name).JoinStrings(", ")}");
                        }
                    }
                }, TaskCreationOptions.LongRunning)).ToArray();

                Task.WaitAll(tasks);
            }
            PAssert.That(() => errors.None());
        }

        [Fact]
        public void EqualityMembersWork()
        {
            AssertEquality(new HtmlAttribute("class", "A"), new HtmlAttribute("id", "B"), false);
            AssertEquality(new HtmlAttribute("class", "A"), new HtmlAttribute("class", "B"), false);
            AssertEquality(new HtmlAttribute("class", "A"), new HtmlAttribute("id", "A"), false);
            AssertEquality(new HtmlAttribute("class", "A"), new HtmlAttribute("class", "A"), true);
        }

        static void AssertEquality(HtmlAttribute attr1, HtmlAttribute attr2, bool expectedEquality)
        {
            PAssert.That(() => attr1 != attr2 != expectedEquality);
            PAssert.That(() => attr1 == attr2 == expectedEquality);
            PAssert.That(() => attr1.Equals(attr2) == expectedEquality);
            PAssert.That(() => attr1.GetHashCode() == attr2.GetHashCode() == expectedEquality);
            PAssert.That(() => attr1.Equals((object)attr2) == expectedEquality);
        }

        [Fact]
        public void AttributesCanBeEnumerated()
        {
            var div = _div._class("A")._id("B").Attribute("data-xyz", "C")._class("D");
            IHtmlElement elem = div;
            _ = div._class("X"); //this should not affect the enumeration.
            PAssert.That(() => elem.Attributes.SequenceEqual(new[] { new HtmlAttribute("class", "A"), new HtmlAttribute("id", "B"), new HtmlAttribute("data-xyz", "C"), new HtmlAttribute("class", "D") }));
        }

        [Fact]
        public void ResetReturnsTheSameDataAsInitiallyEvenIfConcurrentlyModified()
        {
            var div = _div._class("A")._id("B").Attribute("data-xyz", "C")._class("D");
            IHtmlElement elem = div;
            div = div._class("X"); //this should not affect the enumeration.
            using var enumerator = elem.Attributes.GetEnumerator();

            PAssert.That(() => enumerator.MoveNext());
            PAssert.That(() => enumerator.Current == new HtmlAttribute("class", "A"));
            div = div._class("Y"); //this should not affect the enumeration.
            PAssert.That(() => enumerator.MoveNext());
            PAssert.That(() => enumerator.Current == new HtmlAttribute("id", "B"));
            enumerator.Reset();
            var attrsPostReset = new List<HtmlAttribute>();
            while (enumerator.MoveNext()) {
                attrsPostReset.Add(enumerator.Current);
            }
            PAssert.That(() => attrsPostReset.SequenceEqual(new[] { new HtmlAttribute("class", "A"), new HtmlAttribute("id", "B"), new HtmlAttribute("data-xyz", "C"), new HtmlAttribute("class", "D") }));
            var attrsOfConcurrentlyModifiedVariable = (div as IHtmlElement).Attributes;
            PAssert.That(() => attrsOfConcurrentlyModifiedVariable.SequenceEqual(new[] { new HtmlAttribute("class", "A"), new HtmlAttribute("id", "B"), new HtmlAttribute("data-xyz", "C"), new HtmlAttribute("class", "D"), new HtmlAttribute("class", "X"), new HtmlAttribute("class", "Y") }));
        }
    }
}
