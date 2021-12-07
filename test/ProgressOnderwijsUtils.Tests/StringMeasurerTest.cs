using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests;

public sealed class StringMeasurerTest
{
    [Fact]
    public void SimpleSanityChecks()
    {
        var progressScaling = 0.638;
        PAssert.That(
            () =>
                StringMeasurer.Instance.ElideIfNecessary("Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt hier!", 60 * progressScaling).result
                == "Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt …"
        );

        PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("123456789", 5 * progressScaling).result == "123…");
        PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("0,123456789", 5 * progressScaling).result == "0,12…");
        PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("1.024", 5 * progressScaling).result == "1.024");
        PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("testjes", 5 * progressScaling).result == "testj…");
        PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("empty.pdf", 5 * progressScaling).result == "emp…");
        PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("wheee!", 5 * progressScaling).result == "whe…");
        PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("wheee!\ntwolines!", 5 * progressScaling).result == "whe…");
        PAssert.That(() => StringMeasurer.Instance.ElideIfNecessary("wheee!\ntwolines!", 10 * progressScaling).result == "wheee!\ntw…");
    }
}