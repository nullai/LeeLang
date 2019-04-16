using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public static class KeyValuePairUtil
	{
		public static KeyValuePair<K, V> Create<K, V>(K key, V value)
		{
			return new KeyValuePair<K, V>(key, value);
		}

		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value)
		{
			key = keyValuePair.Key;
			value = keyValuePair.Value;
		}
	}
}
