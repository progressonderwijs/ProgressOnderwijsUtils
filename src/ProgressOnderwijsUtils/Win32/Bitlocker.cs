using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace ProgressOnderwijsUtils.Win32;

public static class BitLocker
{
    public static int BitlockerStatusOfDriveLetter(char driveLetter)
    {
        var pszPath = $"{driveLetter}:";
        PInvoke.SHCreateItemFromParsingName(pszPath, null, out IShellItem2 ppv)
            .AssertResultOk(nameof(PInvoke.SHCreateItemFromParsingName), pszPath);

        const string pszName = "System.Volume.BitLockerProtection";
        var driveDebugSuffix = "\nfor:" + pszPath;
        PInvoke.PSGetPropertyKeyFromName(pszName, out var key).AssertResultOk(nameof(PInvoke.SHCreateItemFromParsingName), pszName + driveDebugSuffix);

        ppv.GetProperty(key, out var val);

        PInvoke.PropVariantToInt32(val, out var bitLockerStatus).AssertResultOk(nameof(PInvoke.PropVariantToInt32), $"vt={val.Anonymous.Anonymous.vt};{driveDebugSuffix}");

        return bitLockerStatus;
    }
}
