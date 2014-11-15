using System;

namespace Platform
{
	/// <summary>
	/// An interface that provides on a view onto an object that provides a certain value
	/// including maximum and minimum possible values.  An <see cref="IMeter"/> object
	/// is analogous to a real world meter such as a power meter or petrol gage.
	/// </summary>
	public interface IMeter
		: IModel, IValued
	{
		/// <summary>
		/// An event that is raised when the <see cref="CurrentValue"/> changes.
		/// </summary>
		event EventHandler<MeterEventArgs> ValueChanged;

		/// <summary>
		/// The maximum possible value for the meter.
		/// </summary>
		object MaximumValue
		{
			get;
		}

		/// <summary>
		/// The minimum possible value for the meter.
		/// </summary>
		object MinimumValue
		{
			get;
		}

		/// <summary>
		/// The current value for the meter.
		/// </summary>
		object CurrentValue
		{
			get;
		}

		/// <summary>
		/// Returns a value between 0 and 1 that represents how far along the 
		/// <see cref="CurrentValue"/> is between the <see cref="MinimumValue"/>
		/// and <see cref="MaximumValue"/>.
		/// </summary>
		double Percentage
		{
			get;
		}

		/// <summary>
		/// Gets the units (as a string) that the meter should be measured in.
		/// </summary>
		string Units
		{
			get;
		}
	}
}
