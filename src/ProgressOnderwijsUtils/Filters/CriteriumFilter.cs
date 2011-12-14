﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public sealed class CriteriumFilter : FilterBase, IEquatable<CriteriumFilter>
	{
		readonly string _KolomNaam;
		readonly BooleanComparer _Comparer;
		readonly object _Waarde;

		public string KolomNaam { get { return _KolomNaam; } }
		public BooleanComparer Comparer { get { return _Comparer; } }
		public object Waarde { get { return _Waarde; } }

		public override bool Equals(object obj) { return Equals(obj as CriteriumFilter); }
		public override bool Equals(FilterBase other) { return Equals(other as CriteriumFilter); }
		public bool Equals(CriteriumFilter other) { return other != null && _KolomNaam == other.KolomNaam && _Comparer == other._Comparer && Equals(_Waarde, other._Waarde); }
		public override int GetHashCode() { return Tuple.Create(_KolomNaam, _Comparer, _Waarde).GetHashCode() + 2; }

		public static BooleanComparer[] StringComparers { get { return new[] { BooleanComparer.Contains, BooleanComparer.Equal, BooleanComparer.NotEqual, BooleanComparer.StartsWith, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }
		public static BooleanComparer[] NumericComparers { get { return new[] { BooleanComparer.Equal, BooleanComparer.GreaterThan, BooleanComparer.GreaterThanOrEqual, BooleanComparer.LessThan, BooleanComparer.LessThanOrEqual, BooleanComparer.NotEqual, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }

		internal CriteriumFilter(string kolomnaam, BooleanComparer comparer, object waarde) { _KolomNaam = kolomnaam; _Comparer = comparer; _Waarde = waarde; }

		protected internal override QueryBuilder ToQueryBuilderImpl()
		{
			switch (Comparer)
			{
				case BooleanComparer.LessThan:
					return KolomNaam + "<" + BuildParam();
				case BooleanComparer.LessThanOrEqual:
					return KolomNaam + "<=" + BuildParam();
				case BooleanComparer.Equal:
					return KolomNaam + "=" + BuildParam();
				case BooleanComparer.GreaterThanOrEqual:
					return KolomNaam + ">=" + BuildParam();
				case BooleanComparer.GreaterThan:
					return KolomNaam + ">" + BuildParam();
				case BooleanComparer.NotEqual:
					return KolomNaam + "!=" + BuildParam();
				case BooleanComparer.In:
					if (Waarde is GroupReference)
					{
						return KolomNaam + " in (select keyint0 from statischegroepslid where groep = " + QueryBuilder.Param((Waarde as GroupReference).GroupId) + ")";
					}
					else
					{
						string[] nrs = Waarde.ToString().Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
						string clause = KolomNaam + " in (" + Enumerable.Range(0, nrs.Length).Select(n => "{" + n + "}").JoinStrings(", ") + ")";
						return QueryBuilder.Create(clause, nrs.Cast<object>().ToArray());
					}
				case BooleanComparer.StartsWith:
					return KolomNaam + " like " + QueryBuilder.Param(Waarde + "%");
				case BooleanComparer.Contains:
					return KolomNaam + " like " + QueryBuilder.Param("%" + Waarde + "%");
				case BooleanComparer.IsNull:
					return QueryBuilder.Create(KolomNaam + " is null");
				case BooleanComparer.IsNotNull:
					return QueryBuilder.Create(KolomNaam + " is not null");
				default:
					throw new InvalidOperationException("Geen geldige operator");
			}
		}

		QueryBuilder BuildParam()
		{
			if (Waarde is ColumnReference)
				return QueryBuilder.Create(((ColumnReference)Waarde).ColumnName);
			else
				return QueryBuilder.Param(Waarde);
		}

		protected internal override IEnumerable<string> ColumnsReferenced { get { yield return KolomNaam; if (Waarde is ColumnReference) yield return ((ColumnReference)Waarde).ColumnName; } }

		protected internal override FilterBase ReplaceImpl(FilterBase toReplace, FilterBase replaceWith) { return this == toReplace ? replaceWith : this; }

		protected internal override FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c)
		{
			return filterInEditMode == this
				? Filter.CreateCombined(booleanOperator, this, c)
				: this;
		}

		public override string ToString()
		{
			if (Waarde is GroupReference && Comparer == BooleanComparer.In)
				return string.Format("{0} in {1}", KolomNaam, (Waarde as GroupReference).Name);
			else
				return base.ToString();
		}
	}
}
