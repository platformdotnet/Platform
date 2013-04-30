namespace Platform.Xml.Serialization
{
	public interface ISerializationUnhandledMarkupListener
	{
		void UnhandledAttribute(string name, string value);
		void UnhandledOther(string xml);
	}
}
