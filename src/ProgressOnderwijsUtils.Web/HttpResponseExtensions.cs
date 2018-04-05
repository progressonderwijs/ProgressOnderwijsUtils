using System.Web;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Web
{
    public static class HttpResponseExtensions
    {
        public static void AddContentDispositionHeader([NotNull] this HttpResponse response, string fileName)
        {
            response.AddHeader("Content-Disposition", $"attachment;filename=\"{fileName}\";");
        }

        public static void AddContentDispositionHeader([NotNull] this HttpResponseBase response, string fileName)
        {
            response.AddHeader("Content-Disposition", $"attachment;filename=\"{fileName}\";");
        }
    }
}