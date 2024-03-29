using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace ProgressOnderwijsUtils;

public static class RsaStringSigner
{
    public static string SignString(X509Certificate2 certificate, string input)
        => Convert.ToBase64String(
                certificate.GetRSAPrivateKey()
                    .AssertNotNull()
                    .SignData(Encoding.UTF8.GetBytes(input), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
            )
            + " " + input;

    public static string? VerifySignedString(X509Certificate2 certificate, string input)
    {
        var splitInput = input.Split(new[] { ' ', }, 2);
        if (splitInput.Length != 2) {
            return null;
        }

        var message = splitInput[1];
        var signature = Convert.FromBase64String(splitInput[0]);

        return
            certificate.GetRSAPublicKey().AssertNotNull().VerifyData(Encoding.UTF8.GetBytes(message), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
                ? message
                : null;
    }

    public static string SignJson<TState>(X509Certificate2 certificate, TState obj)
        => SignString(certificate, JsonSerializer.Serialize(obj));

    public static TState VerifySignedJson<TState>(X509Certificate2 certificate, string signedState)
        => JsonSerializer.Deserialize<TState>(VerifySignedString(certificate, signedState) ?? throw new("Signature verification failed")) ?? throw new("Deserialization error?");
}
