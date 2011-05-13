﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProgressOnderwijsUtils
{
	public static class XmlSerializeHelper
	{
		public static T Deserialize<T>(string xml) { return XmlSerializeHelper<T>.Deserialize(xml); }
		public static object Deserialize(Type t, string xml)
		{
			using (var reader = XmlReader.Create(new StringReader(xml)))
				return ((IXmlSerializeHelper)
					typeof(XmlSerializeHelper<>)
						.MakeGenericType(t)
						.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
						.Single()
						.Invoke(null)
					).DeserializeInst(reader);
		}
		public static string Serialize(object o)
		{
			using (var writer = new StringWriter())
			{
				using (var xw = XmlWriter.Create(writer))
					((IXmlSerializeHelper)
						typeof(XmlSerializeHelper<>)
							.MakeGenericType(o.GetType())
							.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
							.Single()
							.Invoke(null)
						).SerializeToInst(xw, o);
				return writer.ToString();
			}
		}
	}

	internal interface IXmlSerializeHelper
	{
		object DeserializeInst(XmlReader from);
		void SerializeToInst(XmlWriter xw, object val);
	}
	public sealed class XmlSerializeHelper<T> : IXmlSerializeHelper
	{
		public readonly static XmlSerializer serializer = new XmlSerializer(typeof(T));

		public static T Deserialize(XDocument from) { using (var reader = from.CreateReader()) return Deserialize(reader); }
		public static T Deserialize(XmlReader from) { return (T)serializer.Deserialize(from); }
		public static T Deserialize(string from) { using (var reader = new StringReader(from)) return (T)serializer.Deserialize(reader); }
		public static void SerializeTo(XmlWriter xw, T val) { serializer.Serialize(xw, val); }
		public static string Serialize(T val) { using (var writer = new StringWriter()) { serializer.Serialize(writer, val); return writer.ToString(); } }

		internal XmlSerializeHelper() { }
		object IXmlSerializeHelper.DeserializeInst(XmlReader from) { return (T)serializer.Deserialize(from); }
		void IXmlSerializeHelper.SerializeToInst(XmlWriter xw, object val) { serializer.Serialize(xw, val); }
	}

	public abstract class XmlSerializableBase<T> where T : XmlSerializableBase<T>
	{
		public static T Deserialize(XDocument from) { return XmlSerializeHelper<T>.Deserialize(from); }
		public XDocument Serialize()
		{
			XDocument doc = new XDocument();
			using (var xw = doc.CreateWriter())
				XmlSerializeHelper<T>.SerializeTo(xw, (T)this);
			return doc;
		}
	}
}
