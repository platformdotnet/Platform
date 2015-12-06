// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Platform
{
	/// <summary>
	/// A delegate that creates an object from a type.
	/// </summary>
	/// <param name="type">
	/// The type of object to create.
	/// </param>
	/// <returns>The newly created object</returns>
	public delegate object ObjectActivator(Type type);

	/// <summary>
	/// A delegate that creates an object from a type.
	/// </summary>
	/// <typeparam name="V">The type of object to return</typeparam>
	/// <param name="type">
	/// The type of object to create.  Type must be <see cref="V"/> or be 
	/// a child of <see cref="V"/>
	/// </param>
	/// <returns>The newly created object</returns>
	public delegate V ObjectActivator<out V>(Type type);
}
