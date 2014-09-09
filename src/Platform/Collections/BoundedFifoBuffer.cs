using System;
using System.Collections;
using System.Collections.Generic;

namespace Platform.Collections
{
	public class BoundedFifoBuffer<T>
		: IList<T>
	{
		protected int position;
		protected readonly T[] bytes;
		
		public int Length { get; protected set; }
		public int Capacity { get { return this.bytes.Length; } }
		public int Spare { get { return this.Capacity - this.Length; } }
		public virtual bool DataIsAvailable { get { return this.Length > 0; } }

		public BoundedFifoBuffer(int capacity, T[] buffer = null)
		{
			if (capacity <= 0)
			{
				throw new ArgumentOutOfRangeException("capacity");
			}

			this.bytes = new T[capacity];

			if (buffer != null)
			{
				this.Write(buffer);
			}
		}

		public virtual void Write(T[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		public virtual void Write(T[] buffer, int offset, int length)
		{
			var capacity = this.Capacity;
			var start = this.position;
			var end = (this.position + this.Length) % capacity;

			if (this.Length + length > capacity)
			{
				throw new BufferOverflowException();
			}

			if (start > end)
			{
				Array.Copy(buffer, offset, this.bytes, end, length);

				this.Length += length;
			}
			else
			{
				var y = 0;
				var x = Math.Min(capacity - end, length);

				Array.Copy(buffer, offset, this.bytes, end, x);

				if (x < length)
				{
					y = length - x;
					Array.Copy(buffer, offset + x, this.bytes, 0, y);
				}

				this.Length += x + y;
			}
		}

		public virtual int Read(T[] output, int offset, int length)
		{
			return this.Read(output, offset, length, false);
		}

		public virtual int Peek(T[] output, int offset, int length)
		{
			return this.Read(output, offset, length, true);
		}

		private int Read(T[] output, int offset, int length, bool peek)
		{
			if (output.Length < offset + length)
			{
				throw new ArgumentException("The output array is not large enough", "output");
			}

			if (this.Length == 0)
			{
				return 0;
			}
			else
			{
				var capacity = this.Capacity;
				var start = this.position;
				var end = (this.position + this.Length) % capacity;

				if (start >= end)
				{
					var y = 0;
					var x = Math.Min(capacity - start, length);

					Array.Copy(this.bytes, start, output, offset, x);

					if (x < length)
					{
						y = Math.Min(end, length - x);

						Array.Copy(this.bytes, 0, output, offset + x, y);
					}

					if (!peek)
					{
						this.Length -= x + y;
						this.position = (this.position + x + y) % capacity;
					}

					return x + y;
				}
				else
				{
					var x = Math.Min(end - start, length);

					Array.Copy(this.bytes, start, output, offset, x);

					if (!peek)
					{
						this.Length -= x;
						this.position = (this.position + x) % capacity;
					}

					return x;
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (this.Length == 0)
			{
				yield break;
			}
			else
			{
				var capacity = this.Capacity;
				var start = this.position;
				var end = (this.position + this.Length) % capacity;

				if (start >= end)
				{
					var y = 0;
					var x = capacity - start;

					for (var i = start; i < start + x; i++)
					{
						yield return this.bytes[i];
					}

					y = end;

					for (var i = 0; i < y; i++)
					{
						yield return this.bytes[i];
					}
				}
				else
				{
					for (var i = start; i < end; i++)
					{
						yield return this.bytes[i];
					}
				}
			}
		}

		public virtual T Read()
		{
			return this.Read(false);
		}

		public virtual T Peek()
		{
			return this.Read(true);
		}

		private T Read(bool peek)
		{
			if (this.Length == 0)
			{
				throw new BufferUnderflowException();
			}

			var capacity = this.Capacity;
			var start = this.position;
			var end = (this.position + this.Length) % capacity;

			if (start >= end)
			{
				T retval;

				if (capacity - start > 0)
				{
					retval = this.bytes[start];
				}
				else
				{
					retval = this.bytes[0];
				}

				if (!peek)
				{
					this.Length--;
					this.position = (this.position + 1) % capacity;
				}

				return retval;
			}
			else
			{
				var retval = this.bytes[start];

				if (!peek)
				{
					this.Length--;
					this.position = (this.position + 1) + capacity;
				}

				return retval;
			}
		}

		public virtual T[] ReadToArray()
		{
			var retval = new T[this.Length];

			this.Read(retval, 0, this.Length);

			return retval;
		}

		public virtual T[] PeekToArray()
		{
			var retval = new T[this.Length];

			this.Peek(retval, 0, this.Length);

			return retval;
		}

		public void Add(T item)
		{
			this.Write(new [] { item });
		}

		public void Clear()
		{
			this.Length = 0;
			this.position = 0;
		}

		public bool Contains(T item)
		{
			return this.IndexOf(item) >= 0;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			this.Read(array, arrayIndex, this.Length);
		}

		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		public int Count { get { return this.Length; }}
		public bool IsReadOnly { get { return false; } }

		public int IndexOf(T item)
		{
			var i = 0;

			foreach (var x in this)
			{
				if (EqualityComparer<T>.Default.Equals(item, x))
				{
					return i;
				}

				i++;
			}

			return -1;
		}

		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public T this[int index]
		{
			get
			{
				if (index >= this.Length)
				{
					throw new ArgumentOutOfRangeException("index");
				}

				index = (this.position + index) % this.bytes.Length;

				return this.bytes[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
