/* TODO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProgressOnderwijsUtils.WebSupport
{
    public static class CommonHttpSecurityModules
    {
        /// <summary>
        /// Configures the app to redirect HTTP to HTTPS, use HSTS, remove some unnecessarily identifying server headers, and mark session cookies as HttpOnly+Secure.
        /// </summary>
        public static void RegisterCommonSecurityModules(HttpApplication app) {
            new HstsAndHttpsRedirectModule().Init(app);
            new RemoveServerHeadersModule().Init(app);
            new SecureHttpOnlySessionCookieModule().Init(app);
        }
    }
}
*/