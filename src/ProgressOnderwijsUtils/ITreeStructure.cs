﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public interface ITreeStructure
	{
		string Naam { get; }
		int NodeId { get; }
		bool Selectable { get; }
		IEnumerable<ITreeStructure> Children { get; }
	}

#if false
	public class MinimalTreeStructure : ITreeStructure
	{
		readonly string naam;
		readonly int nodeid;
		readonly bool selectable;
		readonly MinimalTreeStructure[] children;
		public MinimalTreeStructure(ITreeStructure source)
		{
			naam = source.Naam;
			nodeid = source.NodeId;
			selectable = source.Selectable;
			children = source.Children.Select(child => new MinimalTreeStructure(child)).ToArray();
		}

		public string Naam { get { return naam; } }
		public int NodeId { get { return nodeid; } }
		public bool Selectable { get { return selectable; } }
		public IEnumerable<ITreeStructure> Children { get { return children; } }
	}
#endif

	public static class TreeStructureExtensions
	{
		public static IEnumerable<ITreeStructure> DescendantsAndSelf(this ITreeStructure tree) { return Enumerable.Repeat(tree, 1).Concat(tree.Children.SelectMany(child => child.DescendantsAndSelf())); }
	}
}

