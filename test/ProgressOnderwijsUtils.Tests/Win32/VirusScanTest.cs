using System.Runtime.Versioning;
using ProgressOnderwijsUtils.Win32;

namespace ProgressOnderwijsUtils.Tests.Win32;

public sealed class VirusScanTest
{
    const string virusString = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
    const string schoneString = "Dit is geen virus";

    [FactIgnoreOnAppVeyor]
    [SupportedOSPlatform("windows10.0.10240")]
    public void VirusAlsByte_wordt_herkend()
        => PAssert.That(() => VirusScan.IsMalware(Encoding.ASCII.GetBytes(virusString), "EICAR", "Utils test"));

    [FactIgnoreOnAppVeyor]
    [SupportedOSPlatform("windows10.0.10240")]
    public void SchoneString_als_byte_wordt_niet_als_virus_herkend()
        => PAssert.That(() => !VirusScan.IsMalware(Encoding.ASCII.GetBytes(schoneString), "Progress.Net", "Utils test"));
}

public sealed class FactIgnoreOnAppVeyor : FactAttribute
{
    public FactIgnoreOnAppVeyor()
    {
        if (Environment.GetEnvironmentVariable("APPVEYOR") != null) {
            Skip = "Ignore when run via AppVeyor";
        }
    }
}
