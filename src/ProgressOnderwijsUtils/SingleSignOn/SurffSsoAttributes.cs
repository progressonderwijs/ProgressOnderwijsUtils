using System;
using ProgressOnderwijsUtils;

namespace Progress.Business.Tools
{
    [Serializable]
    public struct SurffSsoAttributes
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
