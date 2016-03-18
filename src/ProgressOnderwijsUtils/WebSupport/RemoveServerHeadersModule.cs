using System.Web;

namespace ProgressOnderwijsUtils.WebSupport
{
    public class RemoveServerHeadersModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.BeginRequest += (o, args) => RemoveIdentifyingHeaders((HttpApplication)o);
        }

        static void RemoveIdentifyingHeaders(HttpApplication sender)
        {
            var context = sender.Context;
            var responseHeaders = context.Response.Headers;

            responseHeaders.Remove("Server");
        }

        public void Dispose() { }
    }
}
