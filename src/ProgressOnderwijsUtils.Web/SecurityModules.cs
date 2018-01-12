using System.Linq;
using System.Web;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Web
{
    public static class CommonHttpSecurityModules
    {
        /// <summary>
        /// Configures the app to redirect HTTP to HTTPS, use HSTS, remove some unnecessarily identifying server headers, and mark session cookies as HttpOnly+Secure.
        /// </summary>
        public static void RegisterCommonSecurityModules([NotNull] HttpApplication app) {
            new HstsAndHttpsRedirectModule().Init(app);
            new RemoveServerHeadersModule().Init(app);
            new SecureHttpOnlySessionCookieModule().Init(app);
        }
    }
}
