using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace ProgressOnderwijsUtils.Win32;

public static class BitLocker
{
    public static int BitlockerStatusOfDriveLetter(char driveLetter)
    {
        var pszPath = $"{driveLetter}:";
        PInvoke.SHCreateItemFromParsingName(
            pszPath,
            null,
            Guid.Parse(typeof(IShellItem2).GetCustomAttribute<GuidAttribute>().AssertNotNull().Value),
            out var ppv_Void
        ).AssertResultOk(nameof(PInvoke.SHCreateItemFromParsingName), pszPath);
        var ppv = (IShellItem2)ppv_Void;

        const string pszName = "System.Volume.BitLockerProtection";
        var driveDebugSuffix = "\nfor:" + pszPath;
        PInvoke.PSGetPropertyKeyFromName(pszName, out var key).AssertResultOk(nameof(PInvoke.SHCreateItemFromParsingName), pszName + driveDebugSuffix);

        ppv.GetProperty(key, out var val);

        PInvoke.PropVariantToInt32(val, out var bitLockerStatus).AssertResultOk(nameof(PInvoke.PropVariantToInt32), $"vt={val.Anonymous.Anonymous.vt};{driveDebugSuffix}");

        return bitLockerStatus;
    }
}
