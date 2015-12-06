namespace Platform.Xml.Serialization
{
	public class XmlSerializeEnumAsIntAttribute
		: XmlAttributeAttribute
	{
		public bool Value { get; set; }

		public XmlSerializeEnumAsIntAttribute()
			: this(true)
		{
		}

		public XmlSerializeEnumAsIntAttribute(bool value)
		{
			this.Value = value;
		}
	}
}
