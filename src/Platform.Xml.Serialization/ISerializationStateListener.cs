using System;
using System.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Interface for objects that want to know when they're being serialized by
	/// an XmlSerializer.
	/// </summary>
	public interface ISerializationStateListener
	{
		/// <summary>
		/// Called when the object is about to be serialized.
		/// </summary>
		/// <remarks>
		/// This method is called on an object before its properties have been serilized.
		/// </remarks>
		/// <param name="parameters"></param>
		void SerializationStart(SerializationParameters parameters);

		/// <summary>
		/// Called when an object has been serialized.
		/// </summary>
		/// <remarks>
		/// This method is called on an object after its properties have been serialized.
		/// </remarks>
		/// <param name="parameters"></param>
		void SerializationEnd(SerializationParameters parameters);

		/// <summary>
		/// Called when an object is about to be deserialized.
		/// </summary>
		/// <remarks>
		/// This method is called on an object after it has been constructed but
		/// before its properties have been set.
		/// </remarks>
		/// <param name="parameters"></param>
		void DeserializationStart(SerializationParameters parameters);

		/// <summary>
		/// Called when an object has been deseriazlied.
		/// </summary>
		/// <remarks>
		/// This method is called on an object after its properties have been set.
		/// </remarks>
		/// <param name="parameters"></param>
		void DeserializationEnd(SerializationParameters parameters);
	}
}