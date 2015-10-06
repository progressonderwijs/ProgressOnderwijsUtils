using System.Xml.Linq;

namespace ProgressOnderwijsUtils
{
    public static class HtmlHelper
    {
        public static XhtmlData CheckmarkGreenNoBorderSymbol() => XhtmlData.Create(new XElement("span", new XAttribute("class", "CheckmarkGreenNoBorderSymbol")));

        public static XhtmlData QuestionRedSymbol(string toolTip) => XhtmlData.Create(
            new XElement("span",
                new XAttribute("class", "QuestionRedSymbol hastooltip"),
                new XAttribute("data-tiptext", toolTip)));

        public static XhtmlData CrossRedSymbol(string toolTip) => XhtmlData.Create(
            new XElement("span",
                new XAttribute("class", "CrossRedSymbol hastooltip"),
                new XAttribute("data-tiptext", toolTip)));

        public static XhtmlData ExclamationGreenSymbol(string toolTip) => XhtmlData.Create(
            new XElement("span",
                new XAttribute("class", "ExclamationGreenSymbol hastooltip"),
                new XAttribute("data-tiptext", toolTip)));

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
