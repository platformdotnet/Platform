using System;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Summary description for EnumTypeSerializer.
	/// </summary>
	public class EnumTypeSerializer
		: TypeSerializerWithSimpleTextSupport
	{
		/// <summary>
		/// 
		/// </summary>
		public override Type SupportedType
		{
			get
			{
				return m_Type;
			}
		}

		private Type m_Type;

		public EnumTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{			
			m_Type = memberInfo.LogicalType;

			if (!typeof(Enum).IsAssignableFrom(m_Type))
			{
				throw new ArgumentException(this.GetType().Name + " only works with Enum types");
			}
		}

		public override string Serialize(object obj, SerializationState state)
		{
			return Enum.GetName(m_Type, obj);
		}

		public override object Deserialize(string value, SerializationState state)
		{
			return Enum.Parse(m_Type, value, true);
		}
	}
}
