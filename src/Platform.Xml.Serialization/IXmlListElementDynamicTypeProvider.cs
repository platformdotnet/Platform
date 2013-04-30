namespace Platform.Xml.Serialization
{
	public interface IXmlListElementDynamicTypeProvider
		: IXmlDynamicTypeProvider
	{
		string GetName(object instance);
	}
}