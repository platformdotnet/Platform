using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Platform.Collections
{
	public class ReadOnlyCollectionReadOnlyListAdapter<T>
		: IReadOnlyList<T>
	{
		private readonly ReadOnlyCollection<T> inner;
		public int Count { get { return this.inner.Count; } }
		public T this[int value] { get { return this.inner[value]; } }

		public ReadOnlyCollectionReadOnlyListAdapter(ReadOnlyCollection<T> inner)
		{
			this.inner = inner;
		}

		public virtual IEnumerator<T> GetEnumerator()
		{
			return this.inner.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
