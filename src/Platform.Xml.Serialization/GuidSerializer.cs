using System;

namespace Platform.Xml.Serialization
{
    public class GuidSerializer
        : StringableTypeSerializer
    {
        public GuidSerializer(Type type, SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
            : base(type, memberInfo,cache,options)
        {
        }

        /// <summary>
        /// <see cref="TypeSerializerWithSimpleTextSupport.Deserialize(string, SerializationContext)"/>
        /// </summary>
        public override object Deserialize(string value, SerializationContext state)
        {
            return new Guid(value);
        }
    }
}