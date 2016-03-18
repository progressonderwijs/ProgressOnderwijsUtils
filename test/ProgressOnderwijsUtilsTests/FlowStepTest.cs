using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using Progress.Business.Text;
using Progress.Business.Tools;
using ProgressOnderwijsUtils;

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
            // Instellingen hechten hier waarde aan, zie bv https://uocg.fogbugz.com/f/cases/15882
        }
    }
}
