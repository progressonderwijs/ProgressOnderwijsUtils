using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ProgressOnderwijsUtils.Conext
{
	public class Saml20MetaData
	{
		private readonly XElement md;

		public Saml20MetaData(XElement md)
		{
			this.md = md;
		}

		public IEnumerable<string> GetEntities()
		{
			return (
				from element in md.DescendantsAndSelf(SchemaSet.SAMLMD_NS + "IDPSSODescriptor")
				select element.Parent.Attribute("entityID").Value
			).ToSet();
		}

		public string SingleSignOnService(string entity)
		{
			XElement desc = (
				from element in md.DescendantsAndSelf(SchemaSet.SAMLMD_NS + "IDPSSODescriptor")
			    where element.Parent.Attribute("entityID").Value == entity
			    select element
			).Single();

			return (
				from elem in desc.Elements(SchemaSet.SAMLMD_NS + "SingleSignOnService")
				where elem.Attribute("Binding").Value == "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"
				select elem.Attribute("Location").Value
			).Single();
		}
	}

	public struct ServiceProviderConfig
	{
		public ServiceProvider sp;
		public string entity;
		public int index;
		public X509Certificate2 certificate;
	}

	public struct IdentityProviderConfig
	{
		public IdentityProvider idp;
		public string metadata;
		public string identity;
		public X509Certificate2 certificate;
	}

	public static class MetaDataFactory
	{
		#region Configurations

		private const string IDP_CERTIFICATE = "conext.cer";

		private const string IDP_PROVIDER = "https://engine.surfconext.nl/authentication/proxy/idps-metadata";
		private const string IDP_PROVIDER_WAYF = "https://engine.surfconext.nl/authentication/idp/metadata";

		private const string IDP_ENTITY = "https://engine.surfconext.nl/authentication/proxy/idps-metadata";
		private const string IDP_ENTITY_WAYF = "https://engine.surfconext.nl/authentication/idp/metadata";

		private const string IDP_ENTITY_RUG = "https://signon.rug.nl/nidp/saml2/metadata";
		private const string IDP_ENTITY_FONTYS = "http://adfs2.fontys.nl/adfs/services/trust";
		private const string IDP_ENTITY_VU = "https://surf-sso.ubvu.vu.nl/simplesaml/saml2/idp/metadata.php";
		private const string IDP_ENTITY_UVA = "https://secure.uva.nl/cas";
		private const string IDP_ENTITY_STENDEN = "http://adfs.stenden.com/adfs/services/trust";

		private const string SP_CERTIFICATE = "surff.pfx";
		private const string SP_PROVIDER = "https://progressnet.nl/saml20/";
		private const string SP_ENTITY_P3W = "http://progresswww.nl";
		private const string SP_ENTITY_P3W_TEST = "http://progresswww.nl/test";
		private const string SP_ENTITY_PNET = "http://progressnet.nl";
		private const string SP_ENTITY_PNET_TEST = "http://test.progressnet.nl";
		private const string SP_ENTITY_PNET_ONTWIKKEL = "http://ontwikkel.progressnet.nl";
		private const string SP_ENTITY_STUDENT = "http://student.progressnet.nl";
		private const string SP_ENTITY_STUDENT_TEST = "http://teststudent.progressnet.nl";
		private const string SP_ENTITY_STUDENT_ONTWIKKEL = "http://ontwikkelstudent.progressnet.nl";
		private const string SP_ENTITY_STUDENT_OAUTH = "http://student.progressnet.nl/oauth";
		private const string SP_ENTITY_STUDENT_TEST_OAUTH = "http://teststudent.progressnet.nl/oauth";
		private const string SP_ENTITY_STUDENT_ONTWIKKEL_OAUTH = "http://ontwikkelstudent.progressnet.nl/oauth";

		private const string RESOURCE_PATH = "ProgressOnderwijsUtils.Conext.Resources";
		private static readonly object LOCK = new object();
		private static readonly IDictionary<string, X509Certificate2> CERTIFICATES = new Dictionary<string, X509Certificate2>();
		private static readonly IDictionary<string, Saml20MetaData> INSTANCES = new Dictionary<string, Saml20MetaData>();

		private static readonly XNamespace NS_XSD = "http://www.w3.org/2001/XMLSchema-instance";
		private static readonly XNamespace NS_DS = "http://www.w3.org/2000/09/xmldsig#";
		private static readonly XNamespace NS_MD = "urn:oasis:names:tc:SAML:2.0:metadata";

		private static Stream GetResource(params string[] paths)
		{
			return Assembly.GetAssembly(typeof(MetaDataFactory)).GetManifestResourceStream(
				paths.Aggregate(RESOURCE_PATH, (current, path) => string.Format(CultureInfo.InvariantCulture, "{0}.{1}", current, path)));
		}

		private static readonly IDictionary<ServiceProvider, IDictionary<DatabaseVersion, ServiceProviderConfig>> SERVICE_PROVIDERS = new Dictionary<ServiceProvider, IDictionary<DatabaseVersion, ServiceProviderConfig>>
		{
			{ ServiceProvider.P3W, new Dictionary<DatabaseVersion, ServiceProviderConfig>
				{
					{ DatabaseVersion.ProductieDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.P3W,
							entity = SP_ENTITY_P3W, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						} 
					},
					{ DatabaseVersion.TestDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.P3W,
							entity = SP_ENTITY_P3W_TEST, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
				}
			},
			{ ServiceProvider.PNet, new Dictionary<DatabaseVersion, ServiceProviderConfig>
				{
					{ DatabaseVersion.ProductieDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.PNet,
							entity = SP_ENTITY_PNET, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.TestDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.PNet,
							entity = SP_ENTITY_PNET_TEST, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.AcceptatieDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.PNet,
							entity = SP_ENTITY_PNET_TEST, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.DuoTestDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.PNet,
							entity = SP_ENTITY_PNET_TEST, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.VeldTestDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.PNet,
							entity = SP_ENTITY_PNET_TEST, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.OntwikkelDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.PNet,
							entity = SP_ENTITY_PNET_ONTWIKKEL, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.BronHODB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.PNet,
							entity = SP_ENTITY_PNET_ONTWIKKEL, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
				}
			},
			{ ServiceProvider.Student, new Dictionary<DatabaseVersion, ServiceProviderConfig>
				{
					{ DatabaseVersion.ProductieDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.Student,
							entity = SP_ENTITY_STUDENT, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.TestDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.Student,
							entity = SP_ENTITY_STUDENT_TEST, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.OntwikkelDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.Student,
							entity = SP_ENTITY_STUDENT_ONTWIKKEL, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
				}
			},
			{ ServiceProvider.StudentOAuth, new Dictionary<DatabaseVersion, ServiceProviderConfig>
				{
					{ DatabaseVersion.ProductieDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.StudentOAuth,
							entity = SP_ENTITY_STUDENT_OAUTH, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.TestDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.StudentOAuth,
							entity = SP_ENTITY_STUDENT_TEST_OAUTH, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
					{ DatabaseVersion.OntwikkelDB, new ServiceProviderConfig 
						{ 
							sp = ServiceProvider.StudentOAuth,
							entity = SP_ENTITY_STUDENT_ONTWIKKEL_OAUTH, 
							index = 0, 
							certificate = GetCertificate(SP_CERTIFICATE, "b00zen") 
						}
					},
				}
			},
		};

		private static readonly IDictionary<IdentityProvider, IdentityProviderConfig> IDENTITY_PROVIDERS = new Dictionary<IdentityProvider, IdentityProviderConfig>
		{
			{ IdentityProvider.Conext, new IdentityProviderConfig
				{
					idp = IdentityProvider.Conext,
					metadata = IDP_PROVIDER, 
					identity = IDP_ENTITY, 
					certificate = GetCertificate(IDP_CERTIFICATE, null)
				}
			},
			{ IdentityProvider.ConextWayf, new IdentityProviderConfig
				{ 
					idp = IdentityProvider.ConextWayf,
					metadata = IDP_PROVIDER_WAYF, 
					identity = IDP_ENTITY_WAYF, 
					certificate = GetCertificate(IDP_CERTIFICATE, null)
				}
			}
		};

		private static readonly IDictionary<IdentityProvider, IDictionary<ServiceProvider, IDictionary<DatabaseVersion, IDictionary<Entity, string>>>> ENTITIES =
			new Dictionary<IdentityProvider, IDictionary<ServiceProvider, IDictionary<DatabaseVersion, IDictionary<Entity, string>>>>
		{
			{ IdentityProvider.Conext, new Dictionary<ServiceProvider, IDictionary<DatabaseVersion, IDictionary<Entity, string>>>
				{
					{ ServiceProvider.P3W, new Dictionary<DatabaseVersion, IDictionary<Entity, string>>
						{
							{ DatabaseVersion.ProductieDB, new Dictionary<Entity, string>
								{
									{ Entity.Fontys, IDP_ENTITY_FONTYS },
									{ Entity.Stenden, IDP_ENTITY_STENDEN },
									{ Entity.UvA, IDP_ENTITY_UVA },
									{ Entity.VU, IDP_ENTITY_VU },
								}
							},
							{ DatabaseVersion.TestDB, new Dictionary<Entity, string>
								{
									{ Entity.Fontys, IDP_ENTITY_FONTYS },
									{ Entity.Stenden, IDP_ENTITY_STENDEN },
									{ Entity.UvA, IDP_ENTITY_UVA },
									{ Entity.VU, IDP_ENTITY_VU },
								}
							},
						}
					},
					{ ServiceProvider.PNet, new Dictionary<DatabaseVersion, IDictionary<Entity, string>>
						{
							{ DatabaseVersion.ProductieDB, new Dictionary<Entity, string>
								{
									{ Entity.Fontys, IDP_ENTITY_FONTYS },
									{ Entity.Stenden, IDP_ENTITY_STENDEN },
								}
							},
							{ DatabaseVersion.TestDB, new Dictionary<Entity, string>
								{
									{ Entity.Fontys, IDP_ENTITY_FONTYS },
									{ Entity.Stenden, IDP_ENTITY_STENDEN },
								}
							},
							{ DatabaseVersion.AcceptatieDB, new Dictionary<Entity, string>
								{
									{ Entity.Fontys, IDP_ENTITY_FONTYS },
									{ Entity.Stenden, IDP_ENTITY_STENDEN },
								}
							},
							{ DatabaseVersion.DuoTestDB, new Dictionary<Entity, string>
								{
									{ Entity.Fontys, IDP_ENTITY_FONTYS },
									{ Entity.Stenden, IDP_ENTITY_STENDEN },
								}
							},
							{ DatabaseVersion.VeldTestDB, new Dictionary<Entity, string>
								{
									{ Entity.Fontys, IDP_ENTITY_FONTYS },
									{ Entity.Stenden, IDP_ENTITY_STENDEN },
								}
							},
							{ DatabaseVersion.OntwikkelDB, new Dictionary<Entity, string>
								{
									{ Entity.Fontys, IDP_ENTITY_FONTYS },
									{ Entity.Stenden, IDP_ENTITY_STENDEN },
								}
							},
							{ DatabaseVersion.BronHODB, new Dictionary<Entity, string>
								{
									{ Entity.Fontys, IDP_ENTITY_FONTYS },
									{ Entity.Stenden, IDP_ENTITY_STENDEN },
								}
							},
						}
					},
					{ ServiceProvider.Student, new Dictionary<DatabaseVersion, IDictionary<Entity, string>>
						{
							{ DatabaseVersion.ProductieDB, new Dictionary<Entity, string>
								{
									{ Entity.RuG, IDP_ENTITY_RUG },
								}
							},
							{ DatabaseVersion.TestDB, new Dictionary<Entity, string>
								{
									{ Entity.RuG, IDP_ENTITY_RUG },
								}
							},
							{ DatabaseVersion.OntwikkelDB, new Dictionary<Entity, string>
								{
									{ Entity.RuG, IDP_ENTITY_RUG },
								}
							},
						}
					},
					{ ServiceProvider.StudentOAuth, new Dictionary<DatabaseVersion, IDictionary<Entity, string>>
						{
							{ DatabaseVersion.ProductieDB, new Dictionary<Entity, string>
								{
									{ Entity.RuG, IDP_ENTITY_RUG },
								}
							},
							{ DatabaseVersion.TestDB, new Dictionary<Entity, string>
								{
									{ Entity.RuG, IDP_ENTITY_RUG },
								}
							},
							{ DatabaseVersion.OntwikkelDB, new Dictionary<Entity, string>
								{
									{ Entity.RuG, IDP_ENTITY_RUG },
								}
							},
						}
					},
				}
			},
		};

		private static readonly IDictionary<RootOrganisatie, Entity> INSTELLINGEN = new Dictionary<RootOrganisatie, Entity>
		{
			{ RootOrganisatie.CHN, Entity.Stenden },
			{ RootOrganisatie.RUG, Entity.RuG },
			{ RootOrganisatie.Fontys, Entity.Fontys },
		};

		#endregion

		#region Factory methods

		public static ServiceProviderConfig GetServiceProvider(ServiceProvider sp, DatabaseVersion db)
		{
			return SERVICE_PROVIDERS[sp][db];
		}

		public static IdentityProviderConfig GetIdentityProvider(IdentityProvider idp)
		{
			return IDENTITY_PROVIDERS[idp];
		}

		public static IDictionary<Entity, string> GetEntities(IdentityProvider idp, ServiceProvider? sp = null, DatabaseVersion? db = null)
		{
			return idp == IdentityProvider.ConextWayf 
				? EnumHelpers.GetValues<Entity>().ToDictionary(o => o, o => IDP_ENTITY_WAYF)
				: ENTITIES[idp][sp.Value][db.Value];
		}

		public static Entity GetEntity(RootOrganisatie instelling)
		{
			return INSTELLINGEN.GetOrDefault(instelling, Entity.Unknown);
		}

		public static Saml20MetaData GetMetaData(IdentityProviderConfig idp, ServiceProviderConfig? sp = null)
		{
			switch (idp.idp)
			{
			case IdentityProvider.Conext:
				if (sp.HasValue)
					return GetIdPProvider(idp.identity, sp.Value.entity, idp.certificate);
				throw new ArgumentException();
			case IdentityProvider.ConextWayf:
				var xml = XElement.Load(idp.metadata, LoadOptions.PreserveWhitespace);
				Validate(xml, idp.certificate);
				return new Saml20MetaData(xml);
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private static Saml20MetaData GetIdPProvider(string url, string sp, X509Certificate2 cer)
		{
			string uri = url + "?sp-entity-id=" + Uri.EscapeDataString(sp);
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
			{
				var xml = XElement.Load(reader);
				Validate(xml, cer);
				return new Saml20MetaData(xml);
			}
		}

		public static void Validate(XElement assertion, X509Certificate2 cer)
		{
			new XDocument(assertion).Validate(null);

			XmlDocument doc = new XmlDocument
			{
				PreserveWhitespace = true,
			};

			using (XmlReader reader = assertion.CreateReader())
			{
				doc.Load(reader);
			}

			SignedXml dsig = new SignedXml(doc);
			dsig.LoadXml(doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").Cast<XmlElement>().Single());
			if (!dsig.CheckSignature(cer, true)) // TODO: cannot seem to validate the certificate? Must be stored?
			{
				throw new CryptographicException("metadata not signed");
			}
		}

		private static X509Certificate2 GetCertificate(string cer, string passwd)
		{
			lock (LOCK)
			{
				if (!CERTIFICATES.ContainsKey(cer))
				{
					byte[] buf;
					using (Stream str = GetResource(cer))
					{
						buf = new byte[str.Length];
						str.Read(buf, 0, buf.Length);
					}
					CERTIFICATES.Add(cer, new X509Certificate2(buf, passwd));
				}
				return CERTIFICATES[cer];
			}
		}

		#endregion

		#region Generate

		public static XmlDocument Generate()
		{
			Guid id = Guid.NewGuid();
			X509Certificate2 cer = GetCertificate(SP_CERTIFICATE, "b00zen");
			XDocument md = Generate(id, cer);
			XmlDocument result = Sign(md, cer);
			return result;
		}

		private static XDocument Generate(Guid id, X509Certificate2 cer)
		{
			return new XDocument(new XDeclaration("1.0", "utf-8", null),
				new XElement(NS_MD + "EntitiesDescriptor",
					new XAttribute(XNamespace.Xmlns + "xsi", NS_XSD.NamespaceName),
					new XAttribute(NS_XSD + "schemaLocation", "urn:oasis:names:tc:SAML:2.0:metadata http://docs.oasis-open.org/security/saml/v2.0/saml-schema-metadata-2.0.xsd"),
					new XAttribute("Name", "http://www.uocgmarket.nl"),
					new XAttribute("ID", string.Format("_{0}", id)),
					new XAttribute("validUntil", DateTime.UtcNow.AddHours(6)),
					GenerateEntity(cer, SP_ENTITY_P3W, "https://progresswww.nl/surff/sso/post/"),
					GenerateEntity(cer, SP_ENTITY_P3W_TEST, "https://progresswww.nl/surfftest/sso/post/"),
					GenerateEntity(cer, SP_ENTITY_PNET, "https://progressnet.nl/singlesignon"),
					GenerateEntity(cer, SP_ENTITY_PNET_TEST, "https://test.progressnet.nl/singlesignon"),
					GenerateEntity(cer, SP_ENTITY_PNET_ONTWIKKEL, "https://ontwikkel.progressnet.nl/singlesignon"),
					GenerateEntity(cer, SP_ENTITY_STUDENT, "https://student.progressnet.nl/saml20/sso/post"),
					GenerateEntity(cer, SP_ENTITY_STUDENT_TEST, "https://teststudent.progressnet.nl/saml20/sso/post"),
					GenerateEntity(cer, SP_ENTITY_STUDENT_ONTWIKKEL, "https://ontwikkelstudent.progressnet.nl/saml20/sso/post"),
					GenerateEntity(cer, SP_ENTITY_STUDENT_OAUTH, "https://student.progressnet.nl/oauth/sso/post"),
					GenerateEntity(cer, SP_ENTITY_STUDENT_TEST_OAUTH, "https://teststudent.progressnet.nl/oauth/sso/post"),
					GenerateEntity(cer, SP_ENTITY_STUDENT_ONTWIKKEL_OAUTH, "https://ontwikkelstudent.progressnet.nl/oauth/sso/post")
				)
			);
		}

		private static XElement GenerateEntity(X509Certificate2 cer, string entity, string post, string redirect = null, string slo = null)
		{
			return new XElement(NS_MD + "EntityDescriptor", new XAttribute("entityID", entity),
				new XElement(NS_MD + "SPSSODescriptor", 
					new XAttribute("protocolSupportEnumeration", "urn:oasis:names:tc:SAML:2.0:protocol"), 
					new XAttribute("AuthnRequestsSigned", "true"),
					new XElement(NS_MD + "KeyDescriptor", 
						new XAttribute("use", "signing"),
						new XElement(NS_DS + "KeyInfo",
							new XElement(NS_DS + "X509Data",
								new XElement(NS_DS + "X509Certificate",
									new XText(Convert.ToBase64String(cer.RawData)))))),
					slo == null ? null :
					new XElement(NS_MD + "SingleLogoutService",
						new XAttribute("Binding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"),
						new XAttribute("Location", slo)),
					post == null ? null :
					new XElement(NS_MD + "AssertionConsumerService",
						new XAttribute("Binding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"),
						new XAttribute("Location", post),
						new XAttribute("index", "0"),
						new XAttribute("isDefault", "true")),
					redirect == null ? null :
					new XElement(NS_MD + "AssertionConsumerService",
						new XAttribute("Binding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"),
						new XAttribute("Location", redirect),
						new XAttribute("index", "1"))),
				GenerateOrganization(),
				GenerateTechnicalContact(),
				GenerateAdministrativeContact());
		}

		private static XElement GenerateOrganization()
		{
			return new XElement(NS_MD + "Organization",
	            new XElement(NS_MD + "OrganizationName",
					new XAttribute(XNamespace.Xml + "lang", "nl"),
					new XText("UOCGMarket")),
			    new XElement(NS_MD + "OrganizationDisplayName",
			        new XAttribute(XNamespace.Xml + "lang", "nl"),
			        new XText("UOCGMarket BV")),
			    new XElement(NS_MD + "OrganizationURL",
			        new XAttribute(XNamespace.Xml + "lang", "nl"),
			        new XText("http://www.uocgmarket.nl")));
		}

		private static XElement GenerateTechnicalContact()
		{
			return GenerateContact("technical", "Fons", "Dijkstra", "f.j.dijkstra@rug.nl", "+31(0)50 363 6671");
		}

		private static XElement GenerateAdministrativeContact()
		{
			return GenerateContact("administrative", "Jan", "Batteram", "j.batteram@rug.nl", "+31(0)50 363 2000");
		}

		private static XElement GenerateContact(string type, string givenName, string surName, string email, string phone)
		{
			return new XElement(NS_MD + "ContactPerson",
				new XAttribute("contactType", type),
				new XElement(NS_MD + "GivenName", givenName),
				new XElement(NS_MD + "SurName", surName),
				new XElement(NS_MD + "EmailAddress", email),
				new XElement(NS_MD + "TelephoneNumber", phone));
		}

		private static XmlDocument Sign(XDocument md, X509Certificate2 cer)
		{
			// load the XML document to sign
			XmlDocument result = new XmlDocument
         	{
         		PreserveWhitespace = true,
         	};
			using (XmlReader reader = md.CreateReader())
			{
				result.Load(reader);
			}
			SignedXml dsig = new SignedXml(result)
         	{
         		SigningKey = cer.PrivateKey,
         	};
			XmlNode root = result.GetElementsByTagName("EntitiesDescriptor", NS_MD.NamespaceName).Cast<XmlNode>().Single();

			// specify the signature
			Reference reference = new Reference(string.Format("#{0}", root.Attributes["ID"].Value));
			reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
			reference.AddTransform(new XmlDsigExcC14NTransform());
			dsig.AddReference(reference);
			dsig.KeyInfo = new KeyInfo();
			dsig.KeyInfo.AddClause(new KeyInfoX509Data(cer));

			// sign
			dsig.ComputeSignature();
			root.PrependChild(dsig.GetXml());

			return result;
		}

		#endregion
	}
}