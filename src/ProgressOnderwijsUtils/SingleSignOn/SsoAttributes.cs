using System;

namespace ProgressOnderwijsUtils.SingleSignOn
{
    [Serializable]
    public struct SsoAttributes
    {
        public string uid;
        public string domain;
        public string[] email;
        public string[] roles;

        public override string ToString()
        {
            return $"uid='{uid}'; domain='{domain}'; emails='{StringUtils.ToFlatDebugString(email)}'; roles='{StringUtils.ToFlatDebugString(roles)}'";
        }
    }
}
