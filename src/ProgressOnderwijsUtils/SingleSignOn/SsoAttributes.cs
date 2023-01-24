namespace ProgressOnderwijsUtils.SingleSignOn;

public readonly record struct SsoAttributes()
{
    public required string Uid { get; init; }
    public required string? Domain { get; init; }
    public string[] Email { get; init; } = Array.Empty<string>();
    public string[] Roles { get; init; } = Array.Empty<string>();
    public string? InResponseTo { get; init; }
    public string? AuthnContextClassRef { get; init; }
    public XElement? RawAssertion { get; init; }

    public override string ToString()
        => $"uid='{Uid}'; domain='{Domain}'; emails='{StringUtils.ToFlatDebugString(Email)}'; roles='{StringUtils.ToFlatDebugString(Roles)}'";
}
