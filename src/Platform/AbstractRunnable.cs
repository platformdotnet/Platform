// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Platform
{
	/// <summary>
	/// Base class for all <see cref="IRunnable"/> implementations
	/// </summary>
	public abstract class AbstractRunnable
		: MarshalByRefObject, IRunnable
	{
		/// <summary>
		/// Performs the <c>Run</c> operation
		/// </summary>
		public abstract void Run();
	}
}