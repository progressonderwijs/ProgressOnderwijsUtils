using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace ProgressOnderwijsUtils
{
    public static class RsaStringSigner
    {
        [NotNull]
        public static string SignString(X509Certificate2 certificate, [NotNull] string input)
            => Convert.ToBase64String(
                    certificate.GetRSAPrivateKey()
                        .SignData(Encoding.UTF8.GetBytes(input), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
                )
                + " " + input;

        public static string? VerifySignedString(X509Certificate2 certificate, [NotNull] string input)
        {
            var splitInput = input.Split(new[] { ' ' }, 2);
            if (splitInput.Length != 2) {
                return null;
            }

            var message = splitInput[1];
            var signature = Convert.FromBase64String(splitInput[0]);

            return
                certificate.GetRSAPublicKey().VerifyData(Encoding.UTF8.GetBytes(message), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
                    ? message
                    : null;
        }

        [NotNull]
        public static string SignJson<TState>(X509Certificate2 certificate, TState obj)
            => SignString(certificate, JsonConvert.SerializeObject(obj));

        public static TState VerifySignedJson<TState>(X509Certificate2 certificate, [NotNull] string signedState)
            => JsonConvert.DeserializeObject<TState>(VerifySignedString(certificate, signedState) ?? throw new Exception("Signature verification failed"));
    }
}
