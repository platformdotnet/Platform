using System.Collections.Generic;

namespace Platform.Collections
{
#if NET40
	public interface IReadOnlyCollection<out T>
		: IEnumerable<T>
	{
		int Count { get; }
	}
#endif
}
