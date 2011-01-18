using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class SerializationCloner
	{
		public static T Clone<T>(T obj)
		{
			if (!typeof(T).IsSerializable) throw new ArgumentException("Type " + typeof(T) + " is not serializable and cannot be cloned", "obj");
			var bf = new BinaryFormatter();
			using (var ms = new MemoryStream())
			{
				bf.Serialize(ms, obj);
				ms.Position = 0;
				return (T)bf.Deserialize(ms);
			}
		}
	}
}

