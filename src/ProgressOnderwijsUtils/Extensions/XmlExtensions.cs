#nullable disable
using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class XmlExtensions
    {
        [NotNull]
        public static IDictionary<string, string> GetAttributes([NotNull] this XmlReader reader)
        {
            var result = new Dictionary<string, string>();
            if (reader.HasAttributes) {
                var next = reader.MoveToFirstAttribute();
                while (next) {
                    result[reader.Name.ToLower()] = reader.Value;
                    next = reader.MoveToNextAttribute();
                }
                reader.MoveToElement();
            }
            return result;
        }
    }
}
