using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Filters;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public class FilterFactoryWildcardSearchTest
    {
        public class ExampleRow : IMetaObject
        {
            public string Text { get; set; }
        }

        static FilterFactory<ExampleRow>.FilterCreator<string> FilterFactory()
            => new FilterFactory<ExampleRow>().FilterOn(o => o.Text);

        [Test]
        public void WithoutWildcardsMeansEqual()
        {
            PAssert.That(() =>
                Equals(FilterFactory().WildcardSearch("needle"), FilterFactory().Equal("needle"))
                );
        }

        [Test]
        public void WildcardSuffixMeansStartsWith()
        {
            PAssert.That(() =>
                Equals(FilterFactory().WildcardSearch("needle*"), FilterFactory().StartsWith("needle"))
                );
        }

        [Test]
        public void WildcardPrefixMeansEndsWith()
        {
            PAssert.That(() =>
                Equals(FilterFactory().WildcardSearch("*needle"), FilterFactory().EndsWith("needle"))
                );
        }

        [Test]
        public void SuffixAndPrefixMeansContains()
        {
            PAssert.That(() =>
                Equals(FilterFactory().WildcardSearch("*needle*"), FilterFactory().Contains("needle"))
                );
        }

        [Test]
        public void UnnecessaryStarsAreTrimmed()
        {
            PAssert.That(() =>
                Equals(FilterFactory().WildcardSearch("***needle*****"), FilterFactory().Contains("needle"))
                );
        }

        [Test]
        public void StarsInTheMiddleArePlainContent()
        {
            PAssert.That(() =>
                Equals(FilterFactory().WildcardSearch("a*needle*b"), FilterFactory().Equal("a*needle*b"))
                );
        }
    }
}
