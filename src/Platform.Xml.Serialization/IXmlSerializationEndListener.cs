namespace Platform.Xml.Serialization
{
	public interface IXmlSerializationEndListener
	{	
		/// <summary>
		/// Called when an object has been serialized.
		/// </summary>
		/// <remarks>
		/// This method is called on an object after its properties have been serialized.
		/// </remarks>
		/// <param name="parameters"></param>
		void XmlSerializationEnd(SerializationParameters parameters);
	}
}