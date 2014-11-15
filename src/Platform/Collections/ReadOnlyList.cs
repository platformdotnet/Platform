using System.Collections;
using System.Collections.Generic;

namespace Platform.Collections
{
#if NET40
	public class ReadOnlyList<T>
		: IReadOnlyList<T>
	{
		private readonly IList<T> innerList;
		public int Count { get { return this.innerList.Count; } }
		public T this[int index] { get { return this.innerList[index]; } }

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
#endif
}
