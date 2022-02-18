using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace ProgressOnderwijsUtils.AspNetCore;

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
            context.Response.Headers[HeaderNames.ContentSecurityPolicy] = options.ContentSecurityPolicy;
        }
        if (options.ReferrerPolicy != null) {
            context.Response.Headers["Referrer-Policy"] = options.ReferrerPolicy;
        }
        if (options.XContentTypeOptions != null) {
            context.Response.Headers[HeaderNames.XContentTypeOptions] = options.XContentTypeOptions;
        }

        return next(context);
    }
}
