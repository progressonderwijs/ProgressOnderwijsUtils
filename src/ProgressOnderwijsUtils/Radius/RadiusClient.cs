using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using JetBrains.Annotations;

//see http://tools.ietf.org/html/rfc2138 or http://en.wikipedia.org/wiki/RADIUS
//really basic idea: each packet is 1 byte code, 1 byte "identifier" (random sequence number),
//2 bytes length,
//16 bytes "authenticator" (random for requests, a hashed version of the request auth+server secret for response)
//plus any number of attributes thereafter.
//auth is then sending a packet with code 1 and attributes username, userpassword and NASIPAddress to the server
//server responds with code 2 if ok, code 3 if not and generally no attributes.  That's all!

//Radius client very loosely based on nRadius by Nightwalker_z; completely rewritten 2009-07-28, to address the following:
//bugfixes in randomness of requestAuthenticator; fixed passwords of over 16 bytes; externalizes string-encoding issues;
//added appropriate IDisposable treatment for MD5 (probably unnecessary) and UdpClient;
//replaced numeric codes with more comprehensible enums
//added verification for the response authenticator integrity
//fixed bug where server responses other than success or failure were interpreted as success.

namespace ProgressOnderwijsUtils.Radius
{
    public enum RadiusAuthResults
    {
        // ReSharper disable UnusedMember.Global
        UndefinedError,
        Success,
        AuthenticationFailed,
        ServiceErrorNoResponse,
        ServiceErrorPacketTooShort,
        ServiceErrorUnexpectedIdentifier,
        ServiceErrorPacketMalformed,
        ServiceErrorBadResponseAuthenticator,
        // ReSharper restore UnusedMember.Global
    }

    public static class RadiusClient
    {
        //Utility function to make working with RNGCryptoServiceProvider easier
        [NotNull]
        public static byte[] NextBytes([NotNull] this RNGCryptoServiceProvider secureRandom, int byteCount)
        {
            var data = new byte[byteCount];
            secureRandom.GetBytes(data);
            return data;
        }

        public static byte NextByte([NotNull] this RNGCryptoServiceProvider secureRandom)
        {
            var data = new byte[1];
            secureRandom.GetBytes(data);
            return data[0];
        }

        const int UDP_TTL = 20;
        const int pRadiusPort = 1812;
        const int udpTimeoutMs = 1000;

        //see http://tools.ietf.org/html/rfc2138
        public static RadiusAuthResults Authenticate(string serverHostname, [NotNull] byte[] sharedSecret, byte[] username, [NotNull] byte[] password, [NotNull] params RadiusAttribute[] extraAttributes)
        {
            byte requestCode = 1; //means "Access-Request"
            var secureRandom = new RNGCryptoServiceProvider();

            var requestIdentifier = secureRandom.NextByte();
            var requestAuthenticator = secureRandom.NextBytes(16);

            var radiusAttributes = new List<RadiusAttribute>(extraAttributes) {
                new RadiusAttribute(RadiusAttributeType.UserName, username),
                new RadiusAttribute(RadiusAttributeType.UserPassword, HashPapPassword(password, sharedSecret, requestAuthenticator))
            };
            if (radiusAttributes.Count != radiusAttributes.Select(attr => attr.AttributeType).Distinct().Count()) {
                throw new ArgumentException("extraAttributes may not contain duplicate attributes or a UserName or UserPassword attribute");
            }
            using (var requestStream = new MemoryStream()) {
                requestStream.WriteByte(requestCode);
                requestStream.WriteByte(requestIdentifier);
                requestStream.WriteByte(0); //2
                requestStream.WriteByte(0); //length placeholder;
                requestStream.Write(requestAuthenticator, 0, requestAuthenticator.Length);
                foreach (var serializedAttribute in radiusAttributes.Select(attr => attr.Paket)) {
                    requestStream.Write(serializedAttribute, 0, serializedAttribute.Length);
                }
                var request = requestStream.ToArray();
                if (request.Length > short.MaxValue) { //perhaps actually ushort.Length is permitted, but we're taking no risks
                    throw new Exception("nRadius request too large");
                }
                request[2] = (byte)(request.Length >> 8); //most significant byte first - big endian!
                request[3] = (byte)request.Length;

                byte[] response;
                try {
                    response = AuthNetworkHelper(serverHostname, request);
                } catch (SocketException) { //one retry.
                    try {
                        response = AuthNetworkHelper(serverHostname, request);
                    } catch (SocketException) {
                        return RadiusAuthResults.ServiceErrorNoResponse;
                    }
                }
                return ProcessServerResponse(response, requestIdentifier, requestAuthenticator, sharedSecret);
            }
        }

        [NotNull]
        static byte[] AuthNetworkHelper([NotNull] string serverHostname, [NotNull] byte[] request)
        {
            using (var udpClient = new UdpClient()) {
                udpClient.Client.SendTimeout = udpTimeoutMs;
                udpClient.Client.ReceiveTimeout = udpTimeoutMs;
                udpClient.Ttl = UDP_TTL;
                var RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                udpClient.Connect(serverHostname, pRadiusPort);
                udpClient.Send(request, request.Length);
                var response = udpClient.Receive(ref RemoteIpEndPoint); //TODO: parallelization issue!!!

                udpClient.Close(); //probably redundant; docs (as usual) just don't say.
                return response;
            }
        }

        static RadiusAuthResults ProcessServerResponse([NotNull] byte[] response, byte requestIdentifier, byte[] requestAuthenticator, byte[] sharedSecret)
        {
            // Checking the minimum paket length of 20 bytes
            if (response.Length < 20) {
                return RadiusAuthResults.ServiceErrorPacketTooShort; //error
            }

            // Checking Radius identifier. The request identifier (pClientIdentifier) must be equal to
            // the response identifier of the radius server.
            int responseIdentifier = response[1];
            if (responseIdentifier != requestIdentifier) {
                return RadiusAuthResults.ServiceErrorUnexpectedIdentifier; //error
            }

            // Checking the length field of the paket. This value must be equal to the byteArray length
            // "receivedBytes.Length"
            var responseLen = (response[2] << 8) + response[3];
            if (responseLen != response.Length) {
                return RadiusAuthResults.ServiceErrorPacketMalformed; //error
            }

            var receivedMd5 = response.Skip(4).Take(16).ToArray();

            var verificationStream =
                response.Take(4) //Code+ID+Length
                    .Concat(requestAuthenticator) //request authenticator
                    .Concat(response.Skip(20)) //attributes
                    .Concat(sharedSecret)
                    .ToArray();

            using (var md5 = new MD5CryptoServiceProvider()) {
                if (!md5.ComputeHash(verificationStream).SequenceEqual(receivedMd5)) {
                    return RadiusAuthResults.ServiceErrorBadResponseAuthenticator;
                }
            }

            //ok, we've checked that the packet is basically OK and has a valid response authenticator...

            // Checking Radius Code (first Byte)
            int responseCode = response[0];
            return responseCode switch {
                2 => RadiusAuthResults.Success,
                3 => RadiusAuthResults.AuthenticationFailed,
                _ => RadiusAuthResults.ServiceErrorPacketMalformed
            };
        }

        [NotNull]
        public static byte[] HashPapPassword([NotNull] byte[] password, [NotNull] byte[] sharedSecret, [NotNull] byte[] requestAuthenticator)
        {
            using (var md5 = new MD5CryptoServiceProvider()) {
                //Hashed password in generated in 16-byte chunks; for each 16-bytes an "unguessable" pad is generated which
                //is XOR-ed with the password.
                //then, for the next 16-bytes, a new pad is generated, and so on.

                // Initially pad is MD5(sharedSecret + requestAuthenticator)
                var pMD5Sum = md5.ComputeHash(sharedSecret.Concat(requestAuthenticator).ToArray());

                // how many rounds are needed == number of 16-byte "chunks" in password (rounded up)
                var nHashRounds = (password.Length + 15) / 16;
                var Result = new byte[nHashRounds * 16];

                for (var j = 0; j < nHashRounds; j++) {
                    for (var i = 0; i < 16; i++) {
                        var pos = j * 16 + i;
                        var pp = pos < password.Length ? password[pos] : (byte)0;
                        Result[pos] = (byte)(pMD5Sum[i] ^ pp);
                    }

                    //after this round, create pad for next round: MD5(sharedSecret + Last16Bytes of result)
                    pMD5Sum = md5.ComputeHash(sharedSecret.Concat(Result.Skip(16 * j).Take(16)).ToArray());
                }
                return Result;
            }
        }
    }
}
