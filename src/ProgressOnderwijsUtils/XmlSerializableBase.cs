using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProgressOnderwijsUtils
{
    public static class XmlSerializerHelper
    {
        public static T? Deserialize<T>(string xml)
            => XmlSerializerHelper<T>.Deserialize(xml);

        public static T? Deserialize<T>(XDocument xml)
            => XmlSerializerHelper<T>.Deserialize(xml);

        public static string SerializeToString(object o)
        {
            using var writer = new StringWriter();
            using (var xw = XmlWriter.Create(writer)) {
                ((IXmlSerializeHelper)
                        typeof(XmlSerializerHelper<>)
                            .MakeGenericType(o.GetType())
                            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                            .Single()
                            .Invoke(null)
                    ).SerializeToInst(xw, o);
            }
            return writer.ToString();
        }

        public static XDocument SerializeToXDocument(object o)
        {
            var doc = new XDocument();
            using (var xw = doc.CreateWriter()) {
                ((IXmlSerializeHelper)
                        typeof(XmlSerializerHelper<>)
                            .MakeGenericType(o.GetType())
                            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                            .Single()
                            .Invoke(null)
                    ).SerializeToInst(xw, o);
            }

            return doc;
        }
    }

    interface IXmlSerializeHelper
    {
        void SerializeToInst(XmlWriter xw, object val);
    }

    public sealed class XmlSerializerHelper<T> : IXmlSerializeHelper
    {
        public static readonly XmlSerializer serializer = new XmlSerializer(typeof(T));

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
        {
            using var writer = new StringWriter();
            serializer.Serialize(writer, val);
            return writer.ToString();
        }

        internal XmlSerializerHelper() { }

        void IXmlSerializeHelper.SerializeToInst(XmlWriter xw, object val)
            => serializer.Serialize(xw, val);
    }
}
