using System;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class FlowStepTest
    {
        [Test, Continuous]
        public void String_representation_corresponds_to_integer_representation()
        {
            var flowStepsWhoseStringIsNotTheInteger = EnumHelpers
                .GetValues<FlowStep>()
                .Where(flowStep => Converteer.ToString(flowStep, Taal.NL) != Converteer.ToString((int)flowStep, Taal.NL));
            PAssert.That(() => flowStepsWhoseStringIsNotTheInteger.None());
        }
    }
}
