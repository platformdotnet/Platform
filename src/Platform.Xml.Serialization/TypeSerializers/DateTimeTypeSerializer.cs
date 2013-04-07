using System;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	#region DateTime attributes
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]	
	public class XmlDateTimeFormatAttribute
		: XmlSerializationAttribute
	{		
		/// <summary>
		///
		/// </summary>
		public string Format
		{
			get
			{
				return m_Format;
			}
			
			set
			{
				m_Format = value;
			}
		}
		/// <remarks>
		/// <see cref="Format"/>
		/// </remarks>
		private string m_Format;

		public XmlDateTimeFormatAttribute(string format)
		{
			m_Format = format;
		}
	}
	#endregion

	/// <summary>
	/// 
	/// </summary>
	/// 
	public class DateTimeTypeSerializer
		: TypeSerializerWithSimpleTextSupport
	{
		private bool m_FormatSpecified = false;

		/// <summary>
		/// 
		/// </summary>
		private XmlDateTimeFormatAttribute m_FormatAttribute;

		/// <summary>
		/// 
		/// </summary>
		public override bool MemberBound
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override Type SupportedType
		{
			get
			{
				return typeof(DateTime);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <param name="cache"></param>
		/// <param name="options"></param>
		public DateTimeTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			m_FormatAttribute = (XmlDateTimeFormatAttribute)memberInfo.GetFirstApplicableAttribute(typeof(XmlDateTimeFormatAttribute));
			
			if (m_FormatAttribute == null)
			{				
				m_FormatAttribute = new XmlDateTimeFormatAttribute("G");
				m_FormatSpecified = false;
			}
		}

		/// <summary>
		/// <see cref="TypeSerializerWithSimpleTextSupport.Serialize(object, SerializationState)"/>
		/// </summary>
		public override string Serialize(object obj, SerializationState state)
		{
			return ((DateTime)obj).ToString(m_FormatAttribute.Format);
		}

		/// <summary>
		/// <see cref="TypeSerializerWithSimpleTextSupport.Deserialize(string, SerializationState)"/>
		/// </summary>
		public override object Deserialize(string value, SerializationState state)
		{
			if (m_FormatSpecified)
			{
				try
				{
					// Try parsing using the specified format.

					return DateTime.ParseExact(value, m_FormatAttribute.Format, System.Globalization.CultureInfo.CurrentCulture);
				}
				catch 
				{				
				}
			}

			// Try parsing with the system supplied strategy.

			return DateTime.Parse(value);
		}
	}
}