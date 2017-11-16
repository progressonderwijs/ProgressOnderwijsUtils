using System;
using System.Web;

namespace ProgressOnderwijsUtils.Web
{
    public class HstsAndHttpsRedirectModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.BeginRequest += OnBeginRequest;
        }

        static void OnBeginRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (!context.Request.IsSecureConnection) {
                context.Response.RedirectPermanent(new UriBuilder(context.Request.Url) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri.ToString());
                context.Response.End();
                return;
            }

            var hostHeader = context.Request.Headers.Get("Host");
            if (hostHeader == "localhost" || hostHeader != null && hostHeader.StartsWith("localhost:")) {
                return;
            }
            context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000");
        }

        public void Dispose() { }
    }
}
