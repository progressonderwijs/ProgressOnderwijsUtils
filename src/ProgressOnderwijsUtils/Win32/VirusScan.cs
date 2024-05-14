using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.System.Antimalware;

namespace ProgressOnderwijsUtils.Win32;

public static class VirusScan
{
    [SupportedOSPlatform("windows10.0.10240")]
    public static unsafe bool IsMalware(byte[] buffer, string contentName, string sessionName)
    {
        var context = default(HAMSICONTEXT);

        try {
            PInvoke.AmsiInitialize(sessionName, out context).AssertResultOk(nameof(PInvoke.AmsiInitialize), sessionName);

            fixed (void* bufferPtr = buffer) {
                var hresult = PInvoke.AmsiScanBuffer(context, bufferPtr, (uint)buffer.LongLength, contentName, default(HAMSISESSION), out var result);
                hresult.AssertResultOk("AmsiScanBuffer", "");
                return ResultIsMalware(result);
            }
        } finally {
            if (context != default(HAMSICONTEXT)) {
                PInvoke.AmsiUninitialize(context);
            }
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
