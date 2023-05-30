using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace ProgressOnderwijsUtils.Win32;

public static class BitLocker
{
    public static unsafe int BitlockerStatusOfDriveLetter(char driveLetter)
    {
        IShellItem2* ppv = null;

        try {
            var pszPath = $"{driveLetter}:";
            PInvoke.SHCreateItemFromParsingName(
                pszPath,
                null,
                Guid.Parse(typeof(IShellItem2).GetCustomAttribute<GuidAttribute>().AssertNotNull().Value),
                out var ppv_Void
            ).AssertResultOk(nameof(PInvoke.SHCreateItemFromParsingName), pszPath);
            ppv = (IShellItem2*)ppv_Void;

            const string pszName = "System.Volume.BitLockerProtection";
            var driveDebugSuffix = "\nfor:" + pszPath;
            PInvoke.PSGetPropertyKeyFromName(pszName, out var key).AssertResultOk(nameof(PInvoke.SHCreateItemFromParsingName), pszName + driveDebugSuffix);

            ppv->GetProperty(key, out var val).AssertResultOk(nameof(IShellItem2) + "." + nameof(IShellItem2.GetProperty), $"fmtid={key.fmtid};pid:{key.pid};{driveDebugSuffix}");

            PInvoke.PropVariantToInt32(val, out var bitLockerStatus).AssertResultOk(nameof(PInvoke.PropVariantToInt32), $"vt={val.Anonymous.Anonymous.vt};{driveDebugSuffix}");

            return bitLockerStatus;
        } finally {
            if (ppv != null) {
                _ = ppv->Release();
            }
        }
    }
}
