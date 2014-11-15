// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

namespace Platform
{
	/// <summary>
	/// Interface for objects that contain a <see cref="Key"/> property.
	/// </summary>
	public interface IKeyed
	{
		/// <summary>
		/// The <see cref="Key"/>.
		/// </summary>
		object Key
		{
			get;
		}
	}

	/// <summary>
	/// Interface for objects that contain a <see cref="Key"/> property.
	/// </summary>
	public interface IKeyed<T>
		: IKeyed
	{
		/// <summary>
		/// The <see cref="Key"/>.
		/// </summary>
		new T Key
		{
			get;
		}
	}
}
