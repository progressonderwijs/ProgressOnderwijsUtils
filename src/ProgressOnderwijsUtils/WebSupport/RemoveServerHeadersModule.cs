using System;
using System.Web;

namespace ProgressOnderwijsUtils.WebSupport
{
    public class RemoveServerHeadersModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.BeginRequest += RemoveIdentifyingHeaders;
        }

        static void RemoveIdentifyingHeaders(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            var responseHeaders = context.Response.Headers;

            responseHeaders.Remove("Server");
        }

        public void Dispose() { }
    }
}
