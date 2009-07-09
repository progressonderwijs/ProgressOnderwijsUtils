using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Enums.Support
{

	internal static class EnumConverterHelper<TEnum, TLabel, TAttr>
		where TEnum : struct
		where TAttr : Attribute, IHasLabel<TLabel>
	{
		internal class ConverterImpl : IConverter<TAttr, TLabel>
		{
			public TLabel Convert(TAttr val) { return val.Label; }
		}

		internal static EnumConverterImplementation<TEnum, TLabel, TAttr, ConverterImpl> Converter { get { return EnumConverterImplementation<TEnum, TLabel, TAttr, ConverterImpl>.Converter; } }
	}

}
