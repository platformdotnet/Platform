using System;
using System.IO;
using Platform.Models;
using System.Threading;

namespace Platform.IO
{
	public class StreamPump
		: AbstractTask
	{
		public static readonly int DefaultBufferSize = 4096;

		#region Meter Classes

		private class BytesReadPumpMeter
			: PumpMeter
		{
			public BytesReadPumpMeter(StreamPump pump)
				: base(pump)
			{
			}

			public override object CurrentValue
			{
				get
				{
					return m_Pump.m_BytesRead;
				}
			}

			public override object MaximumValue
			{
				get
				{
					return m_Pump.GetSourceLength();
				}
			}
		}


		private class BytesWrittenPumpMeter
			: PumpMeter
		{
			public BytesWrittenPumpMeter(StreamPump pump)
				: base(pump)
			{
			}

			public override object CurrentValue
			{
				get
				{
					return m_Pump.m_BytesWritten;
				}
			}

			public override object MaximumValue
			{
				get
				{
					return m_Pump.GetSourceLength();
				}
			}
		}


		private abstract class PumpMeter
			: AbstractMeter
		{
			protected StreamPump m_Pump;

			public PumpMeter(StreamPump pump)
			{
				m_Pump = pump;
			}

			public override object MinimumValue
			{
				get
				{
					return 0;
				}
			}

			public override string Units
			{
				get
				{
					return "bytes";
				}
			}

			public virtual void RaiseValueChanged(object oldValue)
			{
				OnValueChanged(oldValue, this.CurrentValue);
			}
		}


		#endregion

		#region Properties

		public virtual IMeter BytesReadMeter
		{
			get
			{
				return m_BytesReadMeter;
			}
		}
		private PumpMeter m_BytesReadMeter;

		public virtual IMeter BytesWrittenMeter
		{
			get
			{
				return m_BytesWrittenMeter;
			}
		}
		private PumpMeter m_BytesWrittenMeter;

		public override IMeter Progress
		{
			get
			{
				return this.BytesWrittenMeter;
			}
		}
		
		/// <summary>
		/// Sets/Gets the Source
		/// </summary>
		public Stream Source
		{
			get
			{
				if (m_Source == null)
				{
					m_Source = this.m_StreamProvider.GetSourceStream();
				}

				return m_Source;
			}
		}
		/// <remarks>
		/// <see cref="Source"/>
		/// </remarks>
		private Stream m_Source;

		/// <summary>
		/// Sets/Gets the Destination
		/// </summary>
		public Stream Destination
		{
			get
			{
				if (m_Destination == null)
				{
					m_Destination = this.m_StreamProvider.GetDestinationStream();
				}

				return m_Destination;
			}
		}
		/// <remarks>
		/// <see cref="Destination"/>
		/// </remarks>
		private Stream m_Destination;

		#endregion

		private byte[] m_Buffer;		
		private long m_BytesRead;
		private long m_BytesWritten;
		
		public interface IStreamProvider
		{
			Stream GetSourceStream();
			Stream GetDestinationStream();
		}

		private IStreamProvider m_StreamProvider;

		public StreamPump(IStreamProvider streamProvider, bool autoCloseSource, bool autoCloseDestination)
			: this(streamProvider, autoCloseSource, autoCloseDestination, DefaultBufferSize)
		{
		}

		public StreamPump(IStreamProvider streamProvider, bool autoCloseSource, bool autoCloseDestination, int bufferSize)
			: this(null, autoCloseSource, null, autoCloseDestination, bufferSize)
		{
			m_StreamProvider = streamProvider;
		}

		public StreamPump(Stream source, bool autoCloseSource, Stream destination, bool autoCloseDestination)
			: this(source, autoCloseSource, destination, autoCloseDestination, DefaultBufferSize)
		{
		}

		private bool m_AutoCloseSource, m_AutoCloseDestination;

		public StreamPump(Stream source, bool autoCloseSource, Stream destination, bool autoCloseDestination, int bufferSize)
		{
			if (bufferSize < 1)
			{
				bufferSize = DefaultBufferSize;
			}

			m_AutoCloseSource = autoCloseSource;
			m_AutoCloseDestination = autoCloseDestination;

			m_Buffer = new byte[bufferSize];

			m_Source = source;
			m_Destination = destination;
			m_BytesReadMeter = new BytesReadPumpMeter(this);
			m_BytesWrittenMeter = new BytesWrittenPumpMeter(this);

			m_TaskState = TaskState.NotStarted;
			m_RequestedTaskState = TaskState.Unknown;

			GetSourceLength();
		}

		private Thread m_PumpThread;

		private long m_LastKnownSourceLength = -1;

		internal long GetSourceLength()
		{
			if (m_Source == null)
			{
				return 0;
			}

			if (m_Source.CanSeek)
			{
				return m_LastKnownSourceLength = m_Source.Length;
			}
			else
			{
				if (m_LastKnownSourceLength == -1)
				{
					return Int64.MaxValue;
				}
				else
				{
					return m_LastKnownSourceLength;
				}
			}
		}

		protected virtual void SetTaskState(TaskState newState)
		{
			TaskState oldState;

			lock (this)
			{
				oldState = this.TaskState;

				m_TaskState = newState;

				OnTaskStateChanged(oldState, newState);
			}
		}
		
		public override void Run()
		{
			int read;

			lock (this)
			{
				if (this.TaskState != TaskState.NotStarted)
				{
					throw new InvalidOperationException("The pump has already started or finished");
				}

				m_TaskState = TaskState.Running;
			}

			m_PumpThread = Thread.CurrentThread;

			if (m_StreamProvider != null)
			{
				if (m_Source == null)
				{
					m_Source = m_StreamProvider.GetSourceStream();
				}

				if (m_Destination == null)
				{
					m_Destination = m_StreamProvider.GetDestinationStream();
				}

				GetSourceLength();
			}

			m_BytesReadMeter.RaiseValueChanged(0);
			m_BytesWrittenMeter.RaiseValueChanged(0);

			for (;;)
			{
				read = m_Source.Read(m_Buffer, 0, m_Buffer.Length);
				
				if (read == 0)
				{
					break;
				}

				m_BytesRead += read;
				m_BytesReadMeter.RaiseValueChanged(m_BytesRead - read);

				m_Destination.Write(m_Buffer, 0, read);

				m_BytesWritten += read;
				m_BytesWrittenMeter.RaiseValueChanged(m_BytesWritten - read);

				if (this.m_RequestedTaskState != TaskState.Unknown)
				{
					lock (this)
					{					
						for (;;)
						{
							if (this.m_RequestedTaskState == TaskState.Paused)
							{
								this.m_RequestedTaskState = TaskState.Unknown;
								SetTaskState(TaskState.Paused);

								Monitor.PulseAll(false);

								Monitor.Wait(this);
							}
							else if (this.m_RequestedTaskState == TaskState.Running)
							{
								this.m_RequestedTaskState = TaskState.Unknown;
								SetTaskState(TaskState.Running);

								Monitor.PulseAll(false);

								break;
							}
							else if (this.m_RequestedTaskState == TaskState.Stopped)
							{
								this.m_RequestedTaskState = TaskState.Unknown;
								SetTaskState(TaskState.Stopped);

								Monitor.PulseAll(false);

								break;
							}
						}
					}
				}

				if (this.m_TaskState == TaskState.Stopped)
				{
					break;
				}
			}

			m_Destination.Flush();

			if (m_AutoCloseSource)
			{
				m_Source.Close();
			}

			if (m_AutoCloseDestination)
			{
				m_Destination.Close();
			}
		}

		public override void Pause()
		{
			lock (m_ControlLock)
			{
				VerifyStarted();

				RequestTaskState(TaskState.Paused);
			}
		}

		public override void Resume()
		{
			lock (m_ControlLock)
			{
				VerifyStarted();

				if (this.TaskState != TaskState.Paused)
				{
					return;
				}

				RequestTaskState(TaskState.Running);

				Monitor.PulseAll(this);
			}
		}

		public override void Stop()
		{
			lock (m_ControlLock)
			{
				VerifyStarted();

				RequestTaskState(TaskState.Stopped);
			}
		}

		private void VerifyStarted()
		{
			if (this.TaskState == TaskState.NotStarted)
			{
				throw new InvalidOperationException("Task not started");
			}
		}

		public override void RequestTaskState(TaskState taskState)
		{
			lock (m_ControlLock)
			{
				VerifyStarted();

				lock (this)
				{
					for (;;)
					{
						if (taskState == TaskState.Finished
							|| taskState == TaskState.NotStarted
							|| taskState == TaskState.Unknown)
						{
							throw new ArgumentException("Illegal state", "taskstate");
						}

						m_RequestedTaskState = taskState;

						if (Thread.CurrentThread == m_PumpThread)
						{
							return;
						}

						if (m_TaskState == taskState
							|| m_TaskState == TaskState.Stopped
							|| m_TaskState == TaskState.Finished)
						{
							break;
						}

						Monitor.Wait(this);
					}
				}
			}
		}

		public override TaskState TaskState
		{
			get
			{
				return m_TaskState;
			}
		}

		private TaskState m_RequestedTaskState;
		private TaskState m_TaskState = TaskState.NotStarted;
		private object m_ControlLock = new object();
		

		public override bool SupportsStart
		{
			get
			{
				return true;
			}
		}

		public override bool SupportsPause
		{
			get
			{
				return true;
			}
		}

		public override bool SupportsResume
		{
			get
			{
				return true;
			}
		}

		public override bool SupportsStop
		{
			get
			{
				return true;
			}
		}
	}
}
