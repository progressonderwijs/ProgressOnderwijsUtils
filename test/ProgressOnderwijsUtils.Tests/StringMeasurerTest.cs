using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class StringMeasurerTest
    {
        [Fact]
        public void SimpleSanityChecks()
        {
            PAssert.That(
                () =>
                    StringMeasurer.Instance.ElideIfNecessary("Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt hier!", 60).result
                    == "Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt …"
            );

            PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("123456789", 5).result == "123…");
            PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("0,123456789", 5).result == "0,12…");
            PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("1.024", 5).result == "1.024");
            PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("testjes", 5).result == "testj…");
            PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("empty.pdf", 5).result == "emp…");
            PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("wheee!", 5).result == "whe…");
            PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("wheee!\ntwolines!", 5).result == "whe…");
            PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("wheee!\ntwolines!", 10).result == "wheee!\ntw…");
        }
    }
}
