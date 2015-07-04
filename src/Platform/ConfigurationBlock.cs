// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System.Collections.Generic;
using System.Configuration;

namespace Platform
{
	public class ConfigurationBlock<T>
	{
		private static readonly IDictionary<string, T> blockCache;

		static ConfigurationBlock()
		{
			blockCache = new Dictionary<string, T>();
		}

		public static T Load(string configurationPath)
		{
			return Load(configurationPath, true);
		}

		public static T Load(string configurationPath, bool reload)
		{
			T retval;

			lock (blockCache)
			{
				if (!reload)
				{
					if (blockCache.TryGetValue(configurationPath, out retval))
					{
						return retval;
					}
				}

				retval = (T)ConfigurationManager.GetSection(configurationPath);
				
				blockCache[configurationPath] = retval;
			}

			return retval;
		}
	}
}
