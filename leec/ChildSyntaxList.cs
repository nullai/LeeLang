using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public struct ChildSyntaxList
	{
		private readonly GreenNode _node;
		private int _count;

		public ChildSyntaxList(GreenNode node)
		{
			_node = node;
			_count = -1;
		}

		public int Count
		{
			get
			{
				if (_count == -1)
				{
					_count = this.CountNodes();
				}

				return _count;
			}
		}

		private int CountNodes()
		{
			int n = 0;
			var enumerator = this.GetEnumerator();
			while (enumerator.MoveNext())
			{
				n++;
			}

			return n;
		}

		// for debugging
		private GreenNode[] Nodes
		{
			get
			{
				var result = new GreenNode[this.Count];
				var i = 0;

				foreach (var n in this)
				{
					result[i++] = n;
				}

				return result;
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_node);
		}

		public Reversed Reverse()
		{
			return new Reversed(_node);
		}

		public struct Enumerator
		{
			private readonly GreenNode _node;
			private int _childIndex;
			private GreenNode _list;
			private int _listIndex;
			private GreenNode _currentChild;

			public Enumerator(GreenNode node)
			{
				_node = node;
				_childIndex = -1;
				_listIndex = -1;
				_list = null;
				_currentChild = null;
			}

			public bool MoveNext()
			{
				if (_node != null)
				{
					if (_list != null)
					{
						_listIndex++;

						if (_listIndex < _list.SlotCount)
						{
							_currentChild = _list.GetSlot(_listIndex);
							return true;
						}

						_list = null;
						_listIndex = -1;
					}

					while (true)
					{
						_childIndex++;

						if (_childIndex == _node.SlotCount)
						{
							break;
						}

						var child = _node.GetSlot(_childIndex);
						if (child == null)
						{
							continue;
						}

						if (child.RawKind == GreenNode.ListKind)
						{
							_list = child;
							_listIndex++;

							if (_listIndex < _list.SlotCount)
							{
								_currentChild = _list.GetSlot(_listIndex);
								return true;
							}
							else
							{
								_list = null;
								_listIndex = -1;
								continue;
							}
						}
						else
						{
							_currentChild = child;
						}

						return true;
					}
				}

				_currentChild = null;
				return false;
			}

			public GreenNode Current
			{
				get { return _currentChild; }
			}
		}

		public struct Reversed
		{
			private readonly GreenNode _node;

			public Reversed(GreenNode node)
			{
				_node = node;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_node);
			}
			public struct Enumerator
			{
				private readonly GreenNode _node;
				private int _childIndex;
				private GreenNode _list;
				private int _listIndex;
				private GreenNode _currentChild;

				public Enumerator(GreenNode node)
				{
					if (node != null)
					{
						_node = node;
						_childIndex = node.SlotCount;
						_listIndex = -1;
					}
					else
					{
						_node = null;
						_childIndex = 0;
						_listIndex = -1;
					}

					_list = null;
					_currentChild = null;
				}

				public bool MoveNext()
				{
					if (_node != null)
					{
						if (_list != null)
						{
							if (--_listIndex >= 0)
							{
								_currentChild = _list.GetSlot(_listIndex);
								return true;
							}

							_list = null;
							_listIndex = -1;
						}

						while (--_childIndex >= 0)
						{
							var child = _node.GetSlot(_childIndex);
							if (child == null)
							{
								continue;
							}

							if (child.IsList)
							{
								_list = child;
								_listIndex = _list.SlotCount;
								if (--_listIndex >= 0)
								{
									_currentChild = _list.GetSlot(_listIndex);
									return true;
								}
								else
								{
									_list = null;
									_listIndex = -1;
									continue;
								}
							}
							else
							{
								_currentChild = child;
							}

							return true;
						}
					}

					_currentChild = null;
					return false;
				}

				public GreenNode Current
				{
					get { return _currentChild; }
				}
			}
		}
	}
}
