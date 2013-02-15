using System.Collections.Generic;
using System.Xml;

namespace ProgressOnderwijsUtils
{
    public static class XmlExtensions
    {
        public static IDictionary<string, string> GetAttributes(this XmlReader reader)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();
            if (reader.HasAttributes)
            {
                bool next = reader.MoveToFirstAttribute();
                while (next)
                {
                    result[reader.Name.ToLower()] = reader.Value;
                    next = reader.MoveToNextAttribute();
                }
                reader.MoveToElement();
            }
            return result;
        }
    }
}