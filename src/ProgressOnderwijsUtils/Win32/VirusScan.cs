using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.System.Antimalware;

namespace ProgressOnderwijsUtils.Win32;

public static class VirusScan
{
    [SupportedOSPlatform("windows10.0.10240")]
    public static bool IsMalware(byte[] buffer, string contentName, string sessionName)
    {
        // to avoid HRESULT failure (0x80070057) while attempting AmsiScanBuffer
        if (buffer is []) {
            return false;
        }

        PInvoke.AmsiInitialize(sessionName, out var context).AssertResultOk(nameof(PInvoke.AmsiInitialize), sessionName);

        using (context) {
            var hresult = PInvoke.AmsiScanBuffer(new(context.DangerousGetHandle()), buffer, contentName, default(HAMSISESSION), out var result);
            hresult.AssertResultOk("AmsiScanBuffer", "");
            return ResultIsMalware(result);
        }
    }

    static bool ResultIsMalware(AMSI_RESULT result)
    {
        // Er is een gedocumenteerde methode AmsiResultIsMalware, maar deze is niet daadwerkelijk beschikbaar
        if (result == AMSI_RESULT.AMSI_RESULT_CLEAN) {
            return false;
        } else if (result == AMSI_RESULT.AMSI_RESULT_NOT_DETECTED) {
            return false;
        } else if (result is >= AMSI_RESULT.AMSI_RESULT_BLOCKED_BY_ADMIN_START and <= AMSI_RESULT.AMSI_RESULT_BLOCKED_BY_ADMIN_END) {
            throw new($"Scannen is geblokkeerd: AMSI_RESULT is: {result}");
        }
        return true;
    }
}
