using System;
using System.Collections.Generic;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// An <see cref="XmlSerializerFactory"/> that supports caching of serializers
	/// </summary>
	public class CachingXmlSerializerFactory
		: XmlSerializerFactory
	{
		private Dictionary<Pair<Type, SerializerOptions>, object> cache = new Dictionary<Pair<Type, SerializerOptions>, object>();
		private Dictionary<Pair<Type, SerializerOptions>, object> cacheForDynamic = new Dictionary<Pair<Type, SerializerOptions>, object>();

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer{T}()"/>
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
			
			var key = new Pair<Type, SerializerOptions>(type, options);

			if (!this.cacheForDynamic.TryGetValue(key, out value))
			{
				if (options == null)
				{
					value = new XmlSerializer<object>(type);
				}
				else
				{
					value = new XmlSerializer<object>(type, options);
				}

				this.cacheForDynamic = new Dictionary<Pair<Type, SerializerOptions>, object>(this.cacheForDynamic) { [key] = value };
			}

			return (XmlSerializer<object>)value;
		}

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer{T}(SerializerOptions)"/>
		/// </summary>
		public override XmlSerializer<T> NewXmlSerializer<T>(SerializerOptions options)
		{
			object value;

			var key = new Pair<Type, SerializerOptions>(typeof(T), options);

			if (!this.cache.TryGetValue(key, out value))
			{
				if (options == null)
				{
					value = new XmlSerializer<object>(typeof(T));
				}
				else
				{
					value = new XmlSerializer<object>(typeof(T), options);
				}

				this.cache = new Dictionary<Pair<Type, SerializerOptions>, object>(this.cache) { [key] = value };
			}

			return (XmlSerializer<T>)value;
		}
	}
}