using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace ProgressOnderwijsUtils.Win32
{
    public static class BitLocker
    {
        public static unsafe int BitlockerStatusOfDriveLetter(char driveLetter)
        {
            void* ppv = null;

            try {
                PInvoke.SHCreateItemFromParsingName(
                    $"{driveLetter}:",
                    null,
                    Guid.Parse(typeof(IShellItem2).GetCustomAttribute<GuidAttribute>().AssertNotNull().Value),
                    out ppv
                ).AssertResultOk();

                PInvoke.PSGetPropertyKeyFromName("System.Volume.BitLockerProtection", out var key).AssertResultOk();

                ((IShellItem2*)ppv)->GetProperty(key, out var val).AssertResultOk();

                PInvoke.PropVariantToInt32(val, out var bitLockerStatus).AssertResultOk();

                return bitLockerStatus;
            } finally {
                if (ppv != null) {
                    _ = ((IShellItem2*)ppv)->Release();
                }
            }
        }
    }
}
