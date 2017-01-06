using System;
using System.Web;

namespace ProgressOnderwijsUtils.WebSupport
{
    public class SecureHttpOnlySessionCookieModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.EndRequest += OnEndRequest;
        }

        static void OnEndRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            var cookies = context.Response.Cookies;
            var cookieCount = cookies.Count;
            for (int i=0;i<cookieCount;i++) {
                cookies.Get("ASP.NET_SessionId");
                //cookies.Get("ASP.NET_SessionId") has a nasty side-effect (sets the cookie), so we loop instead.
                var cookie = cookies[i];
                if (cookie.Name == "ASP.NET_SessionId") {
                    cookie.Secure = true;
                    cookie.HttpOnly = true;
                }
            }
        }

        public void Dispose() { }
    }
}
