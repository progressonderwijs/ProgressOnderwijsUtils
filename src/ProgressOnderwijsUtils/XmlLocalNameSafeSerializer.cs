using System;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace ProgressOnderwijsUtils
{
    public static class XmlLocalNameSafeSerializer
    {
        // using base64 generally results in shorter strings, because less character have to be escaped and XmlConvert escaping is inefficient
        public static string Encode(object obj)
            => XmlConvert.EncodeLocalName(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj))));

        public static T Decode<T>(string str)
            => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(Convert.FromBase64String(XmlConvert.DecodeName(str))));
    }
}
