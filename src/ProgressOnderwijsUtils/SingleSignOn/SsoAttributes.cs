namespace ProgressOnderwijsUtils.SingleSignOn;

public readonly record struct SsoAttributes()
{
    public string uid { get; init; }
    public string? domain { get; init; }
    public string[] email { get; init; } = Array.Empty<string>();
    public string[] roles { get; init; } = Array.Empty<string>();
    public string? InResponseTo { get; init; }
    public string? AuthnContextClassRef { get; init; }
    public XElement? RawAssertion { get; init; }

    public override string ToString()
        => $"uid='{uid}'; domain='{domain}'; emails='{StringUtils.ToFlatDebugString(email)}'; roles='{StringUtils.ToFlatDebugString(roles)}'";
}
