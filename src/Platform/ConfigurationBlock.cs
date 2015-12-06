// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System.Collections.Generic;
using System.Configuration;

namespace Platform
{
	public class ConfigurationBlock<T>
	{
		private static readonly IDictionary<string, T> BlockCache;

		static ConfigurationBlock()
		{
			BlockCache = new Dictionary<string, T>();
		}

		public static T Load(string configurationPath)
		{
			return Load(configurationPath, true);
		}

		public static T Load(string configurationPath, bool reload)
		{
			T retval;

			lock (BlockCache)
			{
				if (!reload)
				{
					if (BlockCache.TryGetValue(configurationPath, out retval))
					{
						return retval;
					}
				}

				retval = (T)ConfigurationManager.GetSection(configurationPath);
				
				BlockCache[configurationPath] = retval;
			}

			return retval;
		}
	}
}
