namespace Platform.Xml.Serialization
{
	public interface IXmlSerializerSerializable
	{
		XmlSerializer<object> GetXmlSerializer();
	}

	public interface IXmlSerializerSerializable<T>
		: IXmlSerializerSerializable
		where T : new()
	{
		new XmlSerializer<T> GetXmlSerializer();
	}
}
