using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ProgressOnderwijsUtils.ErrorHandling
{
    public static class StackTraceToXhtml
    {
        static readonly Regex stackLine =
            new Regex(
                @"(?<=^|\n)   at (?<namespace>.*\.)?(?<class>[^.\n]+)\.(?<method>\.*[^.\n]*)\((?<pars>.*)\)( in (?<path>.*[\\/])?(?<file>[^\\/\n\r]*):line (?<line>\d+))?\r?(\n|$)",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        public static XhtmlData ToXhtml(string stacktrace)
        {
            return XhtmlData.Create(
                stacktrace == null
                    ? new XElement("h3", "Stacktrace missing!")
                    : StacktraceTable(stacktrace)
                );
        }

        static XElement StacktraceTable(string stacktrace)
        {
            return new XElement(
                "div",
                new XAttribute("class", "stacktrace"),
                new XElement("table", decompose(stacktrace))
                );
        }

        static IEnumerable<XNode> decompose(string trace)
        {
            if (trace == null) {
                throw new ArgumentNullException("trace");
            }
            int lastIndex = 0;
            foreach (Match m in stackLine.Matches(trace)) {
                if (m.Index > lastIndex) {
                    foreach (var plaintoken in NewlineToTr(trace.Substring(lastIndex, m.Index - lastIndex))) {
                        yield return plaintoken;
                    }
                }

                lastIndex = m.Index + m.Length;

                var fileLocEl = !m.Groups["file"].Success
                    ? null
                    : new XElement(
                        "span",
                        new XAttribute("class", "stacktrace-src"),
                        " in ",
                        m.Groups["path"].Value,
                        new XElement("span", new XAttribute("class", "stacktrace-file"), m.Groups["file"].Value),
                        ":line ",
                        new XElement("span", new XAttribute("class", "stacktrace-line"), m.Groups["line"].Value)
                        );

                yield return new XElement(
                    "tr",
                    new XAttribute("class", "stacktrace-frame"),
                    new XElement(
                        "td",
                        new XAttribute("class", "stacktrace-class"),
                        !m.Groups["namespace"].Success
                            ? null
                            : new XElement("span", new XAttribute("class", "stacktrace-ns"), m.Groups["namespace"].Value),
                        m.Groups["class"].Value),
                    new XElement(
                        "td",
                        new XAttribute("class", "stacktrace-method"),
                        ".",
                        m.Groups["method"].Value,
                        "(",
                        new XElement("span", new XAttribute("class", "stacktrace-pars"), m.Groups["pars"].Value),
                        ")",
                        fileLocEl
                        )
                    );
            }

            if (trace.Length > lastIndex) {
                foreach (var plaintoken in NewlineToTr(trace.Substring(lastIndex, trace.Length - lastIndex))) {
                    yield return plaintoken;
                }
            }
        }

        static IEnumerable<XElement> NewlineToTr(string substring)
        {
            int at = 0;
            while (at < substring.Length) {
                int next = substring.IndexOf('\n', at);
                if (next < 0) {
                    next = substring.Length;
                }
                var trim = next > 0 && substring[next - 1] == '\r' ? -1 : 0;
                var str = substring.Substring(at, next - at + trim);

                yield return
                    new XElement(
                        "tr",
                        new XElement("td", new XAttribute("colspan", "2"), str.Length == 0 ? "\u00A0" : str));
                at = next + 1;
            }
        }
    }
}
