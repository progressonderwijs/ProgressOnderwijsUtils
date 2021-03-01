using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace ProgressOnderwijsUtils.Win32
{
    public static class BitLocker
    {
        static void Check(HRESULT result)
        {
            if (result.Value != 0) {
                throw new Exception("HRESULT!=0");
            }
        }

        public static unsafe int BitlockerStatusOfDriveLetter(char driveLetter)
        {
            Check(PInvoke.SHCreateItemFromParsingName(
                $"{driveLetter}:",
                null,
                Guid.Parse(typeof(IShellItem2).GetCustomAttribute<GuidAttribute>().AssertNotNull().Value),
                out var ppv
            ));

            Check(PInvoke.PSGetPropertyKeyFromName("System.Volume.BitLockerProtection", out var key));

            Check(((IShellItem2*)ppv)->GetProperty(key, out var val));

            Check(PInvoke.PropVariantToInt32(val, out var bitLockerStatus));

            return bitLockerStatus;
        }
    }
}
