using System;
using System.Web;

namespace ProgressOnderwijsUtils.WebSupport
{
    public class HstsAndHttpsRedirectModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.BeginRequest += (o, args) => context_BeginRequest((HttpApplication)o);
        }

        static void context_BeginRequest(HttpApplication sender)
        {
            var context = sender.Context;

            if (!context.Request.IsSecureConnection) {
                context.Response.RedirectPermanent(new UriBuilder(context.Request.Url) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri.ToString());
                context.Response.End();
            } else if (context.Request.Headers.Get("Host") != "localhost") {
                context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000");
            }
        }

        public void Dispose() { }
    }
}
