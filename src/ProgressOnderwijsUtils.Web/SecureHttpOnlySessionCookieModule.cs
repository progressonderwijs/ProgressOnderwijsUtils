using System;
using System.Web;

namespace ProgressOnderwijsUtils.Web
{
    public class SecureHttpOnlySessionCookieModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            var cookies = context.Response.Cookies;
            var cookieCount = cookies.Count;
            for (var i = 0; i < cookieCount; i++) {
                //cookies.Get("ASP.NET_SessionId") has a nasty side-effect (sets the cookie), so we loop instead.
                var cookie = cookies[i] ?? throw new Exception("cookie == null: this should be impossible");
                if (cookie.Name == "ASP.NET_SessionId" || cookie.Name.StartsWith("ASPSESSIONID", StringComparison.Ordinal)) {
                    cookie.Secure = true;
                    cookie.HttpOnly = true;
                }
            }
        }

        public void Dispose() { }
    }
}
