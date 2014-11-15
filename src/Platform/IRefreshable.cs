// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

namespace Platform
{
	/// <summary>
	/// An interface for objects that can be refreshed.
	/// </summary>
	public interface IRefreshable
	{
		/// <summary>
		/// Refreshes the current object (what refresh means depends on the object).
		/// </summary>
		void Refresh();
	}
}
