using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests;

public sealed class HtmlAttributesTest
{
    [Fact]
    public void HtmlAttributesUsedOnMultipleThreadsWorksAsIfUnshared()
    {
        const int attrListCount = 20;
        const int maxInitAttrListLength = 4;
        const int threadCount = 2;

        var attributeLists = Enumerable.Range(0, attrListCount)
            .Select(
                i =>
                    Enumerable.Range(0, i % (maxInitAttrListLength + 1))
                        .Aggregate(
                            HtmlAttributes.Empty,
                            (attrs, _) => attrs.Add("X", "value")
                        )
            )
            .ToArray();

        var perThreadLists = Enumerable.Repeat(0, threadCount).Select(_ => attributeLists.ToArray()).ToArray();

        var errors = new ConcurrentQueue<string>();

        using (var barrier = new Barrier(threadCount)) {
            var tasks = perThreadLists.Select(
                (lists, threadI) => Task.Factory.StartNew(
                    () => {
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
                    },
                    TaskCreationOptions.LongRunning
                )
            ).ToArray();

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

    static HtmlAttributes GetAttributes(IHtmlElement elem) //convenient due to explicit interface implementation
        => elem.Attributes;

    [Fact]
    public void AttributesCanBeEnumerated()
    {
        var div = _div._class("A")._id("B").Attribute("data-xyz", "C")._class("D");
        _ = div._class("X"); //this should not affect the enumeration.
        PAssert.That(() => GetAttributes(div).SequenceEqual(new[] { new HtmlAttribute("class", "A"), new HtmlAttribute("id", "B"), new HtmlAttribute("data-xyz", "C"), new HtmlAttribute("class", "D") }));
    }

    [Fact]
    public void ResetReturnsTheSameDataAsInitiallyEvenIfConcurrentlyModified()
    {
        var div = _div._class("A")._id("B").Attribute("data-xyz", "C")._class("D");
        var attributes = GetAttributes(div);
        div = div._class("X"); //this should not affect the enumeration.
        using var enumerator = attributes.GetEnumerator();

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
        PAssert.That(() => GetAttributes(div).SequenceEqual(new[] { new HtmlAttribute("class", "A"), new HtmlAttribute("id", "B"), new HtmlAttribute("data-xyz", "C"), new HtmlAttribute("class", "D"), new HtmlAttribute("class", "X"), new HtmlAttribute("class", "Y") }));
    }

    [Fact]
    public void IndexerExtractsUniquelyNamedAttr()
    {
        var div = _div._class("A")._id("B").Attribute("data-xyz", "C")._class("D");
        PAssert.That(() => GetAttributes(div)["id"] == "B");
    }

    [Fact]
    public void IndexerExtractsFirstOccurenceWhenAmbiguousLikeTheDom()
    {
        var div = _div._class("A")._id("B").Attribute("data-xyz", "C").Attribute("data-xyz", "!!!")._class("D");
        PAssert.That(() => GetAttributes(div)["data-xyz"] == "C");
    }

    [Fact]
    public void IndexerSupportsClassNameJoining()
    {
        var div = _div._class("A")._id("B").Attribute("data-xyz", "C").Attribute("data-xyz", "!!!")._class("D");
        PAssert.That(() => GetAttributes(div)["class"] == "A D");
    }

    [Fact]
    public void ClassesListTwoSeparatelyAppliedAttributes()
    {
        var div = _div._class("A")._id("B").Attribute("data-xyz", "C").Attribute("data-xyz", "!!!")._class("D");
        PAssert.That(() => GetAttributes(div).Classes().SequenceEqual(new[] { "A", "D" }));
    }

    [Fact]
    public void ClassesCombinesSpaceSeparatedNamesWithSeparatelyAppliedNames()
    {
        var div = _div._class("A X")._id("B")._class(" D  C E  ")._class("")._class("Y");
        PAssert.That(() => GetAttributes(div).Classes().SequenceEqual(new[] { "A", "X", "D", "C", "E", "Y" }));
    }

    [Fact]
    public void ClassesCanBeEmpty()
    {
        var div = _div.Attribute("data-xyz", "C").Attribute("data-xyz", "!!!")._id("D");
        PAssert.That(() => GetAttributes(div).Classes().None());
    }

    [Fact]
    public void HasClassCombinesSpaceSeparatedNamesWithSeparatelyAppliedNames()
    {
        var div = _div._class("A X")._id("B")._class(" D  C E  ")._class("")._class("Y");
        PAssert.That(() => GetAttributes(div).HasClass("X"));
        PAssert.That(() => GetAttributes(div).HasClass("Y"));
        PAssert.That(() => !GetAttributes(div).HasClass("bla"));
        PAssert.That(() => !GetAttributes(div).HasClass("a"));
    }

    [Fact]
    public void You_cannot_check_multiple_classes_in_one_call()
    {
        var div = _div._class("A X")._id("B")._class(" D  C E  ")._class("")._class("Y");
        PAssert.That(() => !GetAttributes(div).HasClass("A X"));
    }

    [Fact]
    public void TheEmptyClassIsNotPresent()
    {
        var div = _div._class("A X")._id("B")._class(" D  C E  ")._class("")._class("Y");
        PAssert.That(() => !GetAttributes(div).HasClass(""));
    }

    [Fact]
    public void HasClassReturnsFalseWhenThereIsNoClass()
    {
        var div = _div.Attribute("data-xyz", "C").Attribute("data-xyz", "!!!")._id("D");
        PAssert.That(() => !GetAttributes(div).HasClass("D"));
    }
}