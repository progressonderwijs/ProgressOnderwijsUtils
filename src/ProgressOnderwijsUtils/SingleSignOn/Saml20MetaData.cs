﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.SingleSignOn
{
    public class Saml20MetaData
    {
        readonly XElement md;

        public Saml20MetaData(XElement md)
        {
            this.md = md;
        }

        public IEnumerable<string> GetEntities()
        {
            return (
                from element in md.DescendantsAndSelf(SamlNamespaces.SAMLMD_NS + "IDPSSODescriptor")
                select element.Parent.Attribute("entityID").Value
                ).ToSet();
        }

        public string SingleSignOnService(string entity)
        {
            var desc = (
                from element in md.DescendantsAndSelf(SamlNamespaces.SAMLMD_NS + "IDPSSODescriptor")
                where element.Parent.Attribute("entityID").Value == entity
                select element
                ).Single();

            return (
                from elem in desc.Elements(SamlNamespaces.SAMLMD_NS + "SingleSignOnService")
                where elem.Attribute("Binding").Value == "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"
                select elem.Attribute("Location").Value
                ).Single();
        }
    }
}
