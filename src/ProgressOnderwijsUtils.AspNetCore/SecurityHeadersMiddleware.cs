﻿using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace ProgressOnderwijsUtils.AspNetCore
{
    public sealed class SecurityHeadersMiddleware
    {
        readonly RequestDelegate next;
        readonly SecurityHeadersMiddlewareOptions options;

        public SecurityHeadersMiddleware([NotNull] RequestDelegate next, [NotNull] SecurityHeadersMiddlewareOptions options)
        {
            this.next = next;
            this.options = options;
        }

        public Task Invoke([NotNull] HttpContext context)
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

            return next(context);
        }
    }
}
