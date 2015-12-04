using System;

namespace Platform.Xml.Serialization
{
	public class DateTimeTypeSerializer
		: TypeSerializerWithSimpleTextSupport
	{
		private readonly bool formatSpecified = false;

		private readonly XmlDateTimeFormatAttribute formatAttribute;

		public override bool MemberBound
		{
			get
			{
				return true;
			}
		}

		public override Type SupportedType
		{
			get
			{
				return typeof(DateTime);
			}
		}

		public DateTimeTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			formatAttribute = (XmlDateTimeFormatAttribute)memberInfo.GetFirstApplicableAttribute(typeof(XmlDateTimeFormatAttribute));
			
			if (formatAttribute == null)
			{				
				formatAttribute = new XmlDateTimeFormatAttribute("G");
				formatSpecified = false;
			}
			else
			{
				formatSpecified = true;
 			}
		}

		public override string Serialize(object obj, SerializationContext state)
		{
			return ((DateTime)obj).ToString(formatAttribute.Format, formatAttribute.CultureInfo);
		}

		public override object Deserialize(string value, SerializationContext state)
		{
			if (formatSpecified)
			{
				try
				{
					return DateTime.ParseExact(value, formatAttribute.Format, formatAttribute.CultureInfo);
				}
				catch 
				{				
				}
			}

			return DateTime.Parse(value);
		}
	}
}