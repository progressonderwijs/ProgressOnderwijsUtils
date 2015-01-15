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
            return string.Format(
                "uid='{0}'; domain='{1}'; emails='{2}'; roles='{3}'",
                uid,
                domain,
                StringUtils.ToFlatDebugString(email),
                StringUtils.ToFlatDebugString(roles));
        }
    }
}