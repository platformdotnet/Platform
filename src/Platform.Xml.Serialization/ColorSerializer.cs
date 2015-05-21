using System;
using System.Drawing;

namespace Platform.Xml.Serialization
{
    public class ColorSerializer
        : StringableTypeSerializer
    {
        public ColorSerializer(Type type, SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
            : base(type, memberInfo, cache,options)
        {
        }

        public override string Serialize(object obj, SerializationContext state)
        {
            if (obj == null)
            {
                return "";
            }
				
            if (((Color)obj).ToKnownColor() != 0)
            {
                return ((Color)obj).Name;
            }
            else
            {
                return ColorTranslator.ToHtml((Color)obj);
            }
        }

        /// <summary>
        /// <see cref="TypeSerializerWithSimpleTextSupport.Deserialize(string, SerializationContext)"/>
        /// </summary>
        public override object Deserialize(string value, SerializationContext state)
        {
            try
            {
                return ColorTranslator.FromHtml(value);
            }
            catch (Exception)
            {
                return Color.FromName(value);
            }
        }
    }
}