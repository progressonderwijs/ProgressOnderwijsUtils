using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public class Criterium
	{
		readonly string _KolomNaam;
		readonly BooleanComparer _Comparer;
		readonly object _Waarde;

		public string KolomNaam { get { return _KolomNaam; } }
		public BooleanComparer Comparer { get { return _Comparer; } }
		public object Waarde { get { return _Waarde; } }

		public static BooleanComparer[] StringComparers { get { return new[] { BooleanComparer.Contains, BooleanComparer.Equal, BooleanComparer.NotEqual, BooleanComparer.StartsWith, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }
		public static BooleanComparer[] NumericComparers { get { return new[] { BooleanComparer.Equal, BooleanComparer.GreaterThan, BooleanComparer.GreaterThanOrEqual, BooleanComparer.LessThan, BooleanComparer.LessThanOrEqual, BooleanComparer.NotEqual, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }

		public Criterium(string kolomnaam, BooleanComparer comparer, object waarde) { _KolomNaam = kolomnaam; _Comparer = comparer; _Waarde = waarde; }

		public string ToString(List<object> pars)
		{
			switch (Comparer)
			{
				case BooleanComparer.LessThan:
					pars.Add(Waarde);
					return KolomNaam + "<" + "{" + (pars.Count - 1) + "}";
				case BooleanComparer.LessThanOrEqual:
					pars.Add(Waarde);
					return KolomNaam + "<=" + "{" + (pars.Count - 1) + "}";
				case BooleanComparer.Equal:
					pars.Add(Waarde);
					return KolomNaam + "=" + "{" + (pars.Count - 1) + "}";
				case BooleanComparer.GreaterThanOrEqual:
					pars.Add(Waarde);
					return KolomNaam + ">=" + "{" + (pars.Count - 1) + "}";
				case BooleanComparer.GreaterThan:
					pars.Add(Waarde);
					return KolomNaam + ">" + "{" + (pars.Count - 1) + "}";
				case BooleanComparer.NotEqual:
					pars.Add(Waarde);
					return KolomNaam + "!=" + "{" + (pars.Count - 1) + "}";
				case BooleanComparer.In:
					string[] nrs = Waarde.ToString().Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					string clause = KolomNaam + " in (" + Enumerable.Range(pars.Count, nrs.Length).Select(n => "{" + n + "}").JoinStrings(", ") + ")";
					pars.AddRange(nrs);
					return clause;
				case BooleanComparer.StartsWith:
					pars.Add(Waarde + "%");
					return KolomNaam + " like " + "{" + (pars.Count - 1) + "}";
				case BooleanComparer.Contains:
					pars.Add("%" + Waarde + "%");
					return KolomNaam + " like " + "{" + (pars.Count - 1) + "}";
				case BooleanComparer.IsNull:
					return KolomNaam + " is null";
				case BooleanComparer.IsNotNull:
					return KolomNaam + " is not null";
				default:
					throw new Exception("Geen geldige operator");
			}
		}

		public override string ToString()
		{
			List<object> pars = new List<object>();
			string s = ToString(pars);
			return string.Format(s, pars.ToArray());
		}
	}
}
