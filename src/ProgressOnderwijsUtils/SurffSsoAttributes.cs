using System;
using System.Collections.Generic;

namespace ProgressOnderwijsUtils
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
