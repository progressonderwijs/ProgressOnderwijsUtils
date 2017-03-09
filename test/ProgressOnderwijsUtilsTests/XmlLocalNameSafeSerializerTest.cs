using System;
using System.Xml;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;
using Xunit;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class XmlLocalNameSafeSerializerTest
    {
        struct Data
        {
            public string Url;
            public DateTime Time;
            public string Token;
        }

        readonly Data data = new Data {
            Url = "https://en.wikipedia.org/wiki/Main_Page",
            Time = new DateTime(2017, 3, 9, 12, 33, 32, DateTimeKind.Utc),
            Token = "BQac8Qd2aQaJWrOnL-8LEh~VDRpnM1yWDOLyAmwUTeeLOWxGb3",
        };

        [Fact]
        public void Encode_is_reversible_using_Decode()
        {
            var encoded = XmlLocalNameSafeSerializer.Encode(data);
            var decoded = XmlLocalNameSafeSerializer.Decode<Data>(encoded);
            PAssert.That(() => Equals(decoded, data));
        }

        [Fact]
        public void Encode_produces_a_valid_NCName()
        {
            var encoded = XmlLocalNameSafeSerializer.Encode(data);
            XmlConvert.VerifyNCName(encoded);
        }
    }
}
