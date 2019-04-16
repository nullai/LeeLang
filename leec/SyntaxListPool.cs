using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxListPool
	{
		private SyntaxListBuilder[] _freeList = new SyntaxListBuilder[10];
		private int _freeIndex;

#if DEBUG
		private readonly List<SyntaxListBuilder> _allocated = new List<SyntaxListBuilder>();
#endif

		public SyntaxListPool()
		{
		}

		public SyntaxListBuilder Allocate()
		{
			SyntaxListBuilder item;
			if (_freeIndex > 0)
			{
				_freeIndex--;
				item = _freeList[_freeIndex];
				_freeList[_freeIndex] = null;
			}
			else
			{
				item = new SyntaxListBuilder(10);
			}

#if DEBUG
			Debug.Assert(!_allocated.Contains(item));
			_allocated.Add(item);
#endif
			return item;
		}

		public SyntaxListBuilder<TNode> Allocate<TNode>() where TNode : GreenNode
		{
			return new SyntaxListBuilder<TNode>(this.Allocate());
		}

		public SeparatedSyntaxListBuilder<TNode> AllocateSeparated<TNode>() where TNode : GreenNode
		{
			return new SeparatedSyntaxListBuilder<TNode>(this.Allocate());
		}

		public void Free<TNode>(SeparatedSyntaxListBuilder<TNode> item) where TNode : GreenNode
		{
			Free(item.UnderlyingBuilder);
		}

		public void Free(SyntaxListBuilder item)
		{
			item.Clear();
			if (_freeIndex >= _freeList.Length)
			{
				this.Grow();
			}
#if DEBUG
			Debug.Assert(_allocated.Contains(item));

			_allocated.Remove(item);
#endif
			_freeList[_freeIndex] = item;
			_freeIndex++;
		}

		private void Grow()
		{
			var tmp = new SyntaxListBuilder[_freeList.Length * 2];
			Array.Copy(_freeList, tmp, _freeList.Length);
			_freeList = tmp;
		}

		public SyntaxList<TNode> ToListAndFree<TNode>(SyntaxListBuilder<TNode> item)
			where TNode : GreenNode
		{
			var list = item.ToList();
			Free(item);
			return list;
		}
	}
}
