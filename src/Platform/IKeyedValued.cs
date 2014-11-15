namespace Platform
{
	public interface IKeyedValued
		: IKeyed, IValued
	{
	}

	public interface IKeyedValued<K, V>
		: IKeyed<K>, IValued<V>
	{
	}
}
