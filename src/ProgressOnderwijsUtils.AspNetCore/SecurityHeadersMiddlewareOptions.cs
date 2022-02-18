namespace ProgressOnderwijsUtils.AspNetCore;

public sealed record SecurityHeadersMiddlewareOptions
{
    public string? ContentSecurityPolicy { get; init; } = "object-src 'self'; script-src 'self';";
    public string? ReferrerPolicy { get; init; } = "same-origin";
    public string? XContentTypeOptions { get; init; } = "nosniff";
    public string? CrossOriginEmbedderPolicy { get; init; } = "require-corp";
    public string? CrossOriginOpenerPolicy { get; init; } = "same-origin";
}
