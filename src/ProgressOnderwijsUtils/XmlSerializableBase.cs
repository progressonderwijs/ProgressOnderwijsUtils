using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProgressOnderwijsUtils
{
	public class XmlSerializableBase<T> where T : XmlSerializableBase<T>
	{
		readonly static XmlSerializer serializer = new XmlSerializer(typeof(T));
		public static T Deserialize(XmlReader from) { return (T)serializer.Deserialize(from); }
		public static T Deserialize(XDocument from) { using (var reader = from.CreateReader()) return Deserialize(reader); }
		public static T Deserialize(XElement from) {  return Deserialize(new XDocument(from)); }
		public void SerializeTo(Stream s) { serializer.Serialize(s, this); }
		public void SerializeTo(TextWriter w) { serializer.Serialize(w, this); }
		public void SerializeTo(XmlWriter xw) { serializer.Serialize(xw, this); }
	}
}
