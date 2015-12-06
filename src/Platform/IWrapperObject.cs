// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

namespace Platform
{
	/// <summary>
	/// An interface implemented by objects that are wrappers.
	/// </summary>
	/// <typeparam name="T">The type of object being wrapped</typeparam>
	public interface IWrapperObject<out T>
	{
		T Wrappee { get; }
	}
}
