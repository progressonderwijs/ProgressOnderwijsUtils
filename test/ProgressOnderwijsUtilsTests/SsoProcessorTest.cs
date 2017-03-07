﻿using System.Security.Cryptography.X509Certificates;
using System.Text;
using ProgressOnderwijsUtils.SingleSignOn;
using Xunit;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class SsoProcessorTest
    {
        const string conextCertificateData = @"-----BEGIN CERTIFICATE-----
MIID3zCCAsegAwIBAgIJAMVC9xn1ZfsuMA0GCSqGSIb3DQEBCwUAMIGFMQswCQYD
VQQGEwJOTDEQMA4GA1UECAwHVXRyZWNodDEQMA4GA1UEBwwHVXRyZWNodDEVMBMG
A1UECgwMU1VSRm5ldCBCLlYuMRMwEQYDVQQLDApTVVJGY29uZXh0MSYwJAYDVQQD
DB1lbmdpbmUuc3VyZmNvbmV4dC5ubCAyMDE0MDUwNTAeFw0xNDA1MDUxNDIyMzVa
Fw0xOTA1MDUxNDIyMzVaMIGFMQswCQYDVQQGEwJOTDEQMA4GA1UECAwHVXRyZWNo
dDEQMA4GA1UEBwwHVXRyZWNodDEVMBMGA1UECgwMU1VSRm5ldCBCLlYuMRMwEQYD
VQQLDApTVVJGY29uZXh0MSYwJAYDVQQDDB1lbmdpbmUuc3VyZmNvbmV4dC5ubCAy
MDE0MDUwNTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKthMDbB0jKH
efPzmRu9t2h7iLP4wAXr42bHpjzTEk6gttHFb4l/hFiz1YBI88TjiH6hVjnozo/Y
HA2c51us+Y7g0XoS7653lbUN/EHzvDMuyis4Xi2Ijf1A/OUQfH1iFUWttIgtWK9+
fatXoGUS6tirQvrzVh6ZstEp1xbpo1SF6UoVl+fh7tM81qz+Crr/Kroan0UjpZOF
TwxPoK6fdLgMAieKSCRmBGpbJHbQ2xxbdykBBrBbdfzIX4CDepfjE9h/40ldw5jR
n3e392jrS6htk23N9BWWrpBT5QCk0kH3h/6F1Dm6TkyG9CDtt73/anuRkvXbeygI
4wml9bL3rE8CAwEAAaNQME4wHQYDVR0OBBYEFD+Ac7akFxaMhBQAjVfvgGfY8hNK
MB8GA1UdIwQYMBaAFD+Ac7akFxaMhBQAjVfvgGfY8hNKMAwGA1UdEwQFMAMBAf8w
DQYJKoZIhvcNAQELBQADggEBAC8L9D67CxIhGo5aGVu63WqRHBNOdo/FAGI7LURD
FeRmG5nRw/VXzJLGJksh4FSkx7aPrxNWF1uFiDZ80EuYQuIv7bDLblK31ZEbdg1R
9LgiZCdYSr464I7yXQY9o6FiNtSKZkQO8EsscJPPy/Zp4uHAnADWACkOUHiCbcKi
UUFu66dX0Wr/v53Gekz487GgVRs8HEeT9MU1reBKRgdENR8PNg4rbQfLc3YQKLWK
7yWnn/RenjDpuCiePj8N8/80tGgrNgK/6fzM3zI18sSywnXLswxqDb/J+jgVxnQ6
MrsTf1urM8MnfcxG/82oHIwfMh/sXPCZpo+DTLkhQxctJ3M=
-----END CERTIFICATE-----";

        static readonly IdentityProviderConfig identityProvider = new IdentityProviderConfig {
            metadata = "https://engine.surfconext.nl/authentication/proxy/idps-metadata/key:20140505",
            identity = "https://engine.surfconext.nl/authentication/proxy/idps-metadata/key:20140505",
            certificate = new X509Certificate2(Encoding.ASCII.GetBytes(conextCertificateData)),
        };

        [Fact]
        public void GetMetaData_returns_cached_instance_when_idp_and_sp_match()
        {
            var metadata1 = SsoProcessor.GetMetaData(identityProvider, new ServiceProviderConfig { entity = "http://test.progressnet.nl" });
            var metadata2 = SsoProcessor.GetMetaData(identityProvider, new ServiceProviderConfig { entity = "http://test.progressnet.nl" });

            Assert.Same(metadata1, metadata2);
        }

        [Fact]
        public void GetMetaData_returns_different_instances_for_different_providers()
        {
            var metadata1 = SsoProcessor.GetMetaData(identityProvider, new ServiceProviderConfig { entity = "http://test.progressnet.nl" });
            var metadata2 = SsoProcessor.GetMetaData(identityProvider, new ServiceProviderConfig { entity = "http://progresswww.nl/test" });

            Assert.NotSame(metadata1, metadata2);
        }
    }
}
