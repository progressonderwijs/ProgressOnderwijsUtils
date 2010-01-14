using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public class Criterium
	{
		string kolomnaam;
		BooleanComparer comparer;
		object waarde;

		public string KolomNaam { get { return kolomnaam; } set { kolomnaam = value; } }
		public BooleanComparer Comparer { get { return comparer; } set { comparer = value; } }
		public object Waarde { get { return waarde; } set { waarde = value; } }

		public static BooleanComparer[] StringComparers { get { return new BooleanComparer[] { BooleanComparer.Contains, BooleanComparer.Equal, BooleanComparer.NotEqual, BooleanComparer.StartsWith, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }
		public static BooleanComparer[] NumericComparers { get { return new BooleanComparer[] { BooleanComparer.Equal, BooleanComparer.GreaterThan, BooleanComparer.GreaterThanOrEqual, BooleanComparer.LessThan, BooleanComparer.LessThanOrEqual, BooleanComparer.NotEqual, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }

		public Criterium(string kolomnaam, BooleanComparer comparer, object waarde)
		{
			this.kolomnaam = kolomnaam;
			this.comparer = comparer;
			this.waarde = waarde;
		}
		public string ToString(List<object> pars)
		{
			switch (comparer)
			{
				case BooleanComparer.LessThan:
					pars.Add(waarde);
					return kolomnaam + "<" + "{" + (pars.Count - 1).ToString() + "}";
				case BooleanComparer.LessThanOrEqual:
					pars.Add(waarde);
					return kolomnaam + "<=" + "{" + (pars.Count - 1).ToString() + "}";
				case BooleanComparer.Equal:
					pars.Add(waarde);
					return kolomnaam + "=" + "{" + (pars.Count - 1).ToString() + "}";
				case BooleanComparer.GreaterThanOrEqual:
					pars.Add(waarde);
					return kolomnaam + ">=" + "{" + (pars.Count - 1).ToString() + "}";
				case BooleanComparer.GreaterThan:
					pars.Add(waarde);
					return kolomnaam + ">" + "{" + (pars.Count - 1).ToString() + "}";
				case BooleanComparer.NotEqual:
					pars.Add(waarde);
					return kolomnaam + "!=" + "{" + (pars.Count - 1).ToString() + "}";
				case BooleanComparer.In:
					string[] nrs = waarde.ToString().Split(new string[] { " ", "\r\n", "\n", "," }, StringSplitOptions.RemoveEmptyEntries);
					string n = kolomnaam + " in (";
					for (int i = 0; i < nrs.Length; i++)
					{
						pars.Add(nrs[i]);
						n += "{" + (pars.Count - 1).ToString() + "}, ";
					}
					return n.Substring(0, n.Length-2) + ")";
				case BooleanComparer.StartsWith:
					pars.Add(waarde.ToString() + "%");
					return kolomnaam + " like " + "{" + (pars.Count - 1).ToString() + "}";
				case BooleanComparer.Contains:
					pars.Add("%" + waarde.ToString() + "%");
					return kolomnaam + " like " + "{" + (pars.Count - 1).ToString() + "}";
				case BooleanComparer.IsNull:
					return kolomnaam + " is null";
				case BooleanComparer.IsNotNull:
					return kolomnaam + " is not null";
				default:
					throw new Exception("Geen geldige operator");
			}
		}
		public override string ToString()
		{
			List<object> pars = new List<object>();
			string s = ToString(pars);
			object[] parsarray = new object[pars.Count];
			for (int i = 0; i < pars.Count; ++i)
				parsarray[i] = pars[i];
			return string.Format(s, parsarray);
		}
	}
}
