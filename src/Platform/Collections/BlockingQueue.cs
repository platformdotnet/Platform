using System;
using System.Threading;

namespace Platform.Collections
{
	public class BlockingQueue<T>
		: QueueBase<T>, IBlockingQueue<T>
	{
		public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(-1);

		private readonly IQueue<T> queue;
		private readonly object lockObject = new object();

		public BlockingQueue()
			: this(new ArrayQueue<T>())
		{	
		}

		public BlockingQueue(IQueue<T> backingQueue)
			: this(backingQueue, DefaultTimeout)
		{
		}

		public BlockingQueue(IQueue<T> backingQueue, TimeSpan timeout)
		{
			this.queue = backingQueue;
		}

		public override int Count
		{
			get
			{
				lock (this.lockObject)
				{
					return queue.Count;
				}
			}
		}

		public override void Enqueue(T item)
		{
			lock (this.lockObject)
			{
				this.queue.Enqueue(item);

				Monitor.Pulse(this.lockObject);
			}
		}

		public override void Enqueue(T[] items, int offset, int count)
		{
			lock (this.lockObject)
			{
				this.queue.Enqueue(items, offset, count);

				Monitor.Pulse(this.lockObject);
			}
		}

		public override T Dequeue()
		{
			return Dequeue(Timeout.Infinite);
		}

		public virtual T Dequeue(int timeout)
		{
			return Dequeue(TimeSpan.FromMilliseconds(timeout));
		}

		public virtual T Dequeue(TimeSpan timeout)
		{
			T value;

			if (TryDequeue(timeout, out value))
			{
				return value;
			}
			else
			{
				throw new TimeoutException();
			}
		}

		public override bool TryDequeue(out T value)
		{
			return TryDequeue(DefaultTimeout, out value);
		}

		public override bool TryPeek(out T value)
		{
			lock (this.lockObject)
			{
				if (this.Count == 0)
				{
					value = default(T);

					return false;
				}
				else
				{
					value = this.queue.Peek();

					return true;
				}
			}
		}

		public virtual bool TryDequeue(TimeSpan timeout, out T value)
		{
			lock (this.lockObject)
			{
				while (true)
				{
					if (this.Count == 0)
					{
						if (!Monitor.Wait(this.lockObject, timeout))
						{
							value = default(T);

							return false;
						}
					}
					else
					{
						break;	
					}
				}

				value = this.queue.Dequeue();
			}

			return true;
		}		
	}
}
