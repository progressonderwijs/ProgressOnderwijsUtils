using Windows.Win32.Foundation;

namespace ProgressOnderwijsUtils.Win32;

public static class Win32Extentions
{
    const int INPLACE_S_TRUNCATED = 0x401a0;

    internal static void AssertResultOk(this HRESULT result, string attemptingAction, string context)
    {
        if (result.Value is not (0 or INPLACE_S_TRUNCATED)) {
            throw new($"HRESULT failure (0x{result.Value:x}) while attempting {attemptingAction}\nContext:{context}");
        }
    }
}
