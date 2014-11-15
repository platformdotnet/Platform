using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Collections
{
	public class CollectionBase<T>
		: ICollection<T>
	{
		public virtual IEnumerator<T> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public virtual void Add(T item)
		{
			throw new NotImplementedException();
		}

		public virtual void Clear()
		{
			throw new NotImplementedException();
		}

		public virtual bool Contains(T item)
		{
			return Enumerable.Contains(this, item);
		}

		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public virtual bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		public virtual int Count { get { throw new NotImplementedException(); } }

		public virtual bool IsReadOnly { get { throw new NotImplementedException(); } }
	}
}
