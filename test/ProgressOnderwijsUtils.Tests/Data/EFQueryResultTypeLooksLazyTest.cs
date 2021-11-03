using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;
using Xunit;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class EFQueryResultTypeLooksLazyTest
    {
        [Fact]
        public void AFewExamplesAreCorrectlyMarkedForLaziness()
        {
            PAssert.That(() => EFQueryResultTypeLooksLazy<IEnumerable>.IsLazyQueryResultType);
            PAssert.That(() => EFQueryResultTypeLooksLazy<IEnumerable<int>>.IsLazyQueryResultType);
            PAssert.That(() => EFQueryResultTypeLooksLazy<DbDataReader>.IsLazyQueryResultType);
            PAssert.That(() => EFQueryResultTypeLooksLazy<ReadOnlyCollectionBase>.IsLazyQueryResultType, "This is probably not lazy, but might be depending on subclass, so we should err on the side of caution and detect it as lazy");
            PAssert.That(() => EFQueryResultTypeLooksLazy<StringCollection>.IsLazyQueryResultType, "This is not lazy, but an expected false positive due to an algorithmic limitation (and we prefer false positive to false negatives)");
            PAssert.That(() => EFQueryResultTypeLooksLazy<LinkedList<string>>.IsLazyQueryResultType, "This is not lazy, but an expected false positive due to an algorithmic limitation (and we prefer false positive to false negatives)");

            PAssert.That(() => EFQueryResultTypeLooksLazy<string>.IsLazyQueryResultType == false);
            PAssert.That(() => EFQueryResultTypeLooksLazy<DayOfWeek>.IsLazyQueryResultType == false);
            PAssert.That(() => EFQueryResultTypeLooksLazy<byte[]>.IsLazyQueryResultType == false);
            PAssert.That(() => EFQueryResultTypeLooksLazy<int>.IsLazyQueryResultType == false);
            PAssert.That(() => EFQueryResultTypeLooksLazy<IWrittenImplicitly>.IsLazyQueryResultType == false);
            PAssert.That(() => EFQueryResultTypeLooksLazy<List<int>>.IsLazyQueryResultType == false);
            PAssert.That(() => EFQueryResultTypeLooksLazy<int[]>.IsLazyQueryResultType == false);
            PAssert.That(() => EFQueryResultTypeLooksLazy<Dictionary<string, string>>.IsLazyQueryResultType == false);
            PAssert.That(() => EFQueryResultTypeLooksLazy<DistinctArray<string>>.IsLazyQueryResultType == false);
        }
    }
}
