using System;
using Platform.IO;

namespace Platform.Collections
{
	public class CircularFifoBuffer<T>
		: BoundedFifoBuffer<T>
	{
		public CircularFifoBuffer(int capacity, T[] buffer = null)
			: base(capacity, buffer)
		{
		}

		public override void Write(T[] buffer, int offset, int length)
		{
			var capacity = this.Capacity;
			var end = (this.position + this.Length) % capacity;

			if (length >= capacity)
			{
				var sourceOffset = offset + length - capacity;

				Array.Copy(buffer, sourceOffset, bytes, 0, capacity);

				this.position = 0;
				this.Length = this.bytes.Length;
			}
			else
			{
				var x = Math.Min(capacity - end, length);
				Array.Copy(buffer, offset, bytes, end, x);

				Array.Copy(buffer, offset + x, bytes, 0, length - x);

				if (this.Length + length > capacity)
				{
					this.position = (this.position + ((this.Length + length) - capacity)) % capacity;
					this.Length = capacity;
				}
				else
				{
					this.Length += length;
				}
			}
		}
	}
	
}
