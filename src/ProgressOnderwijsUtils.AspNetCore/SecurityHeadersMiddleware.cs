using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ProgressOnderwijsUtils.AspNetCore
{
    public sealed class SecurityHeadersMiddleware
    {
        readonly RequestDelegate next;
        readonly SecurityHeadersMiddlewareOptions options;

        public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersMiddlewareOptions options)
        {
            this.next = next;
            this.options = options;
        }

        public Task Invoke(HttpContext context)
        {
            if (options.ContentSecurityPolicy != null) {
                context.Response.Headers["Content-Security-Policy"] = options.ContentSecurityPolicy;
            }

            if (options.ReferrerPolicy != null) {
                context.Response.Headers["Referrer-Policy"] = options.ReferrerPolicy;
            }

            if (options.XContentTypeOptions != null) {
                context.Response.Headers["X-Content-Type-Options"] = options.XContentTypeOptions;
            }

            if (options.XFrameOptions != null) {
                context.Response.Headers["X-Frame-Options"] = options.XFrameOptions;
            }

            if (options.XXssProtection != null) {
                context.Response.Headers["X-XSS-Protection"] = options.XXssProtection;
            }

            return next(context);
        }
    }
}
