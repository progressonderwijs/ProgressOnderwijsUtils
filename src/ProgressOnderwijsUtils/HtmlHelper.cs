﻿using System.Xml.Linq;

namespace ProgressOnderwijsUtils
{
    public static class HtmlHelper
    {
        static XhtmlData Symbol(string cssClass) => XhtmlData.Create(
            new XElement("span",
                new XAttribute("class", cssClass)));

        static XhtmlData Symbol(string cssClass, string toolTip) => XhtmlData.Create(
            new XElement("span",
                new XAttribute("class", cssClass + " hastooltip"),
                new XAttribute("data-tiptext", toolTip)));

        public static XhtmlData CheckmarkGreenNoBorderSymbol() => Symbol("CheckmarkGreenNoBorderSymbol");

        public static XhtmlData QuestionRedSymbol(string toolTip) => Symbol("QuestionRedSymbol", toolTip);

        public static XhtmlData CrossRedSymbol(string toolTip) => Symbol("CrossRedSymbol", toolTip);

        public static XhtmlData CrossGreenSymbol(string toolTip) => Symbol("CrossGreenSymbol", toolTip);

        public static XhtmlData ExclamationGreenSymbol(string toolTip) => Symbol("ExclamationGreenSymbol", toolTip);

        public static XhtmlData LedGreenSymbol(string toolTip) => Symbol("LedGreenSymbol", toolTip);

        public static XhtmlData LedRedSymbol(string toolTip) => Symbol("LedRedSymbol", toolTip);

        public static XhtmlData RenderPercentageGrafisch(decimal percentage)
        {
            var divOuter = new XElement(
                "div",
                new object[] {
                    new XAttribute("class", "staafdiagramprocentcontainer"),
                    new XElement(
                        "div",
                        new object[] {
                            new XAttribute("class", "staafdiagramprocentdata"),
                            (percentage / 100).ToString("P1")
                        }),
                    new XElement(
                        "div",
                        new object[] {
                            new XAttribute("class", "staafdiagramprocentbalkachtergrond"),
                            new XElement(
                                "div",
                                new object[] {
                                    new XAttribute("class", "staafdiagramprocentbalk"),
                                    new XAttribute("style", $"width: {percentage:N0}px;"),
                                })
                        })
                });
            return XhtmlData.Create(divOuter);
        }
    }
}
