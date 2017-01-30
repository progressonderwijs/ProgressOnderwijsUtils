﻿using System;
using System.Globalization;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class ToFixedPointTest
    {
        static readonly CultureInfo INV = CultureInfo.InvariantCulture;
        static readonly CultureInfo NL = CultureInfo.GetCultureInfo("nl");
        static readonly CultureInfo BE = CultureInfo.GetCultureInfo("nl-BE"); //in belgie is "NaN" "NaN (geen getal)" dus das een mooie corner case

        [Test]
        public void ToFixedPointWorksLikeFormatter()
        {
            foreach (var culture in new[] { NL, INV }) {
                foreach (var scale in Enumerable.Range(0, 4).Select(p => Math.Pow(10, -p))) {
                    for (var n = -10000; n < 10000; n++) {
                        if ((n % 10 + 10) % 10 != 5) {
                            for (var precision = 0; precision < 4; precision++) {
                                var x = n * scale;

                                if (!(Utils.ToFixedPointString(x, culture, precision) == x.ToString("f" + precision, culture))) {
                                    PAssert.That(() => Utils.ToFixedPointString(n * scale, culture, precision) == x.ToString("f" + precision, culture));
                                }
                            }
                        }
                    }
                }
            }
        }

        [Test]
        
        public void FixedPointOmitsMinusForZero()
        {
            PAssert.That(() => Utils.ToFixedPointString(-0.1, INV, 0) == "0");

            PAssert.That(() => Utils.ToFixedPointString(-double.Epsilon, INV, 1) == "0.0");
            PAssert.That(() => Utils.ToFixedPointString(-double.Epsilon, INV, 2) == "0.00");
        }

        [Test]
        
        public void WorksOnNonFiniteNumbers()
        {
            PAssert.That(() => Utils.ToFixedPointString(double.NaN, NL, 0) == double.NaN.ToString("f0", NL));
            PAssert.That(() => Utils.ToFixedPointString(double.NaN, BE, 0) == double.NaN.ToString("f0", BE));
            PAssert.That(() => Utils.ToFixedPointString(double.PositiveInfinity, INV, 1) == double.PositiveInfinity.ToString("f0", INV));
            PAssert.That(() => Utils.ToFixedPointString(double.NegativeInfinity, INV, 2) == double.NegativeInfinity.ToString("f0", INV));
            PAssert.That(() => Utils.ToFixedPointString(double.PositiveInfinity, BE, 1) == double.PositiveInfinity.ToString("f0", BE));
            PAssert.That(() => Utils.ToFixedPointString(double.NegativeInfinity, BE, 2) == double.NegativeInfinity.ToString("f0", BE));
        }

        [Test]
        
        public void WorksOnCornerCases()
        {
            var edgeCase = ulong.MaxValue - 1024;

            Func<double, double, bool> approxEqual = (a, b) => Math.Abs(a - b) / (Math.Abs(a) + Math.Abs(b)) < 1e-14;

            PAssert.That(() => double.Parse(Utils.ToFixedPointString(edgeCase, NL, 0), NumberStyles.AllowDecimalPoint, NL) == (double)edgeCase);
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
