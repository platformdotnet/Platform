namespace Platform.Collections
{
#if NET40
	public interface IReadOnlyList<out T>
		: IReadOnlyCollection<T>
	{
		T this[int index] { get; }
	}
#endif
}
