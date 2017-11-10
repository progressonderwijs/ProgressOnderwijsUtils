using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    [CodeThatsOnlyUsedForTests]
    public static class SerializationCloner
    {
        [NotNull]
        [CodeThatsOnlyUsedForTests]
        public static T Clone<T>([NotNull] T obj)
        {
            if (!typeof(T).IsSerializable) {
                throw new ArgumentException("Type " + typeof(T) + " is not serializable and cannot be cloned", nameof(obj));
            }
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                ms.Position = 0;
                return (T)bf.Deserialize(ms);
            }
        }
    }
}
