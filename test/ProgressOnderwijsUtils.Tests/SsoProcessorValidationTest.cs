using System;
using System.Xml.Linq;
using System.Xml.Schema;
using Xunit;
using ProgressOnderwijsUtils.SingleSignOn;

namespace ProgressOnderwijsUtils.Tests;

public sealed class SsoProcessorValidationTest
{
    static readonly XElement VALID = new XElement(
        SamlNamespaces.SAMLP_NS + "AuthnRequest",
        new XAttribute(XNamespace.Xmlns + "saml", SamlNamespaces.SAML_NS.NamespaceName),
        new XAttribute(XNamespace.Xmlns + "sampl", SamlNamespaces.SAMLP_NS.NamespaceName),
        new XAttribute("ID", "_" + Guid.NewGuid()),
        new XAttribute("Version", "2.0"),
        new XAttribute("IssueInstant", DateTime.UtcNow),
        new XAttribute("Destination", "Naar"),
        new XAttribute("ForceAuthn", "false"),
        new XAttribute("IsPassive", "false"),
        new XAttribute("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"),
        new XElement(SamlNamespaces.SAML_NS + "Issuer", "Iemand")
    );

    static readonly XElement INVALID = new XElement(
        SamlNamespaces.SAMLP_NS + "AuthnRequest",
        new XAttribute(XNamespace.Xmlns + "saml", SamlNamespaces.SAML_NS.NamespaceName),
        new XAttribute(XNamespace.Xmlns + "sampl", SamlNamespaces.SAMLP_NS.NamespaceName),
        new XAttribute("ID", "_" + Guid.NewGuid()),
        new XAttribute("Version", "2.0"),
        new XAttribute("IssueInstant", DateTime.UtcNow),
        new XAttribute("Destination", "Naar"),
        new XAttribute("ForceAuthn", "false"),
        new XAttribute("IsPassive", "false"),
        new XAttribute("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"),
        new XElement(SamlNamespaces.SAML_NS + "Issuers", "Iemand")
    );

    const string VALID_NESTED = @"
<EntitiesDescriptor xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""urn:oasis:names:tc:SAML:2.0:metadata http://docs.oasis-open.org/security/saml/v2.0/saml-schema-metadata-2.0.xsd"" Name=""http://www.uocgmarket.nl"" ID=""_12bae828-bc3d-4cd7-a935-2b640b9fb927"" validUntil=""2012-12-14T19:35:13.665039Z"" xmlns=""urn:oasis:names:tc:SAML:2.0:metadata"">
    <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
        <SignedInfo>
            <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315""/>
            <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/>
            <Reference URI=""#_12bae828-bc3d-4cd7-a935-2b640b9fb927"">
                <Transforms>
                    <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature""/>
                    <Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/>
                </Transforms>
                <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
                <DigestValue>woU3dwO/e0Wc6JFkpu8nNU2XRRY=</DigestValue>
            </Reference>
        </SignedInfo>
        <SignatureValue>mfXx42pd/t9+UGZwErGJD4aeJKYD2JatQSLv862aVnCmcKa/cjtBzdHKWcEsfH6FtbGS5mILsVRUtni4BpcAe+u0/trpG/SAQ2RudMk/8+O0pX24NL/SL5qRaoLoXFlkZWr9ryPXQsDAPRZaz3g++FMSg1GJBVnDHHHwzjV7dLjvCVhlOkpWcmumx5h9Jk5QGsGpJ57H3QfXMYrJVX2sokds1kK+MOsCJzCwG2l4Zr95Uy4nmjd5jUaqlycxCGeh/dfoWk0yupHmzXuuaeTQ56wL+J1QM93VidbFLrJMopBGjWLvN3ZPAxW/+Rs9LGlIF9QyBOam2/rzlEfyqIH6cA==</SignatureValue>
        <KeyInfo>
            <X509Data>
                <X509Certificate>MIIC0DCCAbigAwIBAgIQNaBQlPUqn5lC8qskCP72sDANBgkqhkiG9w0BAQUFADARMQ8wDQYDVQQDEwZXRUJQM1cwHhcNMTEwNDE0MTE0MTUyWhcNMTIwNDE0MDAwMDAwWjARMQ8wDQYDVQQDEwZXRUJQM1cwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDS7Wlej9OssPWvpsBRFiQJdDyK4vK5LbTTY+r5tMwyEhAOizzCtv1Vcp2oZ7qDuC47z6mf73VRwaTKgryptA1eDKb9yC4tgWGZQQmkcsi+puhuGv2hnNjtIcP0JGEmWcXfWPVxQWOpcmvqmApYUTYeGpQqi5iEt9pG4G071XrV3pITGF2VI8qt+ZbzMKaFoJVG2XqRfYcTEhmxvjxa/QwZBzFE74l71c2cKv/mmlXSDzEst0jZwLjsvH9aFH+vzw3BEOKJnGS1CCzE0lKcxXt+X0pkKvi9wN75QreWKpzxX0ELAOSN1UmObkHcCzaTE96kigJ6iLxx1iZM7fpCZcFpAgMBAAGjJDAiMAsGA1UdDwQEAwIEMDATBgNVHSUEDDAKBggrBgEFBQcDATANBgkqhkiG9w0BAQUFAAOCAQEAFJLBprHswbJHYR3Wyw1SULNrRDwHsNze0sMlHj43fxhsZCo3CrtG8jBVs49t2mOM1mlM2v2A4C7cFauUrNqvjGiWH4XhTdPgu/+o7x+YkTAn/NlxOkxarUIbKwyVNBbKjraVMcfvQCm3TUni3366x2kwZsqb00xsMBT0Ibp73/Yee7DbEEXNKGIu7PMxFYFbiIkETwC6G45dA2w7oGyJ+ZCX9RDZKbXZ49SqG83eWt7z3vMDhi3GkXa+slWeXPukr0EYHLIQ9uN1xPvt/0NBFGj5BMYHNSwDmAmeOrEjo0uNR6wh+A+8D3vOy/2bwH+sGzxW3uoyhAiEpAcE/2j46g==</X509Certificate>
            </X509Data>
        </KeyInfo>
    </Signature>
    <EntityDescriptor entityID=""http://progresswww.nl"">
        <SPSSODescriptor protocolSupportEnumeration=""urn:oasis:names:tc:SAML:2.0:protocol"" AuthnRequestsSigned=""true"">
            <KeyDescriptor use=""signing"">
                <KeyInfo xmlns=""http://www.w3.org/2000/09/xmldsig#"">
                    <X509Data>
                        <X509Certificate>MIIC0DCCAbigAwIBAgIQNaBQlPUqn5lC8qskCP72sDANBgkqhkiG9w0BAQUFADARMQ8wDQYDVQQDEwZXRUJQM1cwHhcNMTEwNDE0MTE0MTUyWhcNMTIwNDE0MDAwMDAwWjARMQ8wDQYDVQQDEwZXRUJQM1cwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDS7Wlej9OssPWvpsBRFiQJdDyK4vK5LbTTY+r5tMwyEhAOizzCtv1Vcp2oZ7qDuC47z6mf73VRwaTKgryptA1eDKb9yC4tgWGZQQmkcsi+puhuGv2hnNjtIcP0JGEmWcXfWPVxQWOpcmvqmApYUTYeGpQqi5iEt9pG4G071XrV3pITGF2VI8qt+ZbzMKaFoJVG2XqRfYcTEhmxvjxa/QwZBzFE74l71c2cKv/mmlXSDzEst0jZwLjsvH9aFH+vzw3BEOKJnGS1CCzE0lKcxXt+X0pkKvi9wN75QreWKpzxX0ELAOSN1UmObkHcCzaTE96kigJ6iLxx1iZM7fpCZcFpAgMBAAGjJDAiMAsGA1UdDwQEAwIEMDATBgNVHSUEDDAKBggrBgEFBQcDATANBgkqhkiG9w0BAQUFAAOCAQEAFJLBprHswbJHYR3Wyw1SULNrRDwHsNze0sMlHj43fxhsZCo3CrtG8jBVs49t2mOM1mlM2v2A4C7cFauUrNqvjGiWH4XhTdPgu/+o7x+YkTAn/NlxOkxarUIbKwyVNBbKjraVMcfvQCm3TUni3366x2kwZsqb00xsMBT0Ibp73/Yee7DbEEXNKGIu7PMxFYFbiIkETwC6G45dA2w7oGyJ+ZCX9RDZKbXZ49SqG83eWt7z3vMDhi3GkXa+slWeXPukr0EYHLIQ9uN1xPvt/0NBFGj5BMYHNSwDmAmeOrEjo0uNR6wh+A+8D3vOy/2bwH+sGzxW3uoyhAiEpAcE/2j46g==</X509Certificate>
                    </X509Data>
                </KeyInfo>
            </KeyDescriptor>
            <AssertionConsumerService Binding=""urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"" Location=""https://progresswww.nl/surff/sso/post/"" index=""0"" isDefault=""true""/>
        </SPSSODescriptor>
    </EntityDescriptor>
</EntitiesDescriptor>
";

    const string INVALID_NESTED = @"
<EntitiesDescriptor xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""urn:oasis:names:tc:SAML:2.0:metadata http://docs.oasis-open.org/security/saml/v2.0/saml-schema-metadata-2.0.xsd"" Name=""http://www.uocgmarket.nl"" ID=""_12bae828-bc3d-4cd7-a935-2b640b9fb927"" validUntil=""2012-12-14T19:35:13.665039Z"" xmlns=""urn:oasis:names:tc:SAML:2.0:metadata"">
    <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
        <SignedInfo>
            <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315""/>
            <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/>
            <Reference URI=""#_12bae828-bc3d-4cd7-a935-2b640b9fb927"">
                <Transforms>
                    <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature""/>
                    <Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/>
                </Transforms>
                <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
                <DigestValue>woU3dwO/e0Wc6JFkpu8nNU2XRRY=</DigestValue>
            </Reference>
        </SignedInfo>
        <SignatureValue>mfXx42pd/t9+UGZwErGJD4aeJKYD2JatQSLv862aVnCmcKa/cjtBzdHKWcEsfH6FtbGS5mILsVRUtni4BpcAe+u0/trpG/SAQ2RudMk/8+O0pX24NL/SL5qRaoLoXFlkZWr9ryPXQsDAPRZaz3g++FMSg1GJBVnDHHHwzjV7dLjvCVhlOkpWcmumx5h9Jk5QGsGpJ57H3QfXMYrJVX2sokds1kK+MOsCJzCwG2l4Zr95Uy4nmjd5jUaqlycxCGeh/dfoWk0yupHmzXuuaeTQ56wL+J1QM93VidbFLrJMopBGjWLvN3ZPAxW/+Rs9LGlIF9QyBOam2/rzlEfyqIH6cA==</SignatureValue>
        <KeyInfo>
            <X509Datas>
                <X509Certificate>MIIC0DCCAbigAwIBAgIQNaBQlPUqn5lC8qskCP72sDANBgkqhkiG9w0BAQUFADARMQ8wDQYDVQQDEwZXRUJQM1cwHhcNMTEwNDE0MTE0MTUyWhcNMTIwNDE0MDAwMDAwWjARMQ8wDQYDVQQDEwZXRUJQM1cwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDS7Wlej9OssPWvpsBRFiQJdDyK4vK5LbTTY+r5tMwyEhAOizzCtv1Vcp2oZ7qDuC47z6mf73VRwaTKgryptA1eDKb9yC4tgWGZQQmkcsi+puhuGv2hnNjtIcP0JGEmWcXfWPVxQWOpcmvqmApYUTYeGpQqi5iEt9pG4G071XrV3pITGF2VI8qt+ZbzMKaFoJVG2XqRfYcTEhmxvjxa/QwZBzFE74l71c2cKv/mmlXSDzEst0jZwLjsvH9aFH+vzw3BEOKJnGS1CCzE0lKcxXt+X0pkKvi9wN75QreWKpzxX0ELAOSN1UmObkHcCzaTE96kigJ6iLxx1iZM7fpCZcFpAgMBAAGjJDAiMAsGA1UdDwQEAwIEMDATBgNVHSUEDDAKBggrBgEFBQcDATANBgkqhkiG9w0BAQUFAAOCAQEAFJLBprHswbJHYR3Wyw1SULNrRDwHsNze0sMlHj43fxhsZCo3CrtG8jBVs49t2mOM1mlM2v2A4C7cFauUrNqvjGiWH4XhTdPgu/+o7x+YkTAn/NlxOkxarUIbKwyVNBbKjraVMcfvQCm3TUni3366x2kwZsqb00xsMBT0Ibp73/Yee7DbEEXNKGIu7PMxFYFbiIkETwC6G45dA2w7oGyJ+ZCX9RDZKbXZ49SqG83eWt7z3vMDhi3GkXa+slWeXPukr0EYHLIQ9uN1xPvt/0NBFGj5BMYHNSwDmAmeOrEjo0uNR6wh+A+8D3vOy/2bwH+sGzxW3uoyhAiEpAcE/2j46g==</X509Certificate>
            </X509Datas>
        </KeyInfo>
    </Signature>
    <EntityDescriptor entityID=""http://progresswww.nl"">
        <SPSSODescriptor protocolSupportEnumeration=""urn:oasis:names:tc:SAML:2.0:protocol"" AuthnRequestsSigned=""true"">
            <KeyDescriptor use=""signing"">
                <KeyInfo xmlns=""http://www.w3.org/2000/09/xmldsig#"">
                    <X509Data>
                        <X509Certificate>MIIC0DCCAbigAwIBAgIQNaBQlPUqn5lC8qskCP72sDANBgkqhkiG9w0BAQUFADARMQ8wDQYDVQQDEwZXRUJQM1cwHhcNMTEwNDE0MTE0MTUyWhcNMTIwNDE0MDAwMDAwWjARMQ8wDQYDVQQDEwZXRUJQM1cwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDS7Wlej9OssPWvpsBRFiQJdDyK4vK5LbTTY+r5tMwyEhAOizzCtv1Vcp2oZ7qDuC47z6mf73VRwaTKgryptA1eDKb9yC4tgWGZQQmkcsi+puhuGv2hnNjtIcP0JGEmWcXfWPVxQWOpcmvqmApYUTYeGpQqi5iEt9pG4G071XrV3pITGF2VI8qt+ZbzMKaFoJVG2XqRfYcTEhmxvjxa/QwZBzFE74l71c2cKv/mmlXSDzEst0jZwLjsvH9aFH+vzw3BEOKJnGS1CCzE0lKcxXt+X0pkKvi9wN75QreWKpzxX0ELAOSN1UmObkHcCzaTE96kigJ6iLxx1iZM7fpCZcFpAgMBAAGjJDAiMAsGA1UdDwQEAwIEMDATBgNVHSUEDDAKBggrBgEFBQcDATANBgkqhkiG9w0BAQUFAAOCAQEAFJLBprHswbJHYR3Wyw1SULNrRDwHsNze0sMlHj43fxhsZCo3CrtG8jBVs49t2mOM1mlM2v2A4C7cFauUrNqvjGiWH4XhTdPgu/+o7x+YkTAn/NlxOkxarUIbKwyVNBbKjraVMcfvQCm3TUni3366x2kwZsqb00xsMBT0Ibp73/Yee7DbEEXNKGIu7PMxFYFbiIkETwC6G45dA2w7oGyJ+ZCX9RDZKbXZ49SqG83eWt7z3vMDhi3GkXa+slWeXPukr0EYHLIQ9uN1xPvt/0NBFGj5BMYHNSwDmAmeOrEjo0uNR6wh+A+8D3vOy/2bwH+sGzxW3uoyhAiEpAcE/2j46g==</X509Certificate>
                    </X509Data>
                </KeyInfo>
            </KeyDescriptor>
            <AssertionConsumerService Binding=""urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"" Location=""https://progresswww.nl/surff/sso/post/"" index=""0"" isDefault=""true""/>
        </SPSSODescriptor>
    </EntityDescriptor>
</EntitiesDescriptor>
";

    [Fact]
    public void ValidateXDocument()
    {
        SsoProcessor.ValidateSchema(VALID); //assert does not throw
        _ = Assert.ThrowsAny<XmlSchemaValidationException>(() => SsoProcessor.ValidateSchema(INVALID));
    }

    [Fact]
    public void ValidateNested()
    {
        SsoProcessor.ValidateSchema(XElement.Parse(VALID_NESTED)); //assert does not throw
        _ = Assert.ThrowsAny<XmlSchemaValidationException>(() => SsoProcessor.ValidateSchema(XElement.Parse(INVALID_NESTED)));
    }
}