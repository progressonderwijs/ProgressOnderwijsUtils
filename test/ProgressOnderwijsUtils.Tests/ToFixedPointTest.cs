using System;
using System.Globalization;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class ToFixedPointTest
    {
        static readonly CultureInfo INV = CultureInfo.InvariantCulture;
        static readonly CultureInfo NL = CultureInfo.GetCultureInfo("nl");
        static readonly CultureInfo BE = CultureInfo.GetCultureInfo("nl-BE"); //in belgie is "NaN" "NaN (geen getal)" dus das een mooie corner case

        [Fact]
        public void ToFixedPointWorksLikeFormatter()
        {
            foreach (var culture in new[] { NL, INV }) {
                foreach (var scale in Enumerable.Range(0, 4).Select(p => Math.Pow(10, -p))) {
                    for (var n = -10000; n < 10000; n++) {
                        if ((n % 10 + 10) % 10 != 5) {
                            for (var precision = 0; precision < 4; precision++) {
                                var x = n * scale;

                                if (Utils.ToFixedPointString(x, culture, precision) != SimpleToFixedPointString(x, culture, precision)) {
                                    PAssert.That(() => Utils.ToFixedPointString(x, culture, precision) == SimpleToFixedPointString(x, culture, precision));
                                }
                            }
                        }
                    }
                }
            }
        }

        static string SimpleToFixedPointString(double x, CultureInfo culture, int precision)
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            => (Math.Round(x, precision, MidpointRounding.AwayFromZero) is var rounded && rounded != 0 ? rounded : 0).ToString("f" + precision, culture);

        [Fact]
        public void FixedPointOmitsMinusForZero()
        {
            PAssert.That(() => Utils.ToFixedPointString(-0.1, INV, 0) == "0");

            PAssert.That(() => Utils.ToFixedPointString(-double.Epsilon, INV, 1) == "0.0");
            PAssert.That(() => Utils.ToFixedPointString(-double.Epsilon, INV, 2) == "0.00");
        }

        [Fact]
        public void WorksOnNonFiniteNumbers()
        {
            PAssert.That(() => Utils.ToFixedPointString(double.NaN, NL, 0) == double.NaN.ToString("f0", NL));
            PAssert.That(() => Utils.ToFixedPointString(double.NaN, BE, 0) == double.NaN.ToString("f0", BE));
            PAssert.That(() => Utils.ToFixedPointString(double.PositiveInfinity, INV, 1) == double.PositiveInfinity.ToString("f0", INV));
            PAssert.That(() => Utils.ToFixedPointString(double.NegativeInfinity, INV, 2) == double.NegativeInfinity.ToString("f0", INV));
            PAssert.That(() => Utils.ToFixedPointString(double.PositiveInfinity, BE, 1) == double.PositiveInfinity.ToString("f0", BE));
            PAssert.That(() => Utils.ToFixedPointString(double.NegativeInfinity, BE, 2) == double.NegativeInfinity.ToString("f0", BE));
        }

        [Fact]
        public void WorksOnCornerCases()
        {
            var edgeCase = ulong.MaxValue - 1024;

            Func<double, double, bool> approxEqual = (a, b) => Math.Abs(a - b) / (Math.Abs(a) + Math.Abs(b)) < 1e-14;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            PAssert.That(() => double.Parse(Utils.ToFixedPointString(edgeCase, NL, 0), NumberStyles.AllowDecimalPoint, NL) == edgeCase);
            PAssert.That(
                () => approxEqual(double.Parse(Utils.ToFixedPointString(edgeCase + 1, NL, 0), NumberStyles.AllowDecimalPoint, NL), edgeCase + 1));
            PAssert.That(
                () =>
                    approxEqual(
                        double.Parse(Utils.ToFixedPointString(edgeCase - 1, NL, 0), NumberStyles.AllowDecimalPoint, NL),
                        double.Parse(((double)(edgeCase - 1)).ToString("f0", NL), NumberStyles.AllowDecimalPoint, NL)));
        }
    }
}
