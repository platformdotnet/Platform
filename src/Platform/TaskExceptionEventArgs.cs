using System;

namespace Platform
{
	/// <summary>
	/// Class that holds event information for task events.
	/// </summary>
	/// <seealso cref="ITask"/>
	/// <seealso cref="AbstractTask"/>
	public class TaskExceptionEventArgs
		: EventArgs
	{
		public TaskExceptionEventArgs(Exception exception)
		{
			this.Exception = exception;
		}

		/// <summary>
		/// Exception
		/// </summary>
		public virtual Exception Exception { get; private set; }
	}
}