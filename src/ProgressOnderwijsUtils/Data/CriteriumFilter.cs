using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ProgressOnderwijsUtils.Data;
using System.Text.RegularExpressions;

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
					return kolomNaamMapped + "<" + BuildParam(colRename);
				case BooleanComparer.LessThanOrEqual:
					return kolomNaamMapped + "<=" + BuildParam(colRename);
				case BooleanComparer.Equal:
					return kolomNaamMapped + "=" + BuildParam(colRename);
				case BooleanComparer.GreaterThanOrEqual:
					return kolomNaamMapped + ">=" + BuildParam(colRename);
				case BooleanComparer.GreaterThan:
					return kolomNaamMapped + ">" + BuildParam(colRename);
				case BooleanComparer.NotEqual:
					return kolomNaamMapped + "!=" + BuildParam(colRename);
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
					throw new InvalidOperationException ("Geen geldige operator");
			}
		}

		QueryBuilder BuildParam(Func<string, string> colRename)
		{
			if (Waarde is ColumnReference)
				return QueryBuilder.Create(colRename(((ColumnReference)Waarde).ColumnName));
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
	}

	[Serializable]
	public class ColumnReference
	{
		static readonly Regex okname = new Regex(@"^\w+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
		public readonly string ColumnName;

		public ColumnReference(string colname)
		{
			if (colname == null) throw new ArgumentNullException("colname");
			else if (!okname.IsMatch(colname)) throw new ArgumentException("Geen valide kolomnaam " + colname, "colname");
			ColumnName = colname;
		}
	}
}
