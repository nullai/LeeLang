using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class LineDirectiveMap<TDirective>
		where TDirective : GreenNode
	{
		protected readonly LineMappingEntry[] Entries;

		// Get all active #line directives under trivia into the list, in source code order.
		protected abstract bool ShouldAddDirective(TDirective directive);

		// Given a directive and the previous entry, create a new entry.
		protected abstract LineMappingEntry GetEntry(TDirective directive, SourceText sourceText, LineMappingEntry previous);

		// Creates the first entry with language specific content
		protected abstract LineMappingEntry InitializeFirstEntry();

		protected LineDirectiveMap(SyntaxTree syntaxTree)
		{
			// Accumulate all the directives, in source code order
			var syntaxRoot = syntaxTree.GetRoot();
			IList<TDirective> directives = null;// syntaxRoot.GetDirectives(filter: ShouldAddDirective);
			Debug.Assert(directives != null);

			// Create the entry map.
			this.Entries = CreateEntryMap(syntaxTree, directives);
		}

		// Given a span and a default file name, return a FileLinePositionSpan that is the mapped
		// span, taking into account line directives.
		public FileLinePositionSpan TranslateSpan(SourceText sourceText, string treeFilePath, TextSpan span)
		{
			throw new Exception();
			//var unmappedStartPos = sourceText.Lines.GetLinePosition(span.Start);
			//var unmappedEndPos = sourceText.Lines.GetLinePosition(span.End);
			//var entry = FindEntry(unmappedStartPos.Line);

			//return TranslateSpan(entry, treeFilePath, unmappedStartPos, unmappedEndPos);
		}

		protected FileLinePositionSpan TranslateSpan(LineMappingEntry entry, string treeFilePath, LinePosition unmappedStartPos, LinePosition unmappedEndPos)
		{
			string path = entry.MappedPathOpt ?? treeFilePath;
			int mappedStartLine = unmappedStartPos.Line - entry.UnmappedLine + entry.MappedLine;
			int mappedEndLine = unmappedEndPos.Line - entry.UnmappedLine + entry.MappedLine;

			return new FileLinePositionSpan(
				path,
				new LinePositionSpan(
					(mappedStartLine == -1) ? new LinePosition(unmappedStartPos.Character) : new LinePosition(mappedStartLine, unmappedStartPos.Character),
					(mappedEndLine == -1) ? new LinePosition(unmappedEndPos.Character) : new LinePosition(mappedEndLine, unmappedEndPos.Character)),
				hasMappedPath: entry.MappedPathOpt != null);
		}

		/// <summary>
		/// Determines whether the position is considered to be hidden from the debugger or not.
		/// </summary>
		public abstract LineVisibility GetLineVisibility(SourceText sourceText, int position);

		/// <summary>
		/// Combines TranslateSpan and IsHiddenPosition to not search the entries twice when emitting sequence points
		/// </summary>
		public abstract FileLinePositionSpan TranslateSpanAndVisibility(SourceText sourceText, string treeFilePath, TextSpan span, out bool isHiddenPosition);

		/// <summary>
		/// Are there any hidden regions in the map?
		/// </summary>
		/// <returns>True if there's at least one hidden region in the map.</returns>
		public bool HasAnyHiddenRegions()
		{
			return this.Entries.Any(e => e.State == PositionState.Hidden);
		}

		// Find the line mapped entry with the largest unmapped line number <= lineNumber.
		protected LineMappingEntry FindEntry(int lineNumber)
		{
			int r = FindEntryIndex(lineNumber);

			return this.Entries[r];
		}

		// Find the index of the line mapped entry with the largest unmapped line number <= lineNumber.
		protected int FindEntryIndex(int lineNumber)
		{
			int r = Array.BinarySearch(this.Entries, new LineMappingEntry(lineNumber));
			return r >= 0 ? r : ((~r) - 1);
		}

		// Given the ordered list of all directives in the file, return the ordered line mapping
		// entry for the file. This always starts with the null mapped that maps line 0 to line 0.
		private LineMappingEntry[] CreateEntryMap(SyntaxTree tree, IList<TDirective> directives)
		{
			var entries = new LineMappingEntry[directives.Count + 1];
			var current = InitializeFirstEntry();
			var index = 0;
			entries[index] = current;

			if (directives.Count > 0)
			{
				var sourceText = tree.GetText();
				foreach (var directive in directives)
				{
					current = GetEntry(directive, sourceText, current);
					++index;
					entries[index] = current;
				}
			}

#if DEBUG
			// Make sure the entries array is correctly sorted. 
			for (int i = 0; i < entries.Length - 1; ++i)
			{
				Debug.Assert(entries[i].CompareTo(entries[i + 1]) < 0);
			}
#endif

			return entries;
		}

		public enum PositionState : byte
		{
			/// <summary>
			/// Used in VB when the position is not hidden, but it's not known yet that there is a (nonempty) #ExternalSource
			/// following.
			/// </summary>
			Unknown,

			/// <summary>
			/// Used in C# for spans outside of #line directives
			/// </summary>
			Unmapped,

			/// <summary>
			/// Used in C# for spans inside of "#line linenumber" directive
			/// </summary>
			Remapped,

			/// <summary>
			/// Used in VB for spans inside of a "#ExternalSource" directive that followed an unknown span
			/// </summary>
			RemappedAfterUnknown,

			/// <summary>
			/// Used in VB for spans inside of a "#ExternalSource" directive that followed a hidden span
			/// </summary>
			RemappedAfterHidden,

			/// <summary>
			/// Used in C# and VB for spans that are inside of #line hidden (C#) or outside of #ExternalSource (VB) 
			/// directives
			/// </summary>
			Hidden
		}

		// Struct that represents an entry in the line mapping table. Entries sort by the unmapped
		// line.
		protected struct LineMappingEntry : IComparable<LineMappingEntry>
		{
			// 0-based line in this tree
			public readonly int UnmappedLine;

			// 0-based line it maps to.
			public readonly int MappedLine;

			// raw value from #line or #ExternalDirective, may be null
			public readonly string MappedPathOpt;

			// the state of this line
			public readonly PositionState State;

			public LineMappingEntry(int unmappedLine)
			{
				this.UnmappedLine = unmappedLine;
				this.MappedLine = unmappedLine;
				this.MappedPathOpt = null;
				this.State = PositionState.Unmapped;
			}

			public LineMappingEntry(
				int unmappedLine,
				int mappedLine,
				string mappedPathOpt,
				PositionState state)
			{
				this.UnmappedLine = unmappedLine;
				this.MappedLine = mappedLine;
				this.MappedPathOpt = mappedPathOpt;
				this.State = state;
			}

			public int CompareTo(LineMappingEntry other)
			{
				return this.UnmappedLine.CompareTo(other.UnmappedLine);
			}
		}
	}
}
