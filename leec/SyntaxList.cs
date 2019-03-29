using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct SyntaxList<TNode> : IEquatable<SyntaxList<TNode>>
		where TNode : GreenNode
	{
		private readonly GreenNode _node;

		public SyntaxList(GreenNode node)
		{
			_node = node;
		}

		public GreenNode Node => _node;

		public int Count
		{
			get
			{
				return _node == null ? 0 : (_node.IsList ? _node.SlotCount : 1);
			}
		}

		public TNode this[int index]
		{
			get
			{
				if (_node == null)
				{
					return null;
				}
				else if (_node.IsList)
				{
					Debug.Assert(index >= 0);
					Debug.Assert(index <= _node.SlotCount);

					return (TNode)_node.GetSlot(index);
				}
				else if (index == 0)
				{
					return (TNode)_node;
				}
				else
				{
					throw ExceptionUtilities.Unreachable;
				}
			}
		}

		public GreenNode ItemUntyped(int index)
		{
			var node = this._node;
			if (node.IsList)
			{
				return node.GetSlot(index);
			}

			Debug.Assert(index == 0);
			return node;
		}

		public bool Any()
		{
			return _node != null;
		}

		public bool Any(int kind)
		{
			foreach (var element in this)
			{
				if (element.RawKind == kind)
				{
					return true;
				}
			}

			return false;
		}

		public TNode[] Nodes
		{
			get
			{
				var arr = new TNode[this.Count];
				for (int i = 0; i < this.Count; i++)
				{
					arr[i] = this[i];
				}
				return arr;
			}
		}

		public TNode Last
		{
			get
			{
				var node = this._node;
				if (node.IsList)
				{
					return (TNode)node.GetSlot(node.SlotCount - 1);
				}

				return (TNode)node;
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public void CopyTo(int offset, GreenNode[] array, int arrayOffset, int count)
		{
			for (int i = 0; i < count; i++)
			{
				array[arrayOffset + i] = this[i + offset];
			}
		}

		public static bool operator ==(SyntaxList<TNode> left, SyntaxList<TNode> right)
		{
			return left._node == right._node;
		}

		public static bool operator !=(SyntaxList<TNode> left, SyntaxList<TNode> right)
		{
			return left._node != right._node;
		}

		public bool Equals(SyntaxList<TNode> other)
		{
			return _node == other._node;
		}

		public override bool Equals(object obj)
		{
			return (obj is SyntaxList<TNode>) && Equals((SyntaxList<TNode>)obj);
		}

		public override int GetHashCode()
		{
			return _node != null ? _node.GetHashCode() : 0;
		}

		public SeparatedSyntaxList<TOther> AsSeparatedList<TOther>() where TOther : GreenNode
		{
			return new SeparatedSyntaxList<TOther>(this);
		}

		public static implicit operator SyntaxList<TNode>(TNode node)
		{
			return new SyntaxList<TNode>(node);
		}

		public static implicit operator SyntaxList<TNode>(SyntaxList<GreenNode> nodes)
		{
			return new SyntaxList<TNode>(nodes._node);
		}

		public static implicit operator SyntaxList<GreenNode>(SyntaxList<TNode> nodes)
		{
			return new SyntaxList<GreenNode>(nodes.Node);
		}

		public struct Enumerator
		{
			private SyntaxList<TNode> _list;
			private int _index;

			public Enumerator(SyntaxList<TNode> list)
			{
				_list = list;
				_index = -1;
			}

			public bool MoveNext()
			{
				var newIndex = _index + 1;
				if (newIndex < _list.Count)
				{
					_index = newIndex;
					return true;
				}

				return false;
			}

			public TNode Current
			{
				get
				{
					return _list[_index];
				}
			}
		}
	}

	public abstract class SyntaxList : GreenNode
	{
		public SyntaxList()
			: base(GreenNode.ListKind)
		{
		}

		public SyntaxList(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(GreenNode.ListKind, diagnostics, annotations)
		{
		}

		public static GreenNode List(GreenNode child)
		{
			return child;
		}

		public static WithTwoChildren List(GreenNode child0, GreenNode child1)
		{
			Debug.Assert(child0 != null);
			Debug.Assert(child1 != null);

			return new WithTwoChildren(child0, child1);
		}

		public static WithThreeChildren List(GreenNode child0, GreenNode child1, GreenNode child2)
		{
			Debug.Assert(child0 != null);
			Debug.Assert(child1 != null);
			Debug.Assert(child2 != null);

			return new WithThreeChildren(child0, child1, child2);
		}

		public static SyntaxList List(GreenNode[] children)
		{
			// "WithLotsOfChildren" list will allocate a separate array to hold
			// precomputed node offsets. It may not be worth it for smallish lists.
			if (children.Length < 10)
			{
				return new WithManyChildren(children);
			}
			else
			{
				return new WithLotsOfChildren(children);
			}
		}

		public abstract void CopyTo(GreenNode[] array, int offset);

		public static GreenNode Concat(GreenNode left, GreenNode right)
		{
			if (left == null)
			{
				return right;
			}

			if (right == null)
			{
				return left;
			}

			var leftList = left as SyntaxList;
			var rightList = right as SyntaxList;
			if (leftList != null)
			{
				if (rightList != null)
				{
					var tmp = new GreenNode[left.SlotCount + right.SlotCount];
					leftList.CopyTo(tmp, 0);
					rightList.CopyTo(tmp, left.SlotCount);
					return List(tmp);
				}
				else
				{
					var tmp = new GreenNode[left.SlotCount + 1];
					leftList.CopyTo(tmp, 0);
					tmp[left.SlotCount] = right;
					return List(tmp);
				}
			}
			else if (rightList != null)
			{
				var tmp = new GreenNode[rightList.SlotCount + 1];
				tmp[0] = left;
				rightList.CopyTo(tmp, 1);
				return List(tmp);
			}
			else
			{
				return List(left, right);
			}
		}

		public sealed override string Language
		{
			get
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		public sealed override string KindText
		{
			get
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		public sealed override bool IsTriviaWithEndOfLine()
		{
			return false;
		}
	}

	public sealed class WithLotsOfChildren : WithManyChildrenBase
	{
		private readonly int[] _childOffsets;

		public WithLotsOfChildren(GreenNode[] children)
			: base(children)
		{
			_childOffsets = CalculateOffsets(children);
		}

		public WithLotsOfChildren(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, GreenNode[] children, int[] childOffsets)
			: base(diagnostics, annotations, children)
		{
			_childOffsets = childOffsets;
		}

		public override int GetSlotOffset(int index)
		{
			return _childOffsets[index];
		}

		/// <summary>
		/// Find the slot that contains the given offset.
		/// </summary>
		/// <param name="offset">The target offset. Must be between 0 and <see cref="GreenNode.FullWidth"/>.</param>
		/// <returns>The slot index of the slot containing the given offset.</returns>
		/// <remarks>
		/// This implementation uses a binary search to find the first slot that contains
		/// the given offset.
		/// </remarks>
		public override int FindSlotIndexContainingOffset(int offset)
		{
			Debug.Assert(offset >= 0 && offset < FullWidth);
			return _childOffsets.BinarySearchUpperBound(offset) - 1;
		}

		private static int[] CalculateOffsets(GreenNode[] children)
		{
			int n = children.Length;
			var childOffsets = new int[n];
			int offset = 0;
			for (int i = 0; i < n; i++)
			{
				childOffsets[i] = offset;
				offset += children[i].FullWidth;
			}
			return childOffsets;
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] errors)
		{
			return new WithLotsOfChildren(errors, this.GetAnnotations(), children, _childOffsets);
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new WithLotsOfChildren(GetDiagnostics(), annotations, children, _childOffsets);
		}
	}

	public abstract class WithManyChildrenBase : SyntaxList
	{
		public readonly GreenNode[] children;

		public WithManyChildrenBase(GreenNode[] children)
		{
			this.children = children;
			this.InitializeChildren();
		}

		public WithManyChildrenBase(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, GreenNode[] children)
			: base(diagnostics, annotations)
		{
			this.children = children;
			this.InitializeChildren();
		}

		private void InitializeChildren()
		{
			int n = children.Length;
			if (n < byte.MaxValue)
			{
				this.SlotCount = (byte)n;
			}
			else
			{
				this.SlotCount = byte.MaxValue;
			}

			for (int i = 0; i < children.Length; i++)
			{
				this.AdjustFlagsAndWidth(children[i]);
			}
		}

		protected override int GetSlotCount()
		{
			return children.Length;
		}

		public override GreenNode GetSlot(int index)
		{
			return this.children[index];
		}

		public override void CopyTo(GreenNode[] array, int offset)
		{
			Array.Copy(this.children, 0, array, offset, this.children.Length);
		}

		private bool HasNodeTokenPattern()
		{
			for (int i = 0; i < this.SlotCount; i++)
			{
				// even slots must not be tokens, odds slots must be tokens
				if (this.GetSlot(i).IsToken == ((i & 1) == 0))
				{
					return false;
				}
			}

			return true;
		}
	}

	public sealed class WithManyChildren : WithManyChildrenBase
	{
		public WithManyChildren(GreenNode[] children)
			: base(children)
		{
		}

		public WithManyChildren(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, GreenNode[] children)
			: base(diagnostics, annotations, children)
		{
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] errors)
		{
			return new WithManyChildren(errors, this.GetAnnotations(), children);
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new WithManyChildren(GetDiagnostics(), annotations, children);
		}
	}

	public class WithThreeChildren : SyntaxList
	{
		private readonly GreenNode _child0;
		private readonly GreenNode _child1;
		private readonly GreenNode _child2;

		public WithThreeChildren(GreenNode child0, GreenNode child1, GreenNode child2)
		{
			this.SlotCount = 3;
			this.AdjustFlagsAndWidth(child0);
			_child0 = child0;
			this.AdjustFlagsAndWidth(child1);
			_child1 = child1;
			this.AdjustFlagsAndWidth(child2);
			_child2 = child2;
		}

		public WithThreeChildren(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, GreenNode child0, GreenNode child1, GreenNode child2)
			: base(diagnostics, annotations)
		{
			this.SlotCount = 3;
			this.AdjustFlagsAndWidth(child0);
			_child0 = child0;
			this.AdjustFlagsAndWidth(child1);
			_child1 = child1;
			this.AdjustFlagsAndWidth(child2);
			_child2 = child2;
		}


		public override GreenNode GetSlot(int index)
		{
			switch (index)
			{
				case 0:
					return _child0;
				case 1:
					return _child1;
				case 2:
					return _child2;
				default:
					return null;
			}
		}

		public override void CopyTo(GreenNode[] array, int offset)
		{
			array[offset] = _child0;
			array[offset + 1] = _child1;
			array[offset + 2] = _child2;
		}
		public override GreenNode SetDiagnostics(DiagnosticInfo[] errors)
		{
			return new WithThreeChildren(errors, this.GetAnnotations(), _child0, _child1, _child2);
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new WithThreeChildren(GetDiagnostics(), annotations, _child0, _child1, _child2);
		}
	}

	public class WithTwoChildren : SyntaxList
	{
		private readonly GreenNode _child0;
		private readonly GreenNode _child1;

		public WithTwoChildren(GreenNode child0, GreenNode child1)
		{
			this.SlotCount = 2;
			this.AdjustFlagsAndWidth(child0);
			_child0 = child0;
			this.AdjustFlagsAndWidth(child1);
			_child1 = child1;
		}

		public WithTwoChildren(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, GreenNode child0, GreenNode child1)
			: base(diagnostics, annotations)
		{
			this.SlotCount = 2;
			this.AdjustFlagsAndWidth(child0);
			_child0 = child0;
			this.AdjustFlagsAndWidth(child1);
			_child1 = child1;
		}

		public override GreenNode GetSlot(int index)
		{
			switch (index)
			{
				case 0:
					return _child0;
				case 1:
					return _child1;
				default:
					return null;
			}
		}

		public override void CopyTo(GreenNode[] array, int offset)
		{
			array[offset] = _child0;
			array[offset + 1] = _child1;
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] errors)
		{
			return new WithTwoChildren(errors, this.GetAnnotations(), _child0, _child1);
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new WithTwoChildren(GetDiagnostics(), annotations, _child0, _child1);
		}
	}
}
