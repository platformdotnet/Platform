namespace Platform.Xml.Serialization
{
	public interface IXmlSerializationShouldSerializeProvider
	{
		bool ShouldSerialize(SerializerOptions options, SerializationParameters parameters);
	}
}