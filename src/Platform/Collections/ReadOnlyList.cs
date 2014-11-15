using System.Collections;
using System.Collections.Generic;

namespace Platform.Collections
{
	public class ReadOnlyList<T>
		: IReadOnlyList<T>
	{
		private readonly IList<T> innerList;
		public int Count { get { return this.innerList.Count; } }
		public T this[int index] { get { return this.innerList[index]; } }

		public ReadOnlyList(params T[] values)
			: this(new List<T>(values))
		{
		}

		public ReadOnlyList(IEnumerable<T> items)
			: this(items is ReadOnlyList<T> ? (IList<T>)items : new List<T>(items))
		{
		}

		public ReadOnlyList(IList<T> innerList)
		{
			this.innerList = innerList;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.innerList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
