using System;
using System.Linq;
using System.Net;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Radius
{
    public enum RadiusAttributeType
    {
        // ReSharper disable UnusedMember.Global
        //directly from the RFC ;-) after VS.NET regex "^:b+{[^:b]+}:b+{[^:b].+}$" is replaced with "\2 = \1,"
        //then dashes +unassigned nrs. are removed.
        IllegalType = 0,
        UserName = 1,
        UserPassword = 2,
        CHAPPassword = 3,
        NASIPAddress = 4,
        NASPort = 5,
        ServiceType = 6,
        FramedProtocol = 7,
        FramedIPAddress = 8,
        FramedIPNetmask = 9,
        FramedRouting = 10,
        FilterId = 11,
        FramedMTU = 12,
        FramedCompression = 13,
        LoginIPHost = 14,
        LoginService = 15,
        LoginTCPPort = 16,

        //unassigned_17 = 17,
        ReplyMessage = 18,
        CallbackNumber = 19,
        CallbackId = 20,

        //unassigned_21 = 21,
        FramedRoute = 22,
        FramedIPXNetwork = 23,
        State = 24,
        Class = 25,
        VendorSpecific = 26,
        SessionTimeout = 27,
        IdleTimeout = 28,
        TerminationAction = 29,
        CalledStationId = 30,
        CallingStationId = 31,
        NASIdentifier = 32,
        ProxyState = 33,
        LoginLATService = 34,
        LoginLATNode = 35,
        LoginLATGroup = 36,
        FramedAppleTalkLink = 37,
        FramedAppleTalkNetwork = 38,
        FramedAppleTalkZone = 39,

        //40-59:reserved for accounting.
        CHAPChallenge = 60,
        NASPortType = 61,
        PortLimit = 62,
        LoginLATPort = 63,
        // ReSharper restore UnusedMember.Global
    }

    //see http://tools.ietf.org/html/rfc2138 or http://en.wikipedia.org/wiki/RADIUS
    public sealed class RadiusAttribute
    {
        public RadiusAttributeType AttributeType { get; }
        public byte[] AttributeValue { get; set; }

        [NotNull]
        public byte[] Paket
            => new[] { (byte)(int)AttributeType, (byte)(AttributeValue.Length + 2) }.Concat(AttributeValue).ToArray();

        public int Length
            => AttributeValue.Length + 2;

        public RadiusAttribute(RadiusAttributeType Type, byte[] attributeValue)
        {
            AttributeType = Type;
            AttributeValue = attributeValue;
            if (Length > 255) {
                throw new ArgumentException("attribute value too large");
            }
        }

        [NotNull]
        public static RadiusAttribute NASIPAddress([NotNull] IPAddress addr)
            => new RadiusAttribute(RadiusAttributeType.NASIPAddress, addr.GetAddressBytes());
    }
}
