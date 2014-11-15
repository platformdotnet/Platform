// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Platform
{
	/// <summary>
	/// Contains event information for the <see cref="IMeter"/> interface.
	/// </summary>
	public class MeterEventArgs
		: EventArgs, IValued
	{
		/// <summary>
		/// Gets the current value (<see cref="NewValue"/>)
		/// </summary>
		public virtual object Value
		{
			get
			{
				return this.NewValue;
			}
		}

		/// <summary>
		/// Gets the current value (<see cref="NewValue"/>)
		/// </summary>
		object IValued.Value
		{
			get
			{
				return this.NewValue;
			}
		}

		/// <summary>
		/// Gets the current value.
		/// </summary>
		public virtual object NewValue
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the previous value
		/// </summary>
		public virtual object OldValue
		{
			get;
			private set;
		}

		/// <summary>
		/// Constructs a new <see cref="MeterEventArgs"/>.
		/// </summary>
		/// <param name="newValue">The new value</param>
		/// <param name="oldValue">The old value</param>
		public MeterEventArgs(object newValue, object oldValue)
		{
			this.NewValue = newValue;
			this.OldValue = oldValue;
		}
	}
}