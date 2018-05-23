using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace ProgressOnderwijsUtils.AspNetCore
{
    public sealed class RichRequestTelemetryInitializer : ITelemetryInitializer
    {
        readonly HttpContextAccessor _httpContextAccessor;

        public RichRequestTelemetryInitializer(HttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && telemetry is RequestTelemetry requestTelemetry) {
                // Application Insights doesn't store IP addresses for privacy reasons.
                requestTelemetry.Properties["ProgressRemoteIpAddress"] = context.Connection.RemoteIpAddress.ToString();

                // No idea why Application Insights doesn't send user agent by default.
                requestTelemetry.Properties["ProgressUserAgent"] = context.Request.Headers[HeaderNames.UserAgent];
            }
        }
    }
}
