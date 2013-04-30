using System;

namespace Platform.Xml.Serialization
{
	public class XmlTypeSerializerTypeAttribute
		: XmlSerializationAttribute
	{
		public Type SerializerType
		{
			get;
			set;
		}

		public XmlTypeSerializerTypeAttribute(Type serializerType)
		{
			SerializerType = serializerType;
		}
	}
}
