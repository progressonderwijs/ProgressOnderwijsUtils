using System;
using System.Xml.Linq;

namespace ProgressOnderwijsUtils
{
    public enum FlowStep
    {
        [MpLabelUntranslated("-2")]
        Partial = -2,

        [MpLabelUntranslated("-1")]
        Rejected = -1,

        [MpLabelUntranslated("0")]
        CanEdit = 0,

        [MpLabelUntranslated("1")]
        Ok = 1,
    }

    public static class HtmlHelper
    {
        static XhtmlData Symbol(string cssClass, string toolTip)
        {
            var hasToolTip = !string.IsNullOrWhiteSpace(toolTip);
            return XhtmlData.Create(
                new XElement("span",
                    new XAttribute("class", cssClass + (hasToolTip ? " hastooltip undashed" : "")),
                    hasToolTip ? new XAttribute("data-tiptext", toolTip) : null));
        }

        public static XhtmlData CheckmarkGreenNoBorderSymbol() => CheckmarkGreenNoBorderSymbol(null);
        public static XhtmlData CheckmarkGreenNoBorderSymbol(string toolTip) => Symbol("CheckmarkGreenNoBorderSymbol", toolTip);
        public static XhtmlData QuestionRedSymbol(string toolTip) => Symbol("QuestionRedSymbol", toolTip);
        public static XhtmlData CrossRedSymbol(string toolTip) => Symbol("CrossRedSymbol", toolTip);
        public static XhtmlData CrossRedLargerSymbol(string toolTip) => Symbol("CrossRedSymbol LargerSymbol", toolTip);
        public static XhtmlData CrossGreenSymbol(string toolTip) => Symbol("CrossGreenSymbol", toolTip);
        public static XhtmlData ExclamationGreenSymbol(string toolTip) => Symbol("ExclamationGreenSymbol", toolTip);
        public static XhtmlData ExclamationRedSymbol() => ExclamationRedSymbol(null);
        public static XhtmlData ExclamationRedSymbol(string toolTip) => Symbol("ExclamationRedSymbol", toolTip);
        public static XhtmlData ExclamationBlackSymbol(string toolTip) => Symbol("ExclamationBlackSymbol", toolTip);
        public static XhtmlData LedGreenSymbol() => LedGreenSymbol(null);
        public static XhtmlData LedGreenSymbol(string toolTip) => Symbol("LedGreenSymbol", toolTip);
        public static XhtmlData LedRedSymbol() => LedRedSymbol(null);
        public static XhtmlData LedRedSymbol(string toolTip) => Symbol("LedRedSymbol", toolTip);
        public static XhtmlData LedOrangeSymbol(string toolTip) => Symbol("LedOrangeSymbol", toolTip);
        public static XhtmlData LedGreySymbol(string toolTip) => Symbol("LedGreySymbol", toolTip);
        public static XhtmlData CheckmarkBlackSymbol() => CheckmarkBlackSymbol(null);
        public static XhtmlData CheckmarkBlackSymbol(string toolTip) => Symbol("CheckmarkBlackSymbol", toolTip);
        public static XhtmlData EmptySquareSymbol() => Symbol("EmptySquareSymbol", null);
        public static XhtmlData CheckmarkOnGreenSquareSymbol() => Symbol("CheckmarkOnGreenSquareSymbol", null);
        public static XhtmlData TriangleOrangeRightSymbol(string toolTip) => Symbol("TriangleOrangeRightSymbol", toolTip);
        public static XhtmlData CheckmarkOrangeSymbol(string toolTip) => Symbol("CheckmarkOrangeSymbol", toolTip);
        public static XhtmlData NotYetGreySymbol(string toolTip) => Symbol("NotyetGreySymbol", toolTip);
        public static XhtmlData NotOkSymbol(string toolTip) => Symbol("NotOkSymbol", toolTip);
        public static XhtmlData CheckmarkRedSymbol(string toolTip) => Symbol("CheckmarkRedSymbol", toolTip);

        public static XhtmlData FlowStepSymbol(FlowStep step, string toolTip)
        {
            switch (step) {
                case FlowStep.CanEdit:
                    return Symbol("FlowStepSymbol FlowStepCanEditSymbol", toolTip);
                case FlowStep.Partial:
                    return Symbol("FlowStepSymbol FlowStepPartialSymbol", toolTip);
                case FlowStep.Rejected:
                    return Symbol("FlowStepSymbol FlowStepRejectedSymbol", toolTip);
                case FlowStep.Ok:
                    return Symbol("FlowStepSymbol FlowStepOkSymbol", toolTip);
                default:
                    throw new ArgumentOutOfRangeException(nameof(step), step, null);
            }
        }

        public static XhtmlData FlowStepSymbol(FlowStep? step, string toolTip) => step == null ? XhtmlData.Empty : FlowStepSymbol(step.Value, toolTip);
        public static XhtmlData FlowStepSymbol(FlowStep? step) => FlowStepSymbol(step, null);

        public static XhtmlData RenderPercentageGrafisch(decimal percentage)
            => XhtmlData.Create(
                new XElement("div",
                    new XAttribute("class", "staafdiagramprocentcontainer"),
                    new XElement("div",
                        new XAttribute("class", "staafdiagramprocentdata"), (percentage / 100).ToString("P1")),
                    new XElement("div",
                        new XAttribute("class", "staafdiagramprocentbalkachtergrond"),
                        new XElement("div",
                            new XAttribute("class", "staafdiagramprocentbalk"),
                            new XAttribute("style", $"width: {percentage:N0}px;")))));
    }
}
