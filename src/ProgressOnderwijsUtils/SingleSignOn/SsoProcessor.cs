using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml.Schema;

namespace ProgressOnderwijsUtils.SingleSignOn;

public static class SsoProcessor
{
    const string UID = "urn:mace:dir:attribute-def:uid";
    const string MAIL = "urn:mace:dir:attribute-def:mail";
    const string DOMAIN = "urn:mace:terena.org:attribute-def:schacHomeOrganization";
    const string ROLE = "urn:mace:dir:attribute-def:eduPersonAffiliation";

    public static Uri GetRedirectUrl(AuthnRequest request)
    {
        //Don't escape colon: Uri.ToString doesn't either; and this is just a defense-in-depth we don't need
        //ref: https://github.com/aspnet/HttpAbstractions/commit/1e9d57f80ca883881804292448fff4de8b112733
        static string Escape(string str)
            => Uri.EscapeDataString(str).Replace("%3A", ":");
        static string EncodeQueryParameter(string key, string value)
            => $"{Escape(key)}={Escape(value)}";

        var samlRequestQueryString = $"{EncodeQueryParameter("SAMLRequest", request.EncodeAsQueryArgument())}&{EncodeQueryParameter("SigAlg", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")}";
        var rsaPrivateKey = request.Issuer.certificate.GetRSAPrivateKey().AssertNotNull();
        var base64Signature = Convert.ToBase64String(rsaPrivateKey.SignData(Encoding.UTF8.GetBytes(samlRequestQueryString), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        var signedQueryString = $"{samlRequestQueryString}&{EncodeQueryParameter("Signature", base64Signature)}";
        return new($"{request.Destination}?{signedQueryString}");
    }

    static XElement? GetAssertion(XElement response)
    {
        var statusCodes = response.Descendants(SamlNamespaces.SAMLP_NS + "StatusCode").ToArray();
        if (statusCodes.Length > 1) {
            return null;
        }

        var statusCodeAttribute = statusCodes[0].Attribute("Value")
            ?? throw new InvalidOperationException("Missing status code attribute");

        if (statusCodeAttribute.Value == "urn:oasis:names:tc:SAML:2.0:status:Success") {
            return response.Descendants(SamlNamespaces.SAML_NS + "Assertion").Single();
        }

        return null;
    }

    public static Maybe<SsoAttributes, string> GetAttributes(string rawSamlResponse, X509Certificate2 certificate)
    {
        byte[] bytes;
        try {
            bytes = Convert.FromBase64String(rawSamlResponse);
        } catch (FormatException e) {
            return Maybe.Error($"Invalid base64: {e.Message}");
        }

        var rawXml = Encoding.UTF8.GetString(bytes);

        XElement xml;
        try {
            xml = XElement.Parse(rawXml);
        } catch (XmlException e) {
            return Maybe.Error($"Invalid XML: {e.Message}");
        }

        try {
            ValidateSchema(xml);
        } catch (XmlSchemaValidationException e) {
            return Maybe.Error($"Response invalid: {e.Message}");
        }

        var doc = new XmlDocument {
            PreserveWhitespace = true,
        };
        doc.LoadXml(rawXml);

        var signatureElements = doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").Cast<XmlElement>().ToArray();
        if (signatureElements.Length != 1) {
            return Maybe.Error(signatureElements.Length == 0 ? "Signature missing" : "Multiple signatures");
        }

        var dsig = new SignedXml(doc);
        dsig.LoadXml(signatureElements.Single());
        if (!dsig.CheckSignature(certificate.GetRSAPublicKey())) {
            return Maybe.Error("Signature invalid");
        }

        var assertion = GetAssertion(xml);
        if (assertion == null) {
            return Maybe.Error("Missing Assertion element");
        }

        var authnStatement = assertion.Element(SamlNamespaces.SAML_NS + "AuthnStatement");
        if (authnStatement == null) {
            return Maybe.Error("Missing AuthnStatement element");
        }

        var uid = GetNullableAttribute(assertion, UID);
        if (uid == null) {
            return Maybe.Error($"Missing attribute {UID}");
        }

        var (inresponseTo, notOnOrAfter) = GetSubjectConfirmationData(assertion);
        if (notOnOrAfter.Kind != DateTimeKind.Utc) {
            return Maybe.Error("NotOnOrAfter must be UTC");
        }

        var now = DateTime.UtcNow;
        if (now >= notOnOrAfter) {
            return Maybe.Error($"Expired: {now} >= {notOnOrAfter}");
        }

        return Maybe.Ok(
            new SsoAttributes {
                uid = uid,
                domain = GetNullableAttribute(assertion, DOMAIN),
                email = GetAttributes(assertion, MAIL),
                roles = GetAttributes(assertion, ROLE),
                InResponseTo = inresponseTo,
                AuthnContextClassRef = (string?)authnStatement.Element(SamlNamespaces.SAML_NS + "AuthnContext")?.Element(SamlNamespaces.SAML_NS + "AuthnContextClassRef"),
            }
        );
    }

    static (string? inresponseTo, DateTime notOnOrAfter) GetSubjectConfirmationData(XElement assertion)
    {
        var subjectConfirmationData = assertion
            .Element(SamlNamespaces.SAML_NS + "Subject")
            .AssertNotNull()
            .Element(SamlNamespaces.SAML_NS + "SubjectConfirmation")
            .AssertNotNull()
            .Element(SamlNamespaces.SAML_NS + "SubjectConfirmationData")
            .AssertNotNull();
        return (
            XmlConvert.DecodeName((string?)subjectConfirmationData.Attribute("InResponseTo")),
            (DateTime)subjectConfirmationData.Attribute("NotOnOrAfter").AssertNotNull()
        );
    }

    static string? GetNullableAttribute(XElement assertion, string key)
        => (
            from attribute in assertion.Descendants(SamlNamespaces.SAML_NS + "AttributeValue")
            // ReSharper disable PossibleNullReferenceException
            where attribute.Parent?.Attribute("Name")?.Value == key
            // ReSharper restore PossibleNullReferenceException
            select attribute.Value
        ).SingleOrNull();

    static string[] GetAttributes(XElement assertion, string key)
        => (
            from attribute in assertion.Descendants(SamlNamespaces.SAML_NS + "AttributeValue")
            // ReSharper disable PossibleNullReferenceException
            where attribute.Parent?.Attribute("Name")?.Value == key
            // ReSharper restore PossibleNullReferenceException
            select attribute.Value).ToArray();

    static readonly XmlSchemaSet schemaSet = new() { XmlResolver = null, };

    static SsoProcessor()
    {
        var settings = new XmlReaderSettings {
            XmlResolver = null,
            DtdProcessing = DtdProcessing.Parse,
        };

        var schemaResources = new SingleSignOnSchemaResources();
        foreach (var resName in schemaResources.GetResourceNames()) {
            if (resName.EndsWith(".xsd", StringComparison.Ordinal) || resName.EndsWith(".xsd.intellisensehack", StringComparison.Ordinal)) {
                using var stream = schemaResources.GetResource(resName);
                using var reader = XmlReader.Create(stream, settings);
                _ = schemaSet.Add(XmlSchema.Read(reader, null).AssertNotNull());
            }
        }
    }

    public static void ValidateSchema(XElement assertion)
        => new XDocument(assertion).Validate(schemaSet, null, false);
}
