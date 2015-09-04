using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.ToegangsRolInternal;

namespace ProgressOnderwijsUtils
{
    public static class RolUtil
    {
        class RolRelations
        {
            public Rol Rol;
            public bool IsToekenbaar;
            public Rol[] Children, Parents, Descendants, Ancestors;
        }

        [Pure]
        public static IReadOnlyList<Rol> Parents(this Rol rol) => rollenRelations[(int)rol].Parents;
        [Pure]
        public static IReadOnlyList<Rol> Children(this Rol rol) => rollenRelations[(int)rol].Children;
        static readonly Dictionary<int, RolRelations> rollenRelations = new Dictionary<int, RolRelations>();

        static RolUtil()
        {
            foreach (var rol in EnumHelpers.GetValues<Rol>()) {
                rollenRelations.Add(
                    (int)rol,
                    new RolRelations {
                        Rol = rol,
                        Children = EnumHelpers.GetAttrs<ImpliesAttribute>.On(rol).SelectMany(a => a.Kinderen).OrderBy(r => r).ToArray()
                    });
            }
            var q =
                from rel in rollenRelations.Values
                let parent = rel.Rol
                from kid in rel.Children
                select new { parent, kid };
            var byKid = q.ToLookup(o => o.kid, o => o.parent);

            foreach (var kv in rollenRelations) {
                var relation = kv.Value;
                relation.Parents = byKid[relation.Rol].OrderBy(r => r).ToArray();
                relation.IsToekenbaar = EnumHelpers.GetAttrs<ToekenbaarAttribute>.On(relation.Rol).Any();
            }
        }

        [Pure]
        public static bool IsToekenbaar(this Rol rol) => rollenRelations[(int)rol].IsToekenbaar;
        [Pure]
        public static RollenSet OnderliggendeToegangsRollen(this Rol root) => new RollenSet(OnderliggendeToegangsRollenImpl(root));

        static Rol[] OnderliggendeToegangsRollenImpl(Rol root)
        {
            var rel = rollenRelations[(int)root];
            if (rel.Descendants == null) {
                var reachable = Utils.TransitiveClosure(new[] { root }, Children).OrderBy(r => r).ToArray();
                Interlocked.CompareExchange(ref rel.Descendants, reachable, null);
            }
            return rel.Descendants;
        }

        [Pure]
        public static RollenSet BovenliggendeToegangsRollen(this Rol root)
        {
            var rel = rollenRelations[(int)root];
            if (rel.Ancestors == null) {
                var reachable = Utils.TransitiveClosure(new[] { root }, Parents).OrderBy(r => r).ToArray();
                Interlocked.CompareExchange(ref rel.Ancestors, reachable, null);
            }
            return new RollenSet(rel.Ancestors);
        }

        struct Cursor
        {
            public int Pos; //, Value;
            public Rol[] Arr;

            public Cursor(int pos, Rol[] arr)
            {
                Pos = pos;
                Arr = arr;
            }

            public bool Valid() => Pos < Arr.Length;
            public void MoveNext() { Pos++; }
            public int Value => (int)Arr[Pos];
        }

        static readonly int TotalRolCount = EnumHelpers.GetValues<Rol>().Count;
        static readonly ThreadLocal<Rol[]> RolAccumulator = new ThreadLocal<Rol[]>(() => new Rol[TotalRolCount]);

        [Pure]
        public static RollenSet OnderliggendeToegangsRollen(this IEnumerable<Rol> roots)
        {
            var heap = new Cursor[4];
            var len = 0;

            foreach (var rol in roots) {
                if (len == heap.Length) {
                    Array.Resize(ref heap, len * 2);
                }
                heap[len++] = new Cursor(0, OnderliggendeToegangsRollenImpl(rol));
            }
            var rollen = RolAccumulator.Value;
            var rolCount = 0;
            while (len > 0) {
                int minIdx = 0, minValue = heap[0].Value;
                for (int i = 1; i < len; i++) {
                    var value = heap[i].Value;
                    if (minValue > value) {
                        minIdx = i;
                        minValue = value;
                    }
                }
                rollen[rolCount++] = (Rol)minValue;
                int writeIdx = minIdx;
                heap[minIdx].MoveNext();
                if (heap[minIdx].Valid()) {
                    writeIdx++;
                }

                for (minIdx++; minIdx < len; minIdx++) {
                    if (heap[minIdx].Value == minValue) {
                        heap[minIdx].MoveNext();
                        if (!heap[minIdx].Valid()) {
                            continue;
                        }
                    }
                    if (minIdx != writeIdx) {
                        heap[writeIdx] = heap[minIdx];
                    }
                    writeIdx++;
                }
                len = writeIdx;
            }

            var output = new Rol[rolCount];
            for (int i = 0; i < output.Length; i++) {
                output[i] = rollen[i];
            }
            return new RollenSet(output);
        }
    }

    public struct RollenSet
    {
        public RollenSet(Rol[] sortedRollen) { this.sortedRollen = sortedRollen; }
        readonly Rol[] sortedRollen;
        public IReadOnlyList<Rol> Rollen => sortedRollen;
        [Pure]
        public bool Contains(Rol rol) => Array.BinarySearch(sortedRollen, rol) >= 0;
    }

    namespace ToegangsRolInternal
    {
        public sealed class ToekenbaarAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public sealed class ImpliesAttribute : Attribute
        {
            public readonly IReadOnlyList<Rol> Kinderen;
            public ImpliesAttribute(params Rol[] kinderen) { Kinderen = kinderen; }
        }
    }
}
