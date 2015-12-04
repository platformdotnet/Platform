using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// A factory that creates <see cref="XmlSerializer{T}"/> objects.
	/// </summary>
	public abstract class XmlSerializerFactory
	{
		private static readonly XmlSerializerFactory factory = new CachingXmlSerializerFactory();

		/// <summary>
		/// Gets an instance of the default XmlSerializer.
		/// </summary>
		/// <returns></returns>
		public static XmlSerializerFactory Default
		{
			get
			{
				return factory;
			}
		}

		/// <summary>
		/// Creates a new <see cref="XmlSerializer{T}"/>
		/// </summary>
		/// <typeparam name="T">The serializer's supported object type</typeparam>
		/// <returns>A new <see cref="XmlSerializer{T}"/></returns>
		public abstract XmlSerializer<T> NewXmlSerializer<T>();

		/// <summary>
		/// Creates a new <see cref="XmlSerializer{T}"/>
		/// </summary>
		/// <typeparam name="T">The serializer's supported object type</typeparam>
		/// <param name="options">The options for the serializer</param>
		/// <returns>A new <see cref="XmlSerializer{T}"/></returns>
		public abstract XmlSerializer<T> NewXmlSerializer<T>(SerializerOptions options);

		/// <summary>
		/// Creates a new <see cref="XmlSerializer{T}"/>
		/// </summary>
		/// <param name="type">The type supported by the serializer</param>
		/// <returns>A new <see cref="XmlSerializer{T}"/></returns>
		public abstract XmlSerializer<object> NewXmlSerializer(Type type);

        /// <summary>
        /// Creates a new <see cref="XmlSerializer{T}"/>
        /// </summary>
        /// <param name="type">The type supported by the serializer</param>
        /// <param name="options">The options for the serializer</param>
        /// <returns>A new <see cref="XmlSerializer{T}"/></returns>
        public abstract XmlSerializer<object> NewXmlSerializer(Type type, SerializerOptions options);

        /// <summary>
        /// Creates a new <see cref="XmlSerializer{T}"/>
        /// </summary>
        /// <param name="formatting">The formatting for the serializer.Formatting</param>
        /// <returns>A new <see cref="XmlSerializer{T}"/></returns>
        public abstract XmlSerializer<T> NewXmlSerializer<T>(Formatting formatting);

        /// <summary>
        /// Creates a new <see cref="XmlSerializer{T}"/>
        /// </summary>
        /// <param name="type">The type supported by the serializer</param>
        /// <param name="options">The options for the serializer</param>
        /// <param name="formatting">The formatting for the serializer.Formatting</param>
        /// <returns>A new <see cref="XmlSerializer{T}"/></returns>
        public abstract XmlSerializer<object> NewXmlSerializer(Type type, SerializerOptions options, Formatting formatting);
    }
}