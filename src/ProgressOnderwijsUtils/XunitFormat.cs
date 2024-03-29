using System.Xml.Serialization;

namespace ProgressOnderwijsUtils;

public static class XunitFormat
{
    [UsefulToKeep("library attribute")]
    public static XunitResultReport? LoadFromXmlReport(string xUnitXmlReportUri)
        => LoadFromXmlReport(XmlReader.Create(xUnitXmlReportUri));

    public static XunitResultReport? LoadFromXmlReport(XmlReader reader)
        => reader.Using(XmlSerializerHelper<XunitResultReport>.Deserialize);

    /// <summary>
    /// xUnit escapes most content; replacing e.g. newlines with \n. This function unescapes that.
    /// </summary>
    public static string xUnitUnescapeString(string message)
    {
        //see https://github.com/xunit/xunit/blame/master/src/xunit.runner.utility/Sinks/DelegatingSinks/DelegatingXmlCreationSink.cs#L249
        var nextSlash = message.IndexOf('\\');
        if (nextSlash < 0 || message.Length == nextSlash + 1) {
            return message;
        }
        var output = new StringBuilder();
        var doneUpto = 0;
        while (true) {
            _ = output.Append(message, doneUpto, nextSlash - doneUpto);
            var nextChar = message[nextSlash + 1];
            _ = output.Append(
                nextChar switch {
                    'n' => '\n',
                    'r' => '\r',
                    '\\' => '\\',
                    't' => '\t',
                    '"' => '"',
                    var c => c,
                }
            );
            doneUpto = nextSlash + 2;
            nextSlash = message.IndexOf('\\', doneUpto);
            if (nextSlash < 0 || message.Length == nextSlash + 1) {
                _ = output.Append(message, doneUpto, message.Length - doneUpto);
                return output.ToString();
            }
        }
    }

    //Manually transcribed from https://xunit.github.io/docs/format-xml-v2
    [XmlRoot(ElementName = "assemblies")]
    [UsedImplicitlyBySerialization]
    public sealed class XunitResultReport
    {
        [XmlAttribute]
        public string? timestamp;

        [XmlElement]
        public XunitReportAssembly[]? assembly;
    }

    [UsedImplicitlyBySerialization]
    public sealed class XunitReportAssembly
    {
        [XmlAttribute]
        public string? name;

        [XmlAttribute("config-file")]
        public string? configFile;

        [XmlAttribute("test-framework")]
        public string? testFramework;

        [XmlAttribute]
        public string? environment;

        [XmlAttribute("run-date")]
        public string? runDate;

        [XmlAttribute("run-time")]
        public string? runTime;

        [XmlAttribute("time")]
        public double timeInSeconds;

        [XmlAttribute]
        public int total;

        [XmlAttribute]
        public int passed;

        [XmlAttribute]
        public int failed;

        [XmlAttribute]
        public int skipped;

        [XmlAttribute("errors")]
        public int errorCount;

        [XmlArray("errors")]
        [XmlArrayItem("error")]
        public XunitError[]? errors;

        [XmlElement("collection")]
        public XunitCollection[]? collections;
    }

    [UsedImplicitlyBySerialization]
    public sealed class XunitError
    {
        [XmlAttribute]
        public string? name;

        [XmlAttribute]
        public string? type;

        [XmlElement]
        public XunitFailure? failure;
    }

    [UsedImplicitlyBySerialization]
    public sealed class XunitFailure
    {
        [XmlAttribute("exception-type")]
        public string? exceptionType;

        [XmlElement]
        public string? message;

        [XmlElement("stack-trace")]
        public string? stackTrace;
    }

    [UsedImplicitlyBySerialization]
    public sealed class XunitCollection
    {
        [XmlAttribute]
        public string? name;

        [XmlAttribute("time")]
        public double timeInSeconds;

        [XmlAttribute]
        public int total;

        [XmlAttribute]
        public int passed;

        [XmlAttribute]
        public int failed;

        [XmlAttribute]
        public int skipped;

        [XmlElement("test")]
        public XunitTest[]? tests;
    }

    public enum XunitResult
    {
        // ReSharper disable UnusedMember.Global
        Pass, Fail, Skip,
        // ReSharper restore UnusedMember.Global
    }

    [UsedImplicitlyBySerialization]
    public sealed class XunitTest
    {
        [XmlAttribute]
        public string? name;

        [XmlAttribute]
        public string? type;

        [XmlAttribute]
        public string? method;

        [XmlAttribute("time")]
        public double timeInSeconds;

        [XmlAttribute]
        public XunitResult result;

        [XmlElement]
        public XunitFailure? failure;

        [XmlElement("reason")]
        public string? reasonSkipped;

        [XmlArray("traits")]
        [XmlArrayItem("trait")]
        public XunitTrait[]? traits;
    }

    [UsedImplicitlyBySerialization]
    public sealed class XunitTrait
    {
        [XmlAttribute]
        public string? name;

        [XmlAttribute]
        public string? value;
    }
}
