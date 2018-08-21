using System;
using System.Collections.Generic;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// A <see cref="TypeSerializer"/> that supports serializing
	/// various primitive types to and from strings.
	/// </summary>
	public class StringableTypeSerializer
		: TypeSerializerWithSimpleTextSupport
	{
        private readonly bool formatSpecified = false;

        private readonly XmlFormatAttribute formatAttribute;

        public override bool MemberBound => true;

		public override Type SupportedType => this.supportedType;

		public static List<Type> SupportedTypes => new List<Type>()
		{
			typeof(bool),
			typeof(byte),
			typeof(char),
			typeof(decimal),
			typeof(double),
			typeof(float),
			typeof(int),
			typeof(long),
			typeof(sbyte),
			typeof(short),
			typeof(string),
			typeof(uint),
			typeof(ulong),
			typeof(ushort)
		};

		private readonly Type supportedType;

        public StringableTypeSerializer(Type type, SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			supportedType = type;
            if (memberInfo != null)
            {
                formatAttribute= (XmlFormatAttribute) memberInfo.GetFirstApplicableAttribute(typeof (XmlFormatAttribute));
            }

            formatSpecified = formatAttribute != null;
		}

		/// <summary>
		/// <see cref="TypeSerializerWithSimpleTextSupport.Serialize(object, SerializationContext)"/>
		/// </summary>
		public override string Serialize(object obj, SerializationContext state)
		{
		    if (obj is IFormattable && formatSpecified)
		    {
                return (obj as IFormattable).ToString(formatAttribute.Format, formatAttribute.CultureInfo);
		    }

			if (obj == null)
			{
				return string.Empty;
			}

			return obj.ToString();
		}

		/// <summary>
		/// <see cref="TypeSerializerWithSimpleTextSupport.Deserialize(string, SerializationContext)"/>
		/// </summary>
		public override object Deserialize(string value, SerializationContext state)
		{
         
            try
            {
                if (formatSpecified)
                {
                    return Convert.ChangeType(value, supportedType, formatAttribute.CultureInfo);
                }

                return Convert.ChangeType(value, supportedType);
            }
            catch(Exception ex)
            {
                var memberInfo = state.GetCurrentMemberInfo().MemberInfo;
                throw new Exception(string.Format("Can't deserialize property {0} of class {1}. Could not convert value '{2}' to type {3}",
                    memberInfo.Name,
                    memberInfo.DeclaringType.FullName,
                    value,
                    supportedType), ex);
            }
		}
	}
}
