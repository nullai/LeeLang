using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public class CachingFactory<TKey, TValue> : CachingBase<CachingFactory<TKey, TValue>.Entry>
	{
		public struct Entry
		{
			public int hash;
			public TValue value;
		}

		private readonly int _size;
		private readonly Func<TKey, TValue> _valueFactory;
		private readonly Func<TKey, int> _keyHash;
		private readonly Func<TKey, TValue, bool> _keyValueEquality;

		public CachingFactory(int size,
				Func<TKey, TValue> valueFactory,
				Func<TKey, int> keyHash,
				Func<TKey, TValue, bool> keyValueEquality) :
			base(size)
		{
			_size = size;
			_valueFactory = valueFactory;
			_keyHash = keyHash;
			_keyValueEquality = keyValueEquality;
		}

		public void Add(TKey key, TValue value)
		{
			var hash = GetKeyHash(key);
			var idx = hash & mask;

			entries[idx].hash = hash;
			entries[idx].value = value;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int hash = GetKeyHash(key);
			int idx = hash & mask;

			var entries = this.entries;
			if (entries[idx].hash == hash)
			{
				var candidate = entries[idx].value;
				if (_keyValueEquality(key, candidate))
				{
					value = candidate;
					return true;
				}
			}

			value = default(TValue);
			return false;
		}

		public TValue GetOrMakeValue(TKey key)
		{
			int hash = GetKeyHash(key);
			int idx = hash & mask;

			var entries = this.entries;
			if (entries[idx].hash == hash)
			{
				var candidate = entries[idx].value;
				if (_keyValueEquality(key, candidate))
				{
					return candidate;
				}
			}

			var value = _valueFactory(key);
			entries[idx].hash = hash;
			entries[idx].value = value;

			return value;
		}

		private int GetKeyHash(TKey key)
		{
			// Ensure result is non-zero to avoid
			// treating an empty entry as valid.
			int result = _keyHash(key) | _size;
			Debug.Assert(result != 0);
			return result;
		}
	}

	// special case for a situation where the key is a reference type with object identity 
	// in this case:
	//      keyHash             is assumed to be RuntimeHelpers.GetHashCode
	//      keyValueEquality    is an object == for the new and old keys 
	//                          NOTE: we do store the key in this case 
	//                          reference comparison of keys is as cheap as comparing hash codes.
	public class CachingIdentityFactory<TKey, TValue> : CachingBase<CachingIdentityFactory<TKey, TValue>.Entry>
		where TKey : class
	{
		private readonly Func<TKey, TValue> _valueFactory;
		private readonly ObjectPool<CachingIdentityFactory<TKey, TValue>> _pool;

		public struct Entry
		{
			public TKey key;
			public TValue value;
		}

		public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory) :
			base(size)
		{
			_valueFactory = valueFactory;
		}

		public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory, ObjectPool<CachingIdentityFactory<TKey, TValue>> pool) :
			this(size, valueFactory)
		{
			_pool = pool;
		}

		public void Add(TKey key, TValue value)
		{
			var hash = key.GetHashCode();
			var idx = hash & mask;

			entries[idx].key = key;
			entries[idx].value = value;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int hash = key.GetHashCode();
			int idx = hash & mask;

			var entries = this.entries;
			if ((object)entries[idx].key == (object)key)
			{
				value = entries[idx].value;
				return true;
			}

			value = default(TValue);
			return false;
		}

		public TValue GetOrMakeValue(TKey key)
		{
			int hash = key.GetHashCode();
			int idx = hash & mask;

			var entries = this.entries;
			if ((object)entries[idx].key == (object)key)
			{
				return entries[idx].value;
			}

			var value = _valueFactory(key);
			entries[idx].key = key;
			entries[idx].value = value;

			return value;
		}

		// if someone needs to create a pool;
		public static ObjectPool<CachingIdentityFactory<TKey, TValue>> CreatePool(int size, Func<TKey, TValue> valueFactory)
		{
			ObjectPool<CachingIdentityFactory<TKey, TValue>> pool = null;
			pool = new ObjectPool<CachingIdentityFactory<TKey, TValue>>(
				() => new CachingIdentityFactory<TKey, TValue>(size, valueFactory, pool), Environment.ProcessorCount * 2);

			return pool;
		}

		public void Free()
		{
			var pool = _pool;

			// Array.Clear(this.entries, 0, this.entries.Length);

			pool?.Free(this);
		}
	}

	// Just holds the data for the derived caches.
	public abstract class CachingBase<TEntry>
	{
		// cache size is always ^2. 
		// items are placed at [hash ^ mask]
		// new item will displace previous one at the same location.
		protected readonly int mask;
		protected readonly TEntry[] entries;

		public CachingBase(int size)
		{
			var alignedSize = AlignSize(size);
			this.mask = alignedSize - 1;
			this.entries = new TEntry[alignedSize];
		}

		private static int AlignSize(int size)
		{
			Debug.Assert(size > 0);

			size--;
			size |= size >> 1;
			size |= size >> 2;
			size |= size >> 4;
			size |= size >> 8;
			size |= size >> 16;
			return size + 1;
		}
	}
}
