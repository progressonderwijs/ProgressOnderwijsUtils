using Microsoft.Windows.Sdk;

namespace ProgressOnderwijsUtils.Win32;

public static class Win32Extentions
{
    internal static void AssertResultOk(this HRESULT result, string attemptingAction, string context)
    {
        if (result.Value != 0) {
            throw new($"HRESULT failure ({result.Value}) while attempting {attemptingAction}\nContext:{context}");
        }
    }
}
