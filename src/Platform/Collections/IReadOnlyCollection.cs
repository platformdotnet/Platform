using System.Collections.Generic;

namespace Platform.Collections
{
	public interface IReadOnlyCollection<out T>
		: IEnumerable<T>
	{
		int Count { get; }
	}
}
