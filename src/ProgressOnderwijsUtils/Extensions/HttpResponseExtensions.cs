﻿using System.Web;

namespace ProgressOnderwijsUtils
{
    public static class HttpResponseExtensions
    {
        public static void AddContentDispositionHeader(this HttpResponse response, string fileName)
        {
            response.AddHeader("Content-Disposition", $"attachment;filename=\"{fileName}\";");
        }
    }
}
