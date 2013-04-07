using System;

namespace Platform
{
	/// <summary>
	/// Summary description for ICacheable.
	/// </summary>
	public interface ICacheable
	{
		event EventHandler CacheabilityChanged;
		
		/// <summary>
		/// If this property is true then the object can be removed from the cache when it spills.  This can be
		/// used to keep the object alive even if there are no other references to the object.
		/// If this property is false then the object may automatically be removed from the cache at any time.
		/// </summary>
		bool IsWeakCacheable
		{
			get;
		}
	}
}
