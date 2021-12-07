namespace ProgressOnderwijsUtils.AspNetCore
{
    public sealed class SecurityHeadersMiddlewareOptions
    {
        public string? ContentSecurityPolicy { get; set; } = "object-src 'self'; script-src 'self';";
        public string? ReferrerPolicy { get; set; } = "same-origin";
        public string? XContentTypeOptions { get; set; } = "nosniff";
    }
}
