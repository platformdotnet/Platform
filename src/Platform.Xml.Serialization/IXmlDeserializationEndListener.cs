namespace Platform.Xml.Serialization
{
	public interface IXmlDeserializationEndListener
	{
		/// <summary>
		/// Called when an object has been deseriazlied.
		/// </summary>
		/// <remarks>
		/// This method is called on an object after its properties have been set.
		/// </remarks>
		/// <param name="parameters"></param>
		void XmlDeserializationEnd(SerializationParameters parameters);
	}
}