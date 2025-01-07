namespace ProgressOnderwijsUtils.AspNetCore;

public sealed record SecurityHeadersMiddlewareOptions
{
    public string? ContentSecurityPolicy { get; init; } = "default-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'";
    public string? PermissionsPolicy { get; init; } = "microphone=(), camera=(), fullscreen=(), geolocation=(), display-capture=()";
    public string? ReferrerPolicy { get; init; } = "same-origin";
    public string? XContentTypeOptions { get; init; } = "nosniff";
    public string? CrossOriginEmbedderPolicy { get; init; } = "require-corp";
    public string? CrossOriginOpenerPolicy { get; init; } = "same-origin";
}
