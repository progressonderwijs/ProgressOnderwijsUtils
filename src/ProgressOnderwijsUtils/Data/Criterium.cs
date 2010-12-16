﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public sealed class CriteriumFilter : FilterBase
	{
		readonly string _KolomNaam;
		readonly BooleanComparer _Comparer;
		readonly object _Waarde;

		public string KolomNaam { get { return _KolomNaam; } }
		public BooleanComparer Comparer { get { return _Comparer; } }
		public object Waarde { get { return _Waarde; } }

		public static BooleanComparer[] StringComparers { get { return new[] { BooleanComparer.Contains, BooleanComparer.Equal, BooleanComparer.NotEqual, BooleanComparer.StartsWith, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }
		public static BooleanComparer[] NumericComparers { get { return new[] { BooleanComparer.Equal, BooleanComparer.GreaterThan, BooleanComparer.GreaterThanOrEqual, BooleanComparer.LessThan, BooleanComparer.LessThanOrEqual, BooleanComparer.NotEqual, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }

		internal CriteriumFilter(string kolomnaam, BooleanComparer comparer, object waarde) { _KolomNaam = kolomnaam; _Comparer = comparer; _Waarde = waarde; }

		protected internal override QueryBuilder ToSqlStringImpl(Func<string, string> colRename)
		{
			string kolomNaamMapped = colRename(KolomNaam);
			switch (Comparer)
			{
				case BooleanComparer.LessThan:
					return kolomNaamMapped + "<" + QueryBuilder.Param(Waarde);
				case BooleanComparer.LessThanOrEqual:
					return kolomNaamMapped + "<=" + QueryBuilder.Param(Waarde);
				case BooleanComparer.Equal:
					return kolomNaamMapped + "=" + QueryBuilder.Param(Waarde);
				case BooleanComparer.GreaterThanOrEqual:
					return kolomNaamMapped + ">=" + QueryBuilder.Param(Waarde);
				case BooleanComparer.GreaterThan:
					return kolomNaamMapped + ">" + QueryBuilder.Param(Waarde);
				case BooleanComparer.NotEqual:
					return kolomNaamMapped + "!=" + QueryBuilder.Param(Waarde);
				case BooleanComparer.In:
					string[] nrs = Waarde.ToString().Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					string clause = kolomNaamMapped + " in (" + Enumerable.Range(0, nrs.Length).Select(n => "{" + n + "}").JoinStrings(", ") + ")";
					return QueryBuilder.Create(clause, nrs.Cast<object>().ToArray());
				case BooleanComparer.StartsWith:
					return kolomNaamMapped + " like " + QueryBuilder.Param(Waarde + "%");
				case BooleanComparer.Contains:
					return kolomNaamMapped + " like " + QueryBuilder.Param("%" + Waarde + "%");
				case BooleanComparer.IsNull:
					return QueryBuilder.Create(kolomNaamMapped + " is null");
				case BooleanComparer.IsNotNull:
					return QueryBuilder.Create(kolomNaamMapped + " is not null");
				default:
					throw new Exception("Geen geldige operator");
			}
		}

		protected internal override FilterBase ReplaceImpl(FilterBase toReplace, CriteriumFilter replaceWith) { return this == toReplace ? replaceWith : this; }

		protected internal override FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, CriteriumFilter c)
		{
			return filterInEditMode == this
				? Filter.CreateCombined(booleanOperator, this, c)
				: this;
		}
	}
}
