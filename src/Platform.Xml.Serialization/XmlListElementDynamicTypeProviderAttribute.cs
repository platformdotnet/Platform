using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlListElementDynamicTypeProviderAttribute
		: XmlSerializationAttribute
	{
		public Type ProviderType
		{
			get;
			set;
		}

		public XmlListElementDynamicTypeProviderAttribute(Type providerType)
		{
			this.ProviderType = providerType;
		}
	}
}