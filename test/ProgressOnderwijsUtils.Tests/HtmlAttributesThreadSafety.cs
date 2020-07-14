using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Html;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class HtmlAttributesThreadSafety
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
                            errors.Enqueue($"{threadI} / attrList[{i}]: expected {initLength + 2} attrs, have {attrList.Count}; names: {string.Join(", ", attrList.Select(attr => attr.Name))}");
                        }
                    }
                }, TaskCreationOptions.LongRunning)).ToArray();

                Task.WaitAll(tasks);
            }
            PAssert.That(() => errors.None());
        }
    }
}
