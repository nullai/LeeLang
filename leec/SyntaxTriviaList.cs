using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct SyntaxTriviaList : IEquatable<SyntaxTriviaList>
	{
		public static SyntaxTriviaList Empty => default(SyntaxTriviaList);

		public SyntaxTriviaList(SyntaxToken token, GreenNode node, int position, int index = 0)
		{
			Token = token;
			Node = node;
			Position = position;
			Index = index;
		}

		public SyntaxTriviaList(SyntaxToken token, GreenNode node)
		{
			Token = token;
			Node = node;
			Position = token.Position;
			Index = 0;
		}

		public SyntaxTriviaList(SyntaxTrivia trivia)
		{
			Token = default(SyntaxToken);
			Node = trivia;
			Position = 0;
			Index = 0;
		}

		/// <summary>
		/// Creates a list of trivia.
		/// </summary>
		/// <param name="trivias">An array of trivia.</param>
		public SyntaxTriviaList(params SyntaxTrivia[] trivias)
			: this(null, CreateNode(trivias), 0, 0)
		{
		}

		/// <summary>
		/// Creates a list of trivia.
		/// </summary>
		/// <param name="trivias">A sequence of trivia.</param>
		public SyntaxTriviaList(IEnumerable<SyntaxTrivia> trivias)
			: this(null, SyntaxTriviaListBuilder.Create(trivias).Node, 0, 0)
		{
		}

		private static GreenNode CreateNode(SyntaxTrivia[] trivias)
		{
			if (trivias == null)
			{
				return null;
			}

			var builder = new SyntaxTriviaListBuilder(trivias.Length);
			builder.Add(trivias);
			return builder.ToList().Node;
		}

		public SyntaxToken Token { get; }

		public GreenNode Node { get; }

		public int Position { get; }

		public int Index { get; }

		public int Count
		{
			get { return Node == null ? 0 : (Node.IsList ? Node.SlotCount : 1); }
		}

		public SyntaxTrivia ElementAt(int index)
		{
			return this[index];
		}

		/// <summary>
		/// Gets the trivia at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the trivia to get.</param>
		/// <returns>The token at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="Count" />. </exception>
		public SyntaxTrivia this[int index]
		{
			get
			{
				//if (Node != null)
				//{
				//	if (Node.IsList)
				//	{
				//		if (unchecked((uint)index < (uint)Node.SlotCount))
				//		{
				//			return new SyntaxTrivia(Token, Node.GetSlot(index), Position + Node.GetSlotOffset(index), Index + index);
				//		}
				//	}
				//	else if (index == 0)
				//	{
				//		return new SyntaxTrivia(Token, Node, Position, Index);
				//	}
				//}

				throw new ArgumentOutOfRangeException(nameof(index));
			}
		}

		public List<SyntaxTrivia> ToList()
		{
			throw new Exception();
		}

		public SyntaxTrivia[] ToArray()
		{
			throw new Exception();
		}

		/// <summary>
		/// The absolute span of the list elements in characters, including the leading and trailing trivia of the first and last elements.
		/// </summary>
		public TextSpan FullSpan
		{
			get
			{
				if (Node == null)
				{
					return default(TextSpan);
				}

				return new TextSpan(this.Position, Node.FullWidth);
			}
		}

		/// <summary>
		/// The absolute span of the list elements in characters, not including the leading and trailing trivia of the first and last elements.
		/// </summary>
		public TextSpan Span
		{
			get
			{
				if (Node == null)
				{
					return default(TextSpan);
				}

				return TextSpan.FromBounds(Position + Node.GetLeadingTriviaWidth(),
					Position + Node.FullWidth - Node.GetTrailingTriviaWidth());
			}
		}

		/// <summary>
		/// Returns the first trivia in the list.
		/// </summary>
		/// <returns>The first trivia in the list.</returns>
		/// <exception cref="InvalidOperationException">The list is empty.</exception>        
		public SyntaxTrivia First()
		{
			if (Any())
			{
				return this[0];
			}

			throw new InvalidOperationException();
		}

		/// <summary>
		/// Returns the last trivia in the list.
		/// </summary>
		/// <returns>The last trivia in the list.</returns>
		/// <exception cref="InvalidOperationException">The list is empty.</exception>        
		public SyntaxTrivia Last()
		{
			if (Any())
			{
				return this[this.Count - 1];
			}

			throw new InvalidOperationException();
		}

		/// <summary>
		/// Does this list have any items.
		/// </summary>
		public bool Any()
		{
			return Node != null;
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

		/// <summary>
		/// Returns a list which contains all elements of <see cref="SyntaxTriviaList"/> in reversed order.
		/// </summary>
		/// <returns><see cref="Reversed"/> which contains all elements of <see cref="SyntaxTriviaList"/> in reversed order</returns>
		public Reversed Reverse()
		{
			return new Reversed(this);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public int IndexOf(SyntaxTrivia triviaInList)
		{
			for (int i = 0, n = this.Count; i < n; i++)
			{
				var trivia = this[i];
				if (trivia == triviaInList)
				{
					return i;
				}
			}

			return -1;
		}

		public int IndexOf(int rawKind)
		{
			for (int i = 0, n = this.Count; i < n; i++)
			{
				if (this[i].RawKind == rawKind)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Creates a new <see cref="SyntaxTriviaList"/> with the specified trivia added to the end.
		/// </summary>
		/// <param name="trivia">The trivia to add.</param>
		public SyntaxTriviaList Add(SyntaxTrivia trivia)
		{
			return Insert(this.Count, trivia);
		}

		/// <summary>
		/// Creates a new <see cref="SyntaxTriviaList"/> with the specified trivia added to the end.
		/// </summary>
		/// <param name="trivia">The trivia to add.</param>
		public SyntaxTriviaList AddRange(IEnumerable<SyntaxTrivia> trivia)
		{
			return InsertRange(this.Count, trivia);
		}

		/// <summary>
		/// Creates a new <see cref="SyntaxTriviaList"/> with the specified trivia inserted at the index.
		/// </summary>
		/// <param name="index">The index in the list to insert the trivia at.</param>
		/// <param name="trivia">The trivia to insert.</param>
		public SyntaxTriviaList Insert(int index, SyntaxTrivia trivia)
		{
			if (trivia == default(SyntaxTrivia))
			{
				throw new ArgumentOutOfRangeException(nameof(trivia));
			}

			return InsertRange(index, new[] { trivia });
		}

		private static readonly ObjectPool<SyntaxTriviaListBuilder> s_builderPool =
			new ObjectPool<SyntaxTriviaListBuilder>(() => SyntaxTriviaListBuilder.Create());

		private static SyntaxTriviaListBuilder GetBuilder()
			=> s_builderPool.Allocate();

		private static void ClearAndFreeBuilder(SyntaxTriviaListBuilder builder)
		{
			// It's possible someone might create a list with a huge amount of trivia
			// in it.  We don't want to hold onto such items forever.  So only cache
			// reasonably sized lists.  In IDE testing, around 99% of all trivia lists
			// were 16 or less elements.
			const int MaxBuilderCount = 16;
			if (builder.Count <= MaxBuilderCount)
			{
				builder.Clear();
				s_builderPool.Free(builder);
			}
		}

		/// <summary>
		/// Creates a new <see cref="SyntaxTriviaList"/> with the specified trivia inserted at the index.
		/// </summary>
		/// <param name="index">The index in the list to insert the trivia at.</param>
		/// <param name="trivia">The trivia to insert.</param>
		public SyntaxTriviaList InsertRange(int index, IEnumerable<SyntaxTrivia> trivia)
		{
			var thisCount = this.Count;
			if (index < 0 || index > thisCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (trivia == null)
			{
				throw new ArgumentNullException(nameof(trivia));
			}

			// Just return ourselves if we're not being asked to add anything.
			var triviaCollection = trivia as ICollection<SyntaxTrivia>;
			if (triviaCollection != null && triviaCollection.Count == 0)
			{
				return this;
			}

			var builder = GetBuilder();
			try
			{
				for (int i = 0; i < index; i++)
				{
					builder.Add(this[i]);
				}

				builder.AddRange(trivia);

				for (int i = index; i < thisCount; i++)
				{
					builder.Add(this[i]);
				}

				return builder.Count == thisCount ? this : builder.ToList();
			}
			finally
			{
				ClearAndFreeBuilder(builder);
			}
		}

		/// <summary>
		/// Creates a new <see cref="SyntaxTriviaList"/> with the element at the specified index removed.
		/// </summary>
		/// <param name="index">The index identifying the element to remove.</param>
		public SyntaxTriviaList RemoveAt(int index)
		{
			if (index < 0 || index >= this.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			var list = this.ToList();
			list.RemoveAt(index);
			return new SyntaxTriviaList(default(SyntaxToken), Node.CreateList(list), 0, 0);
		}

		/// <summary>
		/// Creates a new <see cref="SyntaxTriviaList"/> with the specified element removed.
		/// </summary>
		/// <param name="triviaInList">The trivia element to remove.</param>
		public SyntaxTriviaList Remove(SyntaxTrivia triviaInList)
		{
			var index = this.IndexOf(triviaInList);
			if (index >= 0 && index < this.Count)
			{
				return this.RemoveAt(index);
			}

			return this;
		}

		/// <summary>
		/// Creates a new <see cref="SyntaxTriviaList"/> with the specified element replaced with new trivia.
		/// </summary>
		/// <param name="triviaInList">The trivia element to replace.</param>
		/// <param name="newTrivia">The trivia to replace the element with.</param>
		public SyntaxTriviaList Replace(SyntaxTrivia triviaInList, SyntaxTrivia newTrivia)
		{
			if (newTrivia == default(SyntaxTrivia))
			{
				throw new ArgumentOutOfRangeException(nameof(newTrivia));
			}

			return ReplaceRange(triviaInList, new[] { newTrivia });
		}

		/// <summary>
		/// Creates a new <see cref="SyntaxTriviaList"/> with the specified element replaced with new trivia.
		/// </summary>
		/// <param name="triviaInList">The trivia element to replace.</param>
		/// <param name="newTrivia">The trivia to replace the element with.</param>
		public SyntaxTriviaList ReplaceRange(SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia)
		{
			var index = this.IndexOf(triviaInList);
			if (index >= 0 && index < this.Count)
			{
				var list = this.ToList();
				list.RemoveAt(index);
				list.InsertRange(index, newTrivia);
				return new SyntaxTriviaList(default(SyntaxToken), Node.CreateList(list), 0, 0);
			}

			throw new ArgumentOutOfRangeException(nameof(triviaInList));
		}

		// for debugging
		private SyntaxTrivia[] Nodes => this.ToArray();


		/// <summary>
		/// get the green node at the specific slot
		/// </summary>
		private GreenNode GetGreenNodeAt(int i)
		{
			return GetGreenNodeAt(Node, i);
		}

		private static GreenNode GetGreenNodeAt(GreenNode node, int i)
		{
			Debug.Assert(node.IsList || (i == 0 && !node.IsList));
			return node.IsList ? node.GetSlot(i) : node;
		}

		public bool Equals(SyntaxTriviaList other)
		{
			return Node == other.Node && Index == other.Index && Token.Equals(other.Token);
		}

		public static bool operator ==(SyntaxTriviaList left, SyntaxTriviaList right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SyntaxTriviaList left, SyntaxTriviaList right)
		{
			return !left.Equals(right);
		}

		public override bool Equals(object obj)
		{
			return (obj is SyntaxTriviaList) && Equals((SyntaxTriviaList)obj);
		}

		public override int GetHashCode()
		{
			return Hash.Combine(Token.GetHashCode(), Hash.Combine(Node, Index));
		}

		/// <summary>
		/// Copy <paramref name="count"/> number of items starting at <paramref name="offset"/> from this list into <paramref name="array"/> starting at <paramref name="arrayOffset"/>.
		/// </summary>
		public void CopyTo(int offset, SyntaxTrivia[] array, int arrayOffset, int count)
		{
			if (offset < 0 || count < 0 || this.Count < offset + count)
			{
				throw new IndexOutOfRangeException();
			}

			if (count == 0)
			{
				return;
			}

			// get first one without creating any red node
			var first = this[offset];
			array[arrayOffset] = first;

			// calculate trivia position from the first ourselves from now on
			var position = first.Position;
			var current = first;

			//for (int i = 1; i < count; i++)
			//{
			//	position += current.FullWidth;
			//	current = new SyntaxTrivia(Token, GetGreenNodeAt(offset + i), position, Index + i);

			//	array[arrayOffset + i] = current;
			//}
		}

		public override string ToString()
		{
			return Node != null ? Node.ToString() : string.Empty;
		}

		public string ToFullString()
		{
			return Node != null ? Node.ToFullString() : string.Empty;
		}

		public static SyntaxTriviaList Create(SyntaxTrivia trivia)
		{
			return new SyntaxTriviaList(trivia);
		}

		public struct Enumerator
		{
			private SyntaxToken _token;
			private GreenNode _singleNodeOrList;
			private int _baseIndex;
			private int _count;

			private int _index;
			private GreenNode _current;
			private int _position;

			public Enumerator(SyntaxTriviaList list)
			{
				_token = list.Token;
				_singleNodeOrList = list.Node;
				_baseIndex = list.Index;
				_count = list.Count;

				_index = -1;
				_current = null;
				_position = list.Position;
			}

			// PERF: Passing SyntaxToken by ref since it's a non-trivial struct
			private void InitializeFrom(SyntaxToken token, GreenNode greenNode, int index, int position)
			{
				_token = token;
				_singleNodeOrList = greenNode;
				_baseIndex = index;
				_count = greenNode.IsList ? greenNode.SlotCount : 1;

				_index = -1;
				_current = null;
				_position = position;
			}

			// PERF: Used to initialize an enumerator for leading trivia directly from a token.
			// This saves constructing an intermediate SyntaxTriviaList. Also, passing token
			// by ref since it's a non-trivial struct
			public void InitializeFromLeadingTrivia(SyntaxToken token)
			{
				InitializeFrom(token, token.GetLeadingTriviaCore(), 0, token.Position);
			}

			// PERF: Used to initialize an enumerator for trailing trivia directly from a token.
			// This saves constructing an intermediate SyntaxTriviaList. Also, passing token
			// by ref since it's a non-trivial struct
			public void InitializeFromTrailingTrivia(SyntaxToken token)
			{
				var leading = token.GetLeadingTriviaCore();
				int index = 0;
				if (leading != null)
				{
					index = leading.IsList ? leading.SlotCount : 1;
				}

				var trailingGreen = token.GetTrailingTriviaCore();
				int trailingPosition = token.Position + token.FullWidth;
				if (trailingGreen != null)
				{
					trailingPosition -= trailingGreen.FullWidth;
				}

				InitializeFrom(token, trailingGreen, index, trailingPosition);
			}

			public bool MoveNext()
			{
				int newIndex = _index + 1;
				if (newIndex >= _count)
				{
					// invalidate iterator
					_current = null;
					return false;
				}

				_index = newIndex;

				if (_current != null)
				{
					_position += _current.FullWidth;
				}

				_current = GetGreenNodeAt(_singleNodeOrList, newIndex);
				return true;
			}

			public SyntaxTrivia Current
			{
				get
				{
					//if (_current == null)
					{
						throw new InvalidOperationException();
					}

					//return new SyntaxTrivia(_token, _current, _position, _baseIndex + _index);
				}
			}
		}

		private class EnumeratorImpl : IEnumerator<SyntaxTrivia>
		{
			private Enumerator _enumerator;

			// SyntaxTriviaList is a relatively big struct so is passed as ref
			public EnumeratorImpl(SyntaxTriviaList list)
			{
				_enumerator = new Enumerator(list);
			}

			public SyntaxTrivia Current => _enumerator.Current;

			object IEnumerator.Current => _enumerator.Current;

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			public void Dispose()
			{
			}
		}

		public struct Reversed : IEnumerable<SyntaxTrivia>, IEquatable<Reversed>
		{
			private readonly SyntaxTriviaList _list;

			public Reversed(SyntaxTriviaList list)
			{
				_list = list;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_list);
			}

			IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
			{
				if (_list.Count == 0)
				{
					return null;
				}

				return new ReversedEnumeratorImpl(_list);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				if (_list.Count == 0)
				{
					return null;
				}

				return new ReversedEnumeratorImpl(_list);
			}

			public override int GetHashCode()
			{
				return _list.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				return obj is Reversed && Equals((Reversed)obj);
			}

			public bool Equals(Reversed other)
			{
				return _list.Equals(other._list);
			}

			public struct Enumerator
			{
				private readonly SyntaxToken _token;
				private readonly GreenNode _singleNodeOrList;
				private readonly int _baseIndex;
				private readonly int _count;

				private int _index;
				private GreenNode _current;
				private int _position;

				public Enumerator(SyntaxTriviaList list)
					: this()
				{
					if (list.Any())
					{
						_token = list.Token;
						_singleNodeOrList = list.Node;
						_baseIndex = list.Index;
						_count = list.Count;

						_index = _count;
						_current = null;

						var last = list.Last();
						_position = last.Position + last.FullWidth;
					}
				}

				public bool MoveNext()
				{
					if (_count == 0 || _index <= 0)
					{
						_current = null;
						return false;
					}

					_index--;

					_current = GetGreenNodeAt(_singleNodeOrList, _index);
					_position -= _current.FullWidth;

					return true;
				}

				public SyntaxTrivia Current
				{
					get
					{
						//if (_current == null)
						{
							throw new InvalidOperationException();
						}

						//return new SyntaxTrivia(_token, _current, _position, _baseIndex + _index);
					}
				}
			}

			private class ReversedEnumeratorImpl : IEnumerator<SyntaxTrivia>
			{
				private Enumerator _enumerator;

				// SyntaxTriviaList is a relatively big struct so is passed as ref
				public ReversedEnumeratorImpl(SyntaxTriviaList list)
				{
					_enumerator = new Enumerator(list);
				}

				public SyntaxTrivia Current => _enumerator.Current;

				object IEnumerator.Current => _enumerator.Current;

				public bool MoveNext()
				{
					return _enumerator.MoveNext();
				}

				public void Reset()
				{
					throw new NotSupportedException();
				}

				public void Dispose()
				{
				}
			}
		}
	}
}
