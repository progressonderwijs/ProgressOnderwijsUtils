using System;
using System.Web;

namespace ProgressOnderwijsUtils.WebSupport
{
    public class HstsAndHttpsRedirectModule : IHttpModule
    {
        public void Init(HttpApplication app) { app.BeginRequest += context_BeginRequest; }

        static void context_BeginRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (context.Request.IsSecureConnection) {
                context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000");
            } else {
                context.Response.RedirectPermanent(new UriBuilder(context.Request.Url) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri.ToString());
                context.Response.End();
            }
        }

        public void Dispose() { }
    }
}
