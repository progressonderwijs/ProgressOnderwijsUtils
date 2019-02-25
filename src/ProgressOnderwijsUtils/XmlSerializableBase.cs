using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class XmlSerializerHelper
    {
        public static T Deserialize<T>([NotNull] string xml)
            => XmlSerializerHelper<T>.Deserialize(xml);

        public static T Deserialize<T>([NotNull] XDocument xml)
            => XmlSerializerHelper<T>.Deserialize(xml);

        [NotNull]
        public static string SerializeToString([NotNull] object o)
        {
            using (var writer = new StringWriter()) {
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
        }

        [NotNull]
        public static XDocument SerializeToXDocument([NotNull] object o)
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

        public static T Deserialize([NotNull] XDocument from)
        {
            using (var reader = from.CreateReader()) {
                return Deserialize(reader);
            }
        }

        public static T Deserialize([NotNull] XmlReader from)
            => (T)serializer.Deserialize(from);

        public static T Deserialize([NotNull] string from)
        {
            using (var reader = new StringReader(from)) {
                return (T)serializer.Deserialize(reader);
            }
        }

        [NotNull]
        [CodeThatsOnlyUsedForTests]
        public static string Serialize([NotNull] T val)
        {
            using (var writer = new StringWriter()) {
                serializer.Serialize(writer, val);
                return writer.ToString();
            }
        }

        internal XmlSerializerHelper() { }

        void IXmlSerializeHelper.SerializeToInst([NotNull] XmlWriter xw, [NotNull] object val)
        {
            serializer.Serialize(xw, val);
        }
    }
}
