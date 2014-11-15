// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

namespace Platform
{
	/// <summary>
	/// Interface for objects that can be run.
	/// </summary>
	public interface IRunnable
	{
		/// <summary>
		/// Runs the object (usually in the current thread)
		/// </summary>
		void Run();
	}
}
