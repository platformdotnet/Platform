using System;

namespace Platform.Collections
{
	public abstract class QueueBase<T>
		: CollectionBase<T>, IQueue<T>
	{
		public abstract void Enqueue(T item);

		public virtual void Enqueue(T[] items, int offset, int count)
		{
			for (var i = offset; i < offset + count; i++)
			{
				Enqueue(items[i]);
			}
		}

		public virtual T Dequeue()
		{
			T value;

			if (TryDequeue(out value))
			{
				return value;
			}

			throw new InvalidOperationException();
		}

		public virtual T Peek()
		{
			T value;

			if (TryPeek(out value))
			{
				return value;
			}

			throw new InvalidOperationException();
		}

		public abstract bool TryDequeue(out T value);
		public abstract bool TryPeek(out T value);

		public override void Add(T item)
		{
			Enqueue(item);
		}

		public override bool Remove(T item)
		{
			throw new NotSupportedException();
		}		
	}
}
