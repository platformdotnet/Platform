namespace Platform.Xml.Serialization
{
	public interface IXmlSerializationStartListener
	{
		/// <summary>
		/// Called when the object is about to be serialized.
		/// </summary>
		/// <remarks>
		/// This method is called on an object before its properties have been serilized.
		/// </remarks>
		/// <param name="parameters"></param>
		void XmlSerializationStart(SerializationParameters parameters);
	}
}