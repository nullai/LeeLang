using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class ItemTokenMap<T> where T : class
	{
		private readonly Dictionary<T, uint> _itemToToken = new Dictionary<T, uint>();
		private readonly List<T> _items = new List<T>();

		public uint GetOrAddTokenFor(T item)
		{
			uint token;
			// NOTE: cannot use GetOrAdd here since items and itemToToken must be in sync
			// so if we do need to add we have to take a lock and modify both collections.
			if (_itemToToken.TryGetValue(item, out token))
			{
				return token;
			}

			return AddItem(item);
		}

		private uint AddItem(T item)
		{
			uint token;

			lock (_items)
			{
				if (_itemToToken.TryGetValue(item, out token))
				{
					return token;
				}

				token = (uint)_items.Count;
				_items.Add(item);
				_itemToToken.Add(item, token);
			}

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
	}
}
