using System.Collections.Generic;

namespace Platform.Collections
{
	public static class DictionaryExtensions
	{
		public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dictionary, K key)
		{
			V result;

			if (dictionary.TryGetValue(key, out result))
			{
				return result;
			}

			return default(V);
		}

		public static V? GetValueOrNull<K, V>(this Dictionary<K, V> dictionary, K key)
			where V : struct
		{
			V result;

			if (dictionary.TryGetValue(key, out result))
			{
				return result;
			}

			return null;
		}
	}
}
