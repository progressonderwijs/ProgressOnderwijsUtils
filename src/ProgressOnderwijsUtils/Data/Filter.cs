using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace ProgressOnderwijsUtils
{
	public class Filter
	{
		BooleanOperator andor;
		List<Filter> filterLijst;
		Criterium criterium;
		public ReadOnlyCollection<Filter> FilterLijst { get { return new ReadOnlyCollection<Filter>(filterLijst); } }
		public void AddFilter(Filter newFilter) { filterLijst.Add(newFilter); }
		public BooleanOperator AndOr { get { return andor; } }
		public Criterium Criterium { get { return criterium; } set { criterium = value; } }
		public Filter Parent { get; private set; }
		public int Position { get; private set; }

		public Filter(Criterium criterium) { this.criterium = criterium; }
		public Filter(BooleanOperator andor, params Filter[] condities) : this(andor, condities.AsEnumerable()) { }
		public Filter(BooleanOperator andor, IEnumerable<Filter> condities)
		{
			this.andor = andor;
			filterLijst = new List<Filter>();
			foreach (Filter t in condities)
				AddHelper(t);
		}
		void AddHelper(Filter f)
		{
			f.Parent = this;
			f.Position = filterLijst.Count;
			filterLijst.Add(f);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pos"></param>
		/// <returns>true als er een leeg filter overblijft, false indien er een gevuld filter overblijft</returns>
		public bool RemoveFilterElement(int pos)
		{
			if (criterium != null)	//Filter bestaat uitsluitend uit criterium, blijft dus leeg filter over
				return true;
			else
			{
				if (filterLijst.Count > 2)		//Filter bestaat uit 3 of meer subfilters, verwijder de juiste
				{
					filterLijst.RemoveAt(pos);
					return false;
				}
				if (filterLijst.Count == 2)	//Filter bestaat uit 2 subfilters, juiste verwijderen en unboxen
				{
					filterLijst.RemoveAt(pos);
					Filter f = filterLijst[0];
					criterium = f.criterium;
					andor = f.andor;
					filterLijst = f.filterLijst;
					if (filterLijst != null)
						foreach (Filter fl in f.filterLijst)
							fl.Parent = this;
					return false;
				}
				return true;		//Zou niet voor moeten kunnen komen, maar retourneer dan maar een leeg filter
			}
		}

		public string ToString(List<object> parameterSink)
		{
			if (criterium == null)
				return "(" + filterLijst.Select(f => f.ToString(parameterSink)).JoinStrings(" " + andor + " ") + ")";
			else
				return criterium.ToString(parameterSink);
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
