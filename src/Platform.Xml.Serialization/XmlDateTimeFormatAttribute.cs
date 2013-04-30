using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]	
	public class XmlDateTimeFormatAttribute
		: XmlSerializationAttribute
	{
		public string Format
		{
			get;
			set;
		}

		public XmlDateTimeFormatAttribute(string format)
		{
			this.Format = format;
		}
	}
}