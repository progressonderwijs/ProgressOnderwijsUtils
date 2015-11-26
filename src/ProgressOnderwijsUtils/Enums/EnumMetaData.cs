using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    struct EnumMetaData<TEnum> : IEnumMetaData
        where TEnum : struct, IConvertible, IComparable
    {
        public EnumMetaData(TEnum value)
        {
            EnumValue = value;
        }

        public TEnum EnumValue { get; }
        public Enum UntypedEnumValue => (Enum)(object)EnumValue;
        public ITranslatable Label => EnumMetaDataCache<TEnum>.Instance.GetLabel(EnumValue);

        public IEnumerable<TAttr> Attributes<TAttr>()
            where TAttr : Attribute
            => EnumMetaDataCache<TEnum>.Instance.AllAttributes(EnumValue).OfType<TAttr>();
    }
}
