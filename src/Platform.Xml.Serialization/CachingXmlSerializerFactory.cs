using System;
using System.Collections.Generic;
using Platform.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// An <see cref="XmlSerializerFactory"/> that supports caching of serializers
	/// </summary>
	public class CachingXmlSerializerFactory
		: XmlSerializerFactory
	{
		private IDictionary<Pair<Type, SerializerOptions>, object> cache;
		private IDictionary<Pair<Type, SerializerOptions>, object> cacheForDynamic;

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer{T}()<>"/>
		/// </summary>
		public override XmlSerializer<T> NewXmlSerializer<T>()
		{
			return this.NewXmlSerializer<T>(null);
		}

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer(Type)"/>
		/// </summary>
		public override XmlSerializer<object> NewXmlSerializer(Type type)
		{
			return this.NewXmlSerializer(type, null);
		}

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer(Type, SerializerOptions)"/>
		/// </summary>
		public override XmlSerializer<object> NewXmlSerializer(Type type, SerializerOptions options)
		{
			object value;
			XmlSerializer<object> serializer;

			var key = new Pair<Type, SerializerOptions>(type, options);

			if (this.cacheForDynamic == null)
			{
				this.cacheForDynamic = new Dictionary<Pair<Type, SerializerOptions>, object>();
			}

			if (this.cacheForDynamic.TryGetValue(key, out value))
			{
				return (XmlSerializer<object>)value;
			}

			if (options == null)
			{
				serializer = new XmlSerializer<object>(type);
			}
			else
			{
				serializer = new XmlSerializer<object>(type, options);
			}

			this.cacheForDynamic[key] = serializer;

			return serializer;
		}

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer{T}(SerializerOptions)"/>
		/// </summary>
		public override XmlSerializer<T> NewXmlSerializer<T>(SerializerOptions options)
		{
			object value;
			XmlSerializer<T> serializer;

			var key = new Pair<Type, SerializerOptions>(typeof(T), options);

			if (this.cache == null)
			{
				this.cache = new Dictionary<Pair<Type, SerializerOptions>, object>();
			}

			if (this.cache.TryGetValue(key, out value))
			{
				return (XmlSerializer<T>)value;
			}

			if (options == null)
			{
				serializer = new XmlSerializer<T>();
			}
			else
			{
				serializer = new XmlSerializer<T>(options);
			}

			this.cache[key] = serializer;

			return serializer;
		}
	}
}