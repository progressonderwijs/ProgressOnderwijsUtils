using System.Xml.Serialization;

namespace ProgressOnderwijsUtils;

public static class XmlSerializerHelper
{
    public static T? Deserialize<T>(string xml)
        => XmlSerializerHelper<T>.Deserialize(xml);

    public static T? Deserialize<T>(XDocument xml)
        => XmlSerializerHelper<T>.Deserialize(xml);

    public static string SerializeToString(object o)
        => SerializeToString(o, null);

    public static string SerializeToString(object o, XmlSerializerNamespaces? namespaces)
    {
        using var writer = new StringWriter();
        using (var xw = XmlWriter.Create(writer)) {
            ((IXmlSerializeHelper)
                    typeof(XmlSerializerHelper<>)
                        .MakeGenericType(o.GetType())
                        .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                        .Single()
                        .Invoke(null)
                ).SerializeToInst(xw, o, namespaces);
        }
        return writer.ToString();
    }

    public static XDocument SerializeToXDocument(object o)
        => SerializeToXDocument(o, null);

    public static XDocument SerializeToXDocument(object o, XmlSerializerNamespaces? namespaces)
    {
        var doc = new XDocument();
        using var xw = doc.CreateWriter();
        ((IXmlSerializeHelper)
                typeof(XmlSerializerHelper<>)
                    .MakeGenericType(o.GetType())
                    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single()
                    .Invoke(null)
            ).SerializeToInst(xw, o, namespaces);

        return doc;
    }

    public static string ToStringWithXmlDeclaration(this XDocument xdoc, XDeclaration declaration)
        => $"{declaration}{Environment.NewLine}{xdoc}";

    public static string ToStringWithXmlDeclaration(this XDocument xdoc, XDeclaration declaration, SaveOptions options)
        => $"{declaration}{Environment.NewLine}{xdoc.ToString(options)}";
}

interface IXmlSerializeHelper
{
    void SerializeToInst(XmlWriter xw, object val)
        => SerializeToInst(xw, val, null);

    void SerializeToInst(XmlWriter xw, object val, XmlSerializerNamespaces? namespaces);
}

public sealed class XmlSerializerHelper<T> : IXmlSerializeHelper
{
    public static readonly XmlSerializer serializer = new(typeof(T));

    public static T? Deserialize(XDocument from)
    {
        using var reader = from.CreateReader();
        return Deserialize(reader);
    }

    public static T? Deserialize(XmlReader from)
#pragma warning disable 8605 // workaround https://youtrack.jetbrains.com/issue/RSRP-483518
        => (T?)serializer.Deserialize(from);
#pragma warning restore 8605

    public static T? Deserialize(string from)
    {
        using var reader = new StringReader(from);
#pragma warning disable 8605 // workaround https://youtrack.jetbrains.com/issue/RSRP-483518
        return (T?)serializer.Deserialize(reader);
#pragma warning restore 8605
    }

    public static string Serialize(T val)
        => Serialize(val, null);

    public static string Serialize(T val, XmlSerializerNamespaces? namespaces)
    {
        using var writer = new StringWriter();
        serializer.Serialize(writer, val, namespaces);
        return writer.ToString();
    }

    internal XmlSerializerHelper() { }

    void IXmlSerializeHelper.SerializeToInst(XmlWriter xw, object val, XmlSerializerNamespaces? namespaces)
        => serializer.Serialize(xw, val, namespaces);
}
