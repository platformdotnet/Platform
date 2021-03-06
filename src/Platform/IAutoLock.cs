﻿// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Platform
{
	/// <summary>
	/// An interface for objects that can be locked and unlocked.
	/// Please refer to <see cref="AutoLock"/>.
	/// </summary>
	public interface IAutoLock
		: IDisposable
	{
		/// <summary>
		/// Locks the object.
		/// </summary>
		/// <returns>The current object</returns>
		IAutoLock Lock();

		/// <summary>
		/// Unlocks the object.
		/// </summary>
		/// <returns>The current object</returns>
		IAutoLock Unlock();
	}
}
