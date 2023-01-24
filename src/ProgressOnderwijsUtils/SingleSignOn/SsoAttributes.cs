namespace ProgressOnderwijsUtils.SingleSignOn;

public struct SsoAttributes
{
    public string uid;
    public string? domain;
    public string[] email;
    public string[] roles;
    public string? InResponseTo;
    public string? AuthnContextClassRef;
    public XElement RawAssertion;

    public override string ToString()
        => $"uid='{uid}'; domain='{domain}'; emails='{StringUtils.ToFlatDebugString(email)}'; roles='{StringUtils.ToFlatDebugString(roles)}'";
}
