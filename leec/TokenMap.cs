using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class TokenMap<T> where T : class
	{
		private readonly Dictionary<T, uint> _itemIdentityToToken = new Dictionary<T, uint>();

		private readonly Dictionary<T, uint> _itemToToken = new Dictionary<T, uint>();
		private readonly List<T> _items = new List<T>();

		public uint GetOrAddTokenFor(T item, out bool referenceAdded)
		{
			uint tmp;
			if (_itemIdentityToToken.TryGetValue(item, out tmp))
			{
				referenceAdded = false;
				return (uint)tmp;
			}

			return AddItem(item, out referenceAdded);
		}

		private uint AddItem(T item, out bool referenceAdded)
		{
			uint token;

			// NOTE: cannot use GetOrAdd here since items and itemToToken must be in sync
			// so if we do need to add we have to take a lock and modify both collections.
			lock (_items)
			{
				if (!_itemToToken.TryGetValue(item, out token))
				{
					token = (uint)_items.Count;
					_items.Add(item);
					_itemToToken.Add(item, token);
				}
			}

			referenceAdded = !_itemIdentityToToken.ContainsKey(item);
			if (referenceAdded)
				_itemIdentityToToken.Add(item, token);
			return token;
		}

		public T GetItem(uint token)
		{
			lock (_items)
			{
				return _items[(int)token];
			}
		}

		public IEnumerable<T> GetAllItems()
		{
			lock (_items)
			{
				return _items.ToArray();
			}
		}

		//TODO: why is this is called twice during emit?
		//      should probably return ROA instead of IE and cache that in Module. (and no need to return count)
		public IEnumerable<T> GetAllItemsAndCount(out int count)
		{
			lock (_items)
			{
				count = _items.Count;
				return _items.ToArray();
			}
		}
	}
}
