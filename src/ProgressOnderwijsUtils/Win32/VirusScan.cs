using System;
using Microsoft.Windows.Sdk;

namespace ProgressOnderwijsUtils.Win32
{
    public static class VirusScan
    {
        public static unsafe bool IsMalware(byte[] buffer, string contentName, string sessionName)
        {
            var context = default(nint);

            try {
                PInvoke.AmsiInitialize(sessionName, out context).AssertResultOk();

                fixed (void* bufferPtr = buffer) {
                    PInvoke.AmsiScanBuffer(context, bufferPtr, (uint)buffer.LongLength, contentName, IntPtr.Zero, out var result);
                    return ResultIsMalware(result);
                }
            } finally {
                if (context != default(nint)) {
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
            } else if (result >= AMSI_RESULT.AMSI_RESULT_BLOCKED_BY_ADMIN_START && result <= AMSI_RESULT.AMSI_RESULT_BLOCKED_BY_ADMIN_END) {
                throw new("Scannen is geblokkeerd");
            }
            return true;
        }
    }
}
