using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProgressOnderwijsUtils
{
    public static class HttpResponseExtensions
    {
        public static void AddContentDispositionHeader(this HttpResponse response, string fileName)
        {
            response.AddHeader("Content-Disposition", string.Format("attachment;filename=\"{0}\";", fileName));
        }
    }
}
