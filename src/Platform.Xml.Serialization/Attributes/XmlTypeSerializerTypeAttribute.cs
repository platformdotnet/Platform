using System;
using System.Collections;

namespace Platform.Xml.Serialization
{
	public class XmlTypeSerializerTypeAttribute
		: XmlSerializationAttribute
	{
		/// <summary>
		///
		/// </summary>
		public Type SerializerType
		{
			get
			{
				return m_SerializerType;
			}
			
			set
			{
				m_SerializerType = value;
			}
		}
		/// <remarks>
		/// <see cref="SerializerType"/>
		/// </remarks>
		private Type m_SerializerType;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serializerType"></param>
		public XmlTypeSerializerTypeAttribute(Type serializerType)
		{
			m_SerializerType = serializerType;
		}
	}
}
