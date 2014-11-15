using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Platform.Collections;

namespace Platform.Text.RegularExpressions
{
	public class RegexCache
	{
		public static readonly RegexCache Default = new RegexCache(TimeSpan.FromMinutes(60));

		private struct CacheKey
		{
		    internal readonly string regex;
			internal readonly RegexOptions options;

			public CacheKey(string regex, RegexOptions options)
			{
				this.regex = regex;
				this.options = options;
			}
		}

		private class CacheKeyEqualityComparer
			: IEqualityComparer<CacheKey>
		{
			public static readonly CacheKeyEqualityComparer Default = new CacheKeyEqualityComparer();

			public bool Equals(CacheKey x, CacheKey y)
			{
				return x.options == y.options && x.regex == y.regex;
			}

			public int GetHashCode(CacheKey obj)
			{
				return (obj.regex == null ? 0 : obj.regex.GetHashCode() ^ (int)obj.options);
			}
		}

		private readonly IDictionary<CacheKey, Regex> cache;

		public RegexCache(TimeSpan timeout)
		{
			cache = new TimedReferenceDictionary<CacheKey, Regex>(timeout);
		}

		public Regex Create(string regex, RegexOptions options)
		{
			lock (cache)
			{
				Regex retval;
				var key = new CacheKey(regex, options);

				if (cache.TryGetValue(key, out retval))
				{
					return retval;
				}

				options |= RegexOptions.Compiled;

				retval = new Regex(regex, options);

				cache[key] = retval;

				return retval;
			}
		}
	}
}

