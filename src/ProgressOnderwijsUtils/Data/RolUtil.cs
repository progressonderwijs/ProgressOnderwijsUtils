using System;
using System.Collections.Generic;
using System.Linq;
using ProgressOnderwijsUtils.ToegangsRolInternal;

namespace ProgressOnderwijsUtils
{
	public static class RolUtil
	{
		class Comparer : IComparer<Rol>
		{
			public int Compare(Rol x, Rol y)
			{
				return x == y ? 0 :
					x.OnderliggendeToegangsRollen().Contains(y) ? -1 :
					y.OnderliggendeToegangsRollen().Contains(x) ? 1 :
					x < y ? -1 : 1;
			}
		}
		public static readonly IComparer<Rol> StructuralOrdering = new Comparer();


		struct RolDescendants
		{
			public readonly Rol Rol;
			public readonly Rol[] Descendants;
			public RolDescendants(Rol rol, Rol[] descendants)
			{
				Rol = rol;
				Descendants = descendants;
			}
		}


		public static readonly ILookup<Rol, Rol> ChildrenOf;
		public static readonly ILookup<Rol, Rol> ParentsOf;

		static RolUtil()
		{
			//var koppelUp =
			//	from kind in EnumHelpers.GetValues<ToegangsRol>()
			//	from ouderAttr in EnumHelpers.GetAttrs<ImpliedByAttribute>.On(kind)
			//	from ouder in ouderAttr.Ouders
			//	orderby ouder, kind//not strictly necessary
			//	select new { ouder, kind };

			var koppelDown =
				from ouder in EnumHelpers.GetValues<Rol>()
				from kindAttr in EnumHelpers.GetAttrs<ImpliesAttribute>.On(ouder)
				from kind in kindAttr.Kinderen
				orderby ouder, kind//not strictly necessary
				select new { ouder, kind };

			//var autoKoppelDown = EnumHelpers.GetValues<Rol>().Where(rol => rol != Rol.Iedereen).Select(rol => new { ouder = rol, kind = Rol.Iedereen });

			var koppelingen = koppelDown
				//.Concat(autoKoppelDown)
				//.Concat(koppelUp)
						.Distinct().OrderBy(r => r.kind);

			ChildrenOf = koppelingen.ToLookup(rel => rel.ouder, rel => rel.kind);
			ParentsOf = koppelingen.ToLookup(rel => rel.kind, rel => rel.ouder);
		}

		public static bool IsToekenbaar(this Rol rol)
		{
			return EnumHelpers.GetAttrs<ToekenbaarAttribute>.On(rol).Any();
		}

		public static HashSet<Rol> OnderliggendeToegangsRollen(this Rol root)
		{
			return Utils.TransitiveClosure(new[] { root }, rol => ChildrenOf[rol]);
		}

		public static HashSet<Rol> BovenliggendeToegangsRollen(this Rol root)
		{
			return Utils.TransitiveClosure(new[] { root }, rol => ParentsOf[rol]);
		}

		public static HashSet<Rol> OnderliggendeToegangsRollen(this IEnumerable<Rol> roots)
		{
			return Utils.TransitiveClosure(roots, rol => ChildrenOf[rol]);
		}
	}

	namespace ToegangsRolInternal
	{
		public sealed class ToekenbaarAttribute : Attribute { }

		//internal sealed class ImpliedByAttribute : Attribute
		//{
		//	public readonly IReadOnlyList<ToegangsRol> Ouders;
		//	public ImpliedByAttribute(params ToegangsRol[] ouders)
		//	{
		//		Ouders = ouders;
		//	}
		//}

		public sealed class ImpliesAttribute : Attribute
		{
			public readonly IReadOnlyList<Rol> Kinderen;
			public ImpliesAttribute(params Rol[] kinderen)
			{
				Kinderen = kinderen;
			}
		}
	}
}
