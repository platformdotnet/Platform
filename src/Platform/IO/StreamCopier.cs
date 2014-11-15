using System;
using System.IO;

namespace Platform.IO
{
	/// <summary>
	/// A class that supports copying from one stream into another stream.
	/// </summary>
	public class StreamCopier
		: AbstractTask
	{
		public static readonly int DefaultBufferSize = 8192 * 8;

		#region Types

		public interface IStreamProvider
		{
			Stream GetSourceStream();
			Stream GetDestinationStream();

			long GetSourceLength();
			long GetDestinationLength();
		}

		public class StaticStreamProvider
			: IStreamProvider
		{
			private readonly Stream source;
			private readonly Stream destination;

			public StaticStreamProvider(Stream source, Stream destination)
			{
				this.source = source;
				this.destination = destination;
			}

			public virtual Stream GetSourceStream()
			{
				return source;
			}

			public virtual Stream GetDestinationStream()
			{
				return destination;
			}

			public virtual long GetSourceLength()
			{
				if (source.CanSeek)
				{
					return source.Length;
				}
				else
				{
					return Int64.MaxValue;
				}
			}

			public virtual long GetDestinationLength()
			{
				if (destination.CanSeek)
				{
					return destination.Length;
				}
				else
				{
					return Int64.MaxValue;
				}
			}
		}

		protected class BytesReadMeter
			: Meter
		{
			public BytesReadMeter(StreamCopier copier)
				: base(copier)
			{
			}

			public override object CurrentValue
			{
				get
				{
					lock (this.copier)
					{
						return this.copier.bytesRead;
					}
				}
			}

			public override object MaximumValue
			{
				get
				{
					lock (this.copier)
					{
						return this.copier.sourceLength;
					}
				}
			}
		}

		protected class BytesWrittenMeter
			: Meter
		{
			public BytesWrittenMeter(StreamCopier copier)
				: base(copier)
			{
			}

			public override object CurrentValue
			{
				get
				{
					lock (this.copier)
					{
						return this.copier.bytesWritten;
					}
				}
			}

			public override object MaximumValue
			{
				get
				{
					lock (this.copier)
					{
						return this.copier.streamProvider.GetSourceLength();
					}
				}
			}
		}

		protected abstract class Meter
			: AbstractMeter
		{
			protected StreamCopier copier;

			protected Meter(StreamCopier copier)
			{
				this.copier = copier;
			}

			public override object Owner
			{
				get
				{
					return this.copier;
				}
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

			public virtual void RaiseValueChanged(long oldValue)
			{
				OnValueChanged(oldValue, this.CurrentValue);
			}

			public virtual void RaiseMajorChange()
			{
				OnValueChanged(this.CurrentValue, this.CurrentValue);
				OnMajorChange();
			}
		}

		#endregion

		private readonly byte[] buffer;
		private long bytesRead = 0;
		private long bytesWritten = 0;
		private long sourceLength = -1;
		private Meter bytesReadMeter;
		private Meter bytesWrittenMeter;
		private readonly bool autoCloseSource;
		private readonly bool autoCloseDestination;
		private IStreamProvider streamProvider;

		protected IStreamProvider StreamProvider
		{
			get
			{
				return streamProvider;
			}
			set
			{
				streamProvider = value;
			}
		}

		public StreamCopier(Stream source, Stream destination)
			: this(source, destination, true, true)
		{
		}

		public StreamCopier(Stream source, Stream destination, int bufferSize)
			: this(source, destination, true, true, bufferSize)
		{
		}

		public StreamCopier(Stream source, Stream destination, bool autoCloseSource, bool autoCloseDestination)
			: this(source, destination, autoCloseSource, autoCloseDestination, -1)
		{
		}

		public StreamCopier(Stream source, Stream destination, bool autoCloseSource, bool autoCloseDestination, int bufferSize)
			: this(new StaticStreamProvider(source, destination), autoCloseSource, autoCloseDestination, bufferSize)
		{	
		}

		public StreamCopier(IStreamProvider streamProvider, bool autoCloseSource, bool autoCloseDestination)
			: this(streamProvider, autoCloseSource, autoCloseDestination, -1)
		{	
		}

		public StreamCopier(IStreamProvider streamProvider, bool autoCloseSource, bool autoCloseDestination, int bufferSize)
		{
			this.streamProvider = streamProvider;

			this.autoCloseSource = autoCloseSource;
			this.autoCloseDestination = autoCloseDestination;
			buffer = new byte[bufferSize > 0 ? bufferSize : DefaultBufferSize];

			bytesReadMeter = new BytesReadMeter(this);
			bytesWrittenMeter = new BytesReadMeter(this);

			sourceLength = this.streamProvider.GetSourceLength();
		}

		protected StreamCopier(bool autoCloseSource, bool autoCloseDestination, int bufferSize)
		{
			streamProvider = (IStreamProvider)this;

			this.autoCloseSource = autoCloseSource;
			this.autoCloseDestination = autoCloseDestination;
			buffer = new byte[bufferSize > 0 ? bufferSize : DefaultBufferSize];
		}

		protected virtual void InitializePump()
		{
			bytesReadMeter = new BytesReadMeter(this);
			bytesWrittenMeter = new BytesReadMeter(this);

			sourceLength = streamProvider.GetSourceLength();
		}

		public override IMeter Progress
		{
			get
			{
				return bytesWrittenMeter;
			}
		}

		public virtual IMeter WriteProgress
		{
			get
			{
				return bytesWrittenMeter;
			}
		}

		public virtual IMeter ReadProgress
		{
			get
			{
				return bytesReadMeter;
			}
		}

		public override void DoRun()
		{
			var finished = false;

			ProcessTaskStateRequest();

			var source = this.streamProvider.GetSourceStream();
			var destination = this.streamProvider.GetDestinationStream();

			try
			{
				try
				{
					bytesReadMeter.RaiseValueChanged(0L);

					ProcessTaskStateRequest();

					bytesWrittenMeter.RaiseValueChanged(0L);

					ProcessTaskStateRequest();

					while (true)
					{
						var read = source.Read(buffer, 0, buffer.Length);

						if (read == 0)
						{
							if (sourceLength != bytesRead)
							{
								sourceLength = bytesRead;
								bytesReadMeter.RaiseMajorChange();
								bytesWrittenMeter.RaiseMajorChange();
							}

							break;
						}

						lock (this)
						{
							bytesRead += read;
						}

						if (bytesRead > sourceLength)
						{
							sourceLength = bytesRead;
						}

						bytesReadMeter.RaiseValueChanged(bytesRead - read);

						ProcessTaskStateRequest();

						destination.Write(buffer, 0, read);

						lock (this)
						{
							bytesWritten += read;
						}

						bytesWrittenMeter.RaiseValueChanged(bytesWritten - read);

						ProcessTaskStateRequest();
					}

					finished = true;
				}
				catch (StopRequestedException)
				{
					SetTaskState(TaskState.Stopped);
				}
			}
			finally
			{
				ActionUtils.IgnoreExceptions(destination.Flush);					

				if (autoCloseSource)
				{
					ActionUtils.IgnoreExceptions(source.Close);
				}

				if (autoCloseDestination)
				{
					ActionUtils.IgnoreExceptions(destination.Close);
				}

				if (finished)
				{
					SetTaskState(TaskState.Finished);
				}
				else
				{
					SetTaskState(TaskState.Stopped);
				}
			}
		}

		public static void Copy(Stream inStream, Stream outStream)
		{
			new StreamCopier(inStream, outStream, true, true).Run();
		}
	}
}
