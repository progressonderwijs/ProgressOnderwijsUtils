using System.Collections.Generic;
using System.Xml;

namespace ProgressOnderwijsUtils
{
    public static class XmlExtensions
    {
        public static IDictionary<string, string> GetAttributes(this XmlReader reader)
        {
            var result = new Dictionary<string, string>();
            if (reader.HasAttributes) {
                var next = reader.MoveToFirstAttribute();
                while (next) {
                    result[reader.Name.ToLowerInvariant()] = reader.Value;
                    next = reader.MoveToNextAttribute();
                }
                _ = reader.MoveToElement();
            }
            return result;
        }
    }
}
