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
        [Test]
        [PullRequestTest]
        public void Do_not_change_flowstep_too_much()
        {
            // dit veranderen is vervelend voor instellingen, ze moeten dan (onterechte) koppelingen aan excel repareren ed
            var flowSteps = EnumHelpers
                .GetValues<FlowStep>()
                .Select(flowStep => Converteer.ToString(flowStep, Taal.NL));
            PAssert.That(() => flowSteps.SequenceEqual(new[] { "Gedeeltelijk", "Afgewezen", "Open", "OK" }));
        }
    }
}
