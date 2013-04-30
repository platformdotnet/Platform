namespace Platform.Xml.Serialization
{
	public interface IXmlDeserializationStartListener
	{
		/// <summary>
		/// Called when an object is about to be deserialized.
		/// </summary>
		/// <remarks>
		/// This method is called on an object after it has been constructed but
		/// before its properties have been set.
		/// </remarks>
		/// <param name="parameters"></param>
		void XmlDeserializationStart(SerializationParameters parameters);
	}
}