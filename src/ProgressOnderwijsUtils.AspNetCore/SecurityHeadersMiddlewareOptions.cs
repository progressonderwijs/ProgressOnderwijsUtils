namespace ProgressOnderwijsUtils.AspNetCore;

public sealed record SecurityHeadersMiddlewareOptions
{
    public string? ContentSecurityPolicy { get; init; } = "object-src 'self'; script-src 'self';";
    public string? PermissionsPolicy { get; init; } = "microphone=(), camera=(), fullscreen=(), geolocation=(), display-capture=()";
    /// <summary>
    /// Legacy header name for Permissions-Policy used in safari 11.1+ and firefox 74+
    /// </summary>
    public string? ReferrerPolicy { get; init; } = "same-origin";
    public string? XContentTypeOptions { get; init; } = "nosniff";
    public string? CrossOriginEmbedderPolicy { get; init; } = "require-corp";
    public string? CrossOriginOpenerPolicy { get; init; } = "same-origin";
}
