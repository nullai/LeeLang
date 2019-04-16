using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct SeparatedSyntaxList<TNode> : IEquatable<SeparatedSyntaxList<TNode>>, IEnumerable<TNode> where TNode : GreenNode
	{
		private readonly SyntaxList<GreenNode> _list;

		public SeparatedSyntaxList(SyntaxList<GreenNode> list)
		{
			Validate(list);
			_list = list;
		}

		[Conditional("DEBUG")]
		private static void Validate(SyntaxList<GreenNode> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				var item = list[i];
				if ((i & 1) == 0)
				{
					Debug.Assert(!item.IsToken, "even elements of a separated list must be nodes");
				}
				else
				{
					Debug.Assert(item.IsToken, "odd elements of a separated list must be tokens");
				}
			}
		}

		public GreenNode Node => _list.Node;

		public int Count
		{
			get
			{
				return (_list.Count + 1) >> 1;
			}
		}

		public int SeparatorCount
		{
			get
			{
				return _list.Count >> 1;
			}
		}

		public TNode this[int index]
		{
			get
			{
				return (TNode)_list[index << 1];
			}
		}

		/// <summary>
		/// Gets the separator at the given index in this list.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public GreenNode GetSeparator(int index)
		{
			return _list[(index << 1) + 1];
		}

		public SyntaxList<GreenNode> GetWithSeparators()
		{
			return _list;
		}

		public static bool operator ==(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right)
		{
			return !left.Equals(right);
		}

		public bool Equals(SeparatedSyntaxList<TNode> other)
		{
			return _list == other._list;
		}

		public override bool Equals(object obj)
		{
			return (obj is SeparatedSyntaxList<TNode>) && Equals((SeparatedSyntaxList<TNode>)obj);
		}

		public override int GetHashCode()
		{
			return _list.GetHashCode();
		}

		public static implicit operator SeparatedSyntaxList<GreenNode>(SeparatedSyntaxList<TNode> list)
		{
			return new SeparatedSyntaxList<GreenNode>(list.GetWithSeparators());
		}

		public struct Enumerator
		{
			private readonly SeparatedSyntaxList<TNode> _list;
			private int _index;

			public Enumerator(SeparatedSyntaxList<TNode> list)
			{
				_list = list;
				_index = -1;
			}

			public bool MoveNext()
			{
				int newIndex = _index + 1;
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

			public void Reset()
			{
				_index = -1;
			}

			public override bool Equals(object obj)
			{
				throw new NotSupportedException();
			}

			public override int GetHashCode()
			{
				throw new NotSupportedException();
			}
		}

		// IEnumerator wrapper for Enumerator.
		private class EnumeratorImpl : IEnumerator<TNode>
		{
			private Enumerator _e;

			public EnumeratorImpl(SeparatedSyntaxList<TNode> list)
			{
				_e = new Enumerator(list);
			}

			TNode IEnumerator<TNode>.Current
			{
				get
				{
					return _e.Current;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return _e.Current;
				}
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return _e.MoveNext();
			}

			public void Reset()
			{
				_e.Reset();
			}

			void IDisposable.Dispose()
			{
				throw new NotImplementedException();
			}

			bool IEnumerator.MoveNext()
			{
				throw new NotImplementedException();
			}

			void IEnumerator.Reset()
			{
				throw new NotImplementedException();
			}
		}

		IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
		{
			return new EnumeratorImpl(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new EnumeratorImpl(this);
		}
	}
}
