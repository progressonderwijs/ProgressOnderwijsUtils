using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;

namespace ProgressOnderwijsUtils.Win32
{
    public static class Win32Extentions
    {
        internal static void AssertResultOk(this HRESULT result)
        {
            if (result.Value != 0) {
                throw new Exception("HRESULT!=0");
            }
        }
    }
}
