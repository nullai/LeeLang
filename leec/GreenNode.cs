using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class GreenNode
	{
		private string GetDebuggerDisplay()
		{
			return this.GetType().Name + " " + this.KindText + " " + this.ToString();
		}

		public const int ListKind = 1;

		private readonly ushort _kind;
		protected NodeFlags flags;
		private byte _slotCount;
		private int _fullWidth;

		private static readonly Dictionary<GreenNode, DiagnosticInfo[]> s_diagnosticsTable = new Dictionary<GreenNode, DiagnosticInfo[]>();

		private static readonly Dictionary<GreenNode, SyntaxAnnotation[]> s_annotationsTable = new Dictionary<GreenNode, SyntaxAnnotation[]>();

		private static readonly DiagnosticInfo[] s_noDiagnostics = new DiagnosticInfo[0];
		private static readonly SyntaxAnnotation[] s_noAnnotations = new SyntaxAnnotation[0];
		private static readonly List<SyntaxAnnotation> s_noAnnotationsEnumerable = new List<SyntaxAnnotation>();

		public int Position => 0;

		public int Index => 0;

		public LeeSyntaxNode Parent => null;

		public Location Location => default(Location);
		public SyntaxTree SyntaxTree => null;

		public TextSpan Span
		{
			get
			{
				// Start with the full span.
				var start = Position;
				var width = this.FullWidth;

				// adjust for preceding trivia (avoid calling this twice, do not call Green.Width)
				var precedingWidth = this.GetLeadingTriviaWidth();
				start += precedingWidth;
				width -= precedingWidth;

				// adjust for following trivia width
				width -= this.GetTrailingTriviaWidth();

				Debug.Assert(width >= 0);
				return new TextSpan(start, width);
			}
		}

		public int SpanStart => Position + GetLeadingTriviaWidth();

		public TextSpan FullSpan => new TextSpan(this.Position, this.FullWidth);

		public SyntaxTriviaList LeadingTrivia
		{
			get
			{
				throw new Exception();
				//return new SyntaxTriviaList(this, GetLeadingTriviaCore(), this.Position);
			}
		}

		public SyntaxTriviaList TrailingTrivia
		{
			get
			{
				var leading = GetLeadingTriviaCore();
				int index = 0;
				if (leading != null)
				{
					index = leading.IsList ? leading.SlotCount : 1;
				}

				var trailingGreen = GetTrailingTriviaCore();
				int trailingPosition = Position + this.FullWidth;
				if (trailingGreen != null)
				{
					trailingPosition -= trailingGreen.FullWidth;
				}

				throw new Exception();
				//return new SyntaxTriviaList(this, trailingGreen, trailingPosition, index);
			}
		}

		protected GreenNode(ushort kind)
		{
			_kind = kind;
		}

		protected GreenNode(ushort kind, int fullWidth)
		{
			_kind = kind;
			_fullWidth = fullWidth;
		}

		protected GreenNode(ushort kind, DiagnosticInfo[] diagnostics, int fullWidth)
		{
			_kind = kind;
			_fullWidth = fullWidth;
			if (diagnostics?.Length > 0)
			{
				this.flags |= NodeFlags.ContainsDiagnostics;
				s_diagnosticsTable.Add(this, diagnostics);
			}
		}

		protected GreenNode(ushort kind, DiagnosticInfo[] diagnostics)
		{
			_kind = kind;
			if (diagnostics?.Length > 0)
			{
				this.flags |= NodeFlags.ContainsDiagnostics;
				s_diagnosticsTable.Add(this, diagnostics);
			}
		}

		protected GreenNode(ushort kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations) :
			this(kind, diagnostics)
		{
			if (annotations?.Length > 0)
			{
				foreach (var annotation in annotations)
				{
					if (annotation == null) throw new ArgumentException(paramName: nameof(annotations), message: "" /*CSharpResources.ElementsCannotBeNull*/);
				}

				this.flags |= NodeFlags.ContainsAnnotations;
				s_annotationsTable.Add(this, annotations);
			}
		}

		protected GreenNode(ushort kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, int fullWidth) :
			this(kind, diagnostics, fullWidth)
		{
			if (annotations?.Length > 0)
			{
				foreach (var annotation in annotations)
				{
					if (annotation == null) throw new ArgumentException(paramName: nameof(annotations), message: "" /*CSharpResources.ElementsCannotBeNull*/);
				}

				this.flags |= NodeFlags.ContainsAnnotations;
				s_annotationsTable.Add(this, annotations);
			}
		}

		protected void AdjustFlagsAndWidth(GreenNode node)
		{
			Debug.Assert(node != null, "PERF: caller must ensure that node!=null, we do not want to re-check that here.");
			this.flags |= (node.flags & NodeFlags.InheritMask);
			_fullWidth += node._fullWidth;
		}

		public abstract string Language { get; }

		#region Kind 
		public int RawKind
		{
			get { return _kind; }
		}

		public SyntaxKind Kind => (SyntaxKind)RawKind;
		public virtual SyntaxKind ContextualKind => (SyntaxKind)RawContextualKind;

		public bool IsList
		{
			get
			{
				return RawKind == ListKind;
			}
		}

		public bool IsKind(SyntaxKind kind)
		{
			return (SyntaxKind)RawKind == kind;
		}

		public abstract string KindText { get; }

		public virtual bool IsStructuredTrivia => false;
		public virtual bool IsDirective => false;
		public virtual bool IsToken => false;
		public virtual bool IsTrivia => false;
		public virtual bool IsSkippedTokensTrivia => false;
		public virtual bool IsDocumentationCommentTrivia => false;

		#endregion

		#region Slots 
		public int SlotCount
		{
			get
			{
				int count = _slotCount;
				if (count == byte.MaxValue)
				{
					count = GetSlotCount();
				}

				return count;
			}

			protected set
			{
				_slotCount = (byte)value;
			}
		}

		public abstract GreenNode GetSlot(int index);

		// for slot counts >= byte.MaxValue
		protected virtual int GetSlotCount()
		{
			return _slotCount;
		}

		public virtual int GetSlotOffset(int index)
		{
			int offset = 0;
			for (int i = 0; i < index; i++)
			{
				var child = this.GetSlot(i);
				if (child != null)
				{
					offset += child.FullWidth;
				}
			}

			return offset;
		}

		public ChildSyntaxList ChildNodesAndTokens()
		{
			return new ChildSyntaxList(this);
		}

		/// <summary>
		/// Enumerates all nodes of the tree rooted by this node (including this node).
		/// </summary>
		public IEnumerable<GreenNode> EnumerateNodes()
		{
			yield return this;

			var stack = new Stack<ChildSyntaxList.Enumerator>(24);
			stack.Push(this.ChildNodesAndTokens().GetEnumerator());

			while (stack.Count > 0)
			{
				var en = stack.Pop();
				if (!en.MoveNext())
				{
					// no more down this branch
					continue;
				}

				var current = en.Current;
				stack.Push(en); // put it back on stack (struct enumerator)

				yield return current;

				if (!current.IsToken)
				{
					// not token, so consider children
					stack.Push(current.ChildNodesAndTokens().GetEnumerator());
					continue;
				}
			}
		}

		/// <summary>
		/// Find the slot that contains the given offset.
		/// </summary>
		/// <param name="offset">The target offset. Must be between 0 and <see cref="FullWidth"/>.</param>
		/// <returns>The slot index of the slot containing the given offset.</returns>
		/// <remarks>
		/// The base implementation is a linear search. This should be overridden
		/// if a derived class can implement it more efficiently.
		/// </remarks>
		public virtual int FindSlotIndexContainingOffset(int offset)
		{
			Debug.Assert(0 <= offset && offset < FullWidth);

			int i;
			int accumulatedWidth = 0;
			for (i = 0; ; i++)
			{
				Debug.Assert(i < SlotCount);
				var child = GetSlot(i);
				if (child != null)
				{
					accumulatedWidth += child.FullWidth;
					if (offset < accumulatedWidth)
					{
						break;
					}
				}
			}

			return i;
		}

		#endregion

		#region Flags 
		[Flags]
		public enum NodeFlags : byte
		{
			None = 0,
			ContainsDiagnostics = 1 << 0,
			ContainsStructuredTrivia = 1 << 1,
			ContainsDirectives = 1 << 2,
			ContainsSkippedText = 1 << 3,
			ContainsAnnotations = 1 << 4,
			IsNotMissing = 1 << 5,

			FactoryContextIsInAsync = 1 << 6,
			FactoryContextIsInQuery = 1 << 7,
			FactoryContextIsInIterator = FactoryContextIsInQuery,  // VB does not use "InQuery", but uses "InIterator" instead

			InheritMask = ContainsDiagnostics | ContainsStructuredTrivia | ContainsDirectives | ContainsSkippedText | ContainsAnnotations | IsNotMissing,
		}

		public NodeFlags Flags
		{
			get { return this.flags; }
		}

		public void SetFlags(NodeFlags flags)
		{
			this.flags |= flags;
		}

		public void ClearFlags(NodeFlags flags)
		{
			this.flags &= ~flags;
		}

		public bool IsMissing
		{
			get
			{
				// flag has reversed meaning hence "=="
				return (this.flags & NodeFlags.IsNotMissing) == 0;
			}
		}

		public bool ParsedInAsync
		{
			get
			{
				return (this.flags & NodeFlags.FactoryContextIsInAsync) != 0;
			}
		}

		public bool ParsedInQuery
		{
			get
			{
				return (this.flags & NodeFlags.FactoryContextIsInQuery) != 0;
			}
		}

		public bool ParsedInIterator
		{
			get
			{
				return (this.flags & NodeFlags.FactoryContextIsInIterator) != 0;
			}
		}

		public bool ContainsSkippedText
		{
			get
			{
				return (this.flags & NodeFlags.ContainsSkippedText) != 0;
			}
		}

		public bool ContainsStructuredTrivia
		{
			get
			{
				return (this.flags & NodeFlags.ContainsStructuredTrivia) != 0;
			}
		}

		public bool ContainsDirectives
		{
			get
			{
				return (this.flags & NodeFlags.ContainsDirectives) != 0;
			}
		}

		public bool ContainsDiagnostics
		{
			get
			{
				return (this.flags & NodeFlags.ContainsDiagnostics) != 0;
			}
		}

		public bool ContainsAnnotations
		{
			get
			{
				return (this.flags & NodeFlags.ContainsAnnotations) != 0;
			}
		}
		#endregion

		#region Spans
		public int FullWidth
		{
			get
			{
				return _fullWidth;
			}

			protected set
			{
				_fullWidth = value;
			}
		}

		public virtual int Width
		{
			get
			{
				return _fullWidth - this.GetLeadingTriviaWidth() - this.GetTrailingTriviaWidth();
			}
		}

		public virtual int GetLeadingTriviaWidth()
		{
			return this.FullWidth != 0 ?
				this.GetFirstTerminal().GetLeadingTriviaWidth() :
				0;
		}

		public virtual int GetTrailingTriviaWidth()
		{
			return this.FullWidth != 0 ?
				this.GetLastTerminal().GetTrailingTriviaWidth() :
				0;
		}

		public bool HasLeadingTrivia
		{
			get
			{
				return this.GetLeadingTriviaWidth() != 0;
			}
		}

		public bool HasTrailingTrivia
		{
			get
			{
				return this.GetTrailingTriviaWidth() != 0;
			}
		}
		#endregion

		#region Annotations 
		public bool HasAnnotations(string annotationKind)
		{
			var annotations = this.GetAnnotations();
			if (annotations == s_noAnnotations)
			{
				return false;
			}

			foreach (var a in annotations)
			{
				if (a.Kind == annotationKind)
				{
					return true;
				}
			}

			return false;
		}

		public bool HasAnnotations(IEnumerable<string> annotationKinds)
		{
			var annotations = this.GetAnnotations();
			if (annotations == s_noAnnotations)
			{
				return false;
			}

			foreach (var a in annotations)
			{
				if (annotationKinds.Contains(a.Kind))
				{
					return true;
				}
			}

			return false;
		}

		public bool HasAnnotation(SyntaxAnnotation annotation)
		{
			var annotations = this.GetAnnotations();
			if (annotations == s_noAnnotations)
			{
				return false;
			}

			foreach (var a in annotations)
			{
				if (a == annotation)
				{
					return true;
				}
			}

			return false;
		}

		public IEnumerable<SyntaxAnnotation> GetAnnotations(string annotationKind)
		{
			if (string.IsNullOrWhiteSpace(annotationKind))
			{
				throw new ArgumentNullException(nameof(annotationKind));
			}

			var annotations = this.GetAnnotations();

			if (annotations == s_noAnnotations)
			{
				return s_noAnnotationsEnumerable;
			}

			return GetAnnotationsSlow(annotations, annotationKind);
		}

		private static IEnumerable<SyntaxAnnotation> GetAnnotationsSlow(SyntaxAnnotation[] annotations, string annotationKind)
		{
			foreach (var annotation in annotations)
			{
				if (annotation.Kind == annotationKind)
				{
					yield return annotation;
				}
			}
		}

		public IEnumerable<SyntaxAnnotation> GetAnnotations(IEnumerable<string> annotationKinds)
		{
			if (annotationKinds == null)
			{
				throw new ArgumentNullException(nameof(annotationKinds));
			}

			var annotations = this.GetAnnotations();

			if (annotations == s_noAnnotations)
			{
				return s_noAnnotationsEnumerable;
			}

			return GetAnnotationsSlow(annotations, annotationKinds);
		}

		private static IEnumerable<SyntaxAnnotation> GetAnnotationsSlow(SyntaxAnnotation[] annotations, IEnumerable<string> annotationKinds)
		{
			foreach (var annotation in annotations)
			{
				if (annotationKinds.Contains(annotation.Kind))
				{
					yield return annotation;
				}
			}
		}

		public SyntaxAnnotation[] GetAnnotations()
		{
			if (this.ContainsAnnotations)
			{
				SyntaxAnnotation[] annotations;
				if (s_annotationsTable.TryGetValue(this, out annotations))
				{
					System.Diagnostics.Debug.Assert(annotations.Length != 0, "we should return nonempty annotations or NoAnnotations");
					return annotations;
				}
			}

			return s_noAnnotations;
		}

		public abstract GreenNode SetAnnotations(SyntaxAnnotation[] annotations);

		#endregion

		#region Diagnostics
		public DiagnosticInfo[] GetDiagnostics()
		{
			if (this.ContainsDiagnostics)
			{
				DiagnosticInfo[] diags;
				if (s_diagnosticsTable.TryGetValue(this, out diags))
				{
					return diags;
				}
			}

			return s_noDiagnostics;
		}

		public abstract GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics);
		#endregion

		#region Text

		public virtual string ToFullString()
		{
			return ToString();
		}

		private static int GetFirstNonNullChildIndex(GreenNode node)
		{
			int n = node.SlotCount;
			int firstIndex = 0;
			for (; firstIndex < n; firstIndex++)
			{
				var child = node.GetSlot(firstIndex);
				if (child != null)
				{
					break;
				}
			}

			return firstIndex;
		}

		private static int GetLastNonNullChildIndex(GreenNode node)
		{
			int n = node.SlotCount;
			int lastIndex = n - 1;
			for (; lastIndex >= 0; lastIndex--)
			{
				var child = node.GetSlot(lastIndex);
				if (child != null)
				{
					break;
				}
			}

			return lastIndex;
		}

		#endregion

		#region Tokens 

		public virtual int RawContextualKind { get { return this.RawKind; } }
		public virtual object GetValue() { return null; }
		public virtual string GetValueText() { return string.Empty; }
		public virtual GreenNode GetLeadingTriviaCore() { return null; }
		public virtual GreenNode GetTrailingTriviaCore() { return null; }

		public virtual GreenNode WithLeadingTrivia(GreenNode trivia)
		{
			return this;
		}

		public virtual GreenNode WithTrailingTrivia(GreenNode trivia)
		{
			return this;
		}

		public GreenNode GetFirstTerminal()
		{
			GreenNode node = this;

			do
			{
				GreenNode firstChild = null;
				for (int i = 0, n = node.SlotCount; i < n; i++)
				{
					var child = node.GetSlot(i);
					if (child != null)
					{
						firstChild = child;
						break;
					}
				}
				node = firstChild;
			} while (node?._slotCount > 0);

			return node;
		}

		public GreenNode GetLastTerminal()
		{
			GreenNode node = this;

			do
			{
				GreenNode lastChild = null;
				for (int i = node.SlotCount - 1; i >= 0; i--)
				{
					var child = node.GetSlot(i);
					if (child != null)
					{
						lastChild = child;
						break;
					}
				}
				node = lastChild;
			} while (node?._slotCount > 0);

			return node;
		}

		public GreenNode GetLastNonmissingTerminal()
		{
			GreenNode node = this;

			do
			{
				GreenNode nonmissingChild = null;
				for (int i = node.SlotCount - 1; i >= 0; i--)
				{
					var child = node.GetSlot(i);
					if (child != null && !child.IsMissing)
					{
						nonmissingChild = child;
						break;
					}
				}
				node = nonmissingChild;
			}
			while (node?._slotCount > 0);

			return node;
		}
		#endregion

		#region Equivalence 
		public virtual bool IsEquivalentTo(GreenNode other)
		{
			if (this == other)
			{
				return true;
			}

			if (other == null)
			{
				return false;
			}

			return EquivalentToInternal(this, other);
		}

		private static bool EquivalentToInternal(GreenNode node1, GreenNode node2)
		{
			if (node1.RawKind != node2.RawKind)
			{
				// A single-element list is usually represented as just a single node,
				// but can be represented as a List node with one child. Move to that
				// child if necessary.
				if (node1.IsList && node1.SlotCount == 1)
				{
					node1 = node1.GetSlot(0);
				}

				if (node2.IsList && node2.SlotCount == 1)
				{
					node2 = node2.GetSlot(0);
				}

				if (node1.RawKind != node2.RawKind)
				{
					return false;
				}
			}

			if (node1._fullWidth != node2._fullWidth)
			{
				return false;
			}

			var n = node1.SlotCount;
			if (n != node2.SlotCount)
			{
				return false;
			}

			for (int i = 0; i < n; i++)
			{
				var node1Child = node1.GetSlot(i);
				var node2Child = node2.GetSlot(i);
				if (node1Child != null && node2Child != null && !node1Child.IsEquivalentTo(node2Child))
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region Factories 

		public abstract bool IsTriviaWithEndOfLine(); // trivia node has end of line

		public virtual GreenNode CreateList(IEnumerable<GreenNode> nodes, bool alwaysCreateListNode = false)
		{
			if (nodes == null)
			{
				return null;
			}

			var list = nodes.ToArray();

			switch (list.Length)
			{
				case 0:
					return null;
				case 1:
					if (alwaysCreateListNode)
					{
						goto default;
					}
					else
					{
						return list[0];
					}
				case 2:
					return SyntaxList.List(list[0], list[1]);
				case 3:
					return SyntaxList.List(list[0], list[1], list[2]);
				default:
					return SyntaxList.List(list);
			}
		}

		#endregion


		/// <summary>
		/// Add an error to the given node, creating a new node that is the same except it has no parent,
		/// and has the given error attached to it. The error span is the entire span of this node.
		/// </summary>
		/// <param name="err">The error to attach to this node</param>
		/// <returns>A new node, with no parent, that has this error added to it.</returns>
		/// <remarks>Since nodes are immutable, the only way to create nodes with errors attached is to create a node without an error,
		/// then add an error with this method to create another node.</remarks>
		public GreenNode AddError(DiagnosticInfo err)
		{
			DiagnosticInfo[] errorInfos;

			// If the green node already has errors, add those on.
			if (GetDiagnostics() == null)
			{
				errorInfos = new[] { err };
			}
			else
			{
				// Add the error to the error list.
				errorInfos = GetDiagnostics();
				var length = errorInfos.Length;
				Array.Resize(ref errorInfos, length + 1);
				errorInfos[length] = err;
			}

			// Get a new green node with the errors added on.
			return SetDiagnostics(errorInfos);
		}

		public SyntaxToken CreateSeparator()
		{
			return SyntaxFactory.Token(SyntaxKind.CommaToken);
		}
	}
}
