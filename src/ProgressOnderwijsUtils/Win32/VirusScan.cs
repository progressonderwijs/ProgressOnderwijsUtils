using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Win32
{
    public static class VirusScan
    {
        public static unsafe bool IsMalware(byte[] buffer, string contentName, string sessionName)
        {
            fixed (void* bufferPtr = buffer) {
                using var scanSession = new VirusScanSessie(sessionName);
                PInvoke.AmsiScanBuffer(scanSession.Context, bufferPtr, (uint)buffer.LongLength, contentName, IntPtr.Zero, out var result);
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
            } else if (result >= AMSI_RESULT.AMSI_RESULT_BLOCKED_BY_ADMIN_START && result <= AMSI_RESULT.AMSI_RESULT_BLOCKED_BY_ADMIN_END) {
                throw new("Scannen is geblokkeerd");
            }
            return true;
        }
    }

    public sealed class VirusScanSessie : IDisposable
    {
        public readonly IntPtr Context;

        public VirusScanSessie(string appName)
            => PInvoke.AmsiInitialize(appName, out Context);

        public void Dispose()
            => PInvoke.AmsiUninitialize(Context);
    }
}
