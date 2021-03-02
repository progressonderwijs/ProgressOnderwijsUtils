using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Win32
{
    public static class VirusScan
    {
        public static bool IsMalware(string content, string contentName)
        {
            using var scanSession = new VirusScanSessie("Progress.Net");
            PInvoke.AmsiScanString(scanSession.Context, content, contentName, IntPtr.Zero, out var result);
            return ResultIsMalware(result);
        }

        public static bool IsMalware(byte[] buffer, string contentName) => IsMalwareUnsafe(buffer, contentName);

        static unsafe bool IsMalwareUnsafe(byte[] buffer, string contentName)
        {
            var sizet = Marshal.SizeOf(typeof(byte)) * buffer.Length;
            var bufferPtr = Marshal.AllocHGlobal(sizet).ToPointer();

            using var scanSession = new VirusScanSessie("Progress.Net");
            PInvoke.AmsiScanBuffer(scanSession.Context, bufferPtr, (uint)buffer.LongLength, contentName, IntPtr.Zero, out var result);
            return ResultIsMalware(result);
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
