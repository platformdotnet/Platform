using System;
using System.Drawing;
using System.Xml;
using System.Collections;

namespace Platform.Xml.Serialization
{
    public class StandardTypeSerializerFactory
        : TypeSerializerFactory
    {
        private readonly SerializerOptions options;

        public StandardTypeSerializerFactory(SerializerOptions options)
        {
            this.options = options;
        }

        public override TypeSerializer NewTypeSerializerBySupportedType(Type supportedType, TypeSerializerCache cache)
        {
            return NewTypeSerializerBySupportedType(supportedType, null, cache);
        }

        public override TypeSerializer NewTypeSerializerBySupportedType(Type supportedType, SerializationMemberInfo memberInfo, TypeSerializerCache cache)
        {
            const string error = "A TypeSerializer can't be created for the given type without a memberInfo";

            if (typeof(Enum).IsAssignableFrom(supportedType))
            {
                return new EnumTypeSerializer(memberInfo, cache, options);
            }
            else if (typeof(IDictionary).IsAssignableFrom(supportedType))
            {
                return new DictionaryTypeSerializer(memberInfo, cache, options);
            }
            else if (typeof(Type).IsAssignableFrom(supportedType))
            {
                return new RuntimeTypeTypeSerializer(memberInfo, cache, options);
            }
            else if (supportedType == typeof(XmlNode))
            {
                return XmlNodeNodeTypeSerializer.Default;
            }
            else if (StringableTypeSerializer.SupportedTypes.Contains(supportedType))
            {
                return new StringableTypeSerializer(supportedType, memberInfo, cache, options);
            }
            else if (supportedType == typeof(Guid))
            {
                return new GuidSerializer(supportedType, memberInfo, cache, options);
            }
            else if (supportedType == typeof(Color))
            {
                return new ColorSerializer(supportedType, memberInfo, cache, options);
            }
            else if (supportedType == typeof(DateTime))
            {
                return new DateTimeTypeSerializer(memberInfo, cache, options);
            }
            else if (supportedType.IsGenericType && supportedType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return NewTypeSerializerBySupportedType
                (
                    Nullable.GetUnderlyingType(supportedType),
                    memberInfo,
                    cache
                );
            }
            else
            {
                var implementsList = false;
                var implementsGenericList = false;

                implementsList = typeof(IList).IsAssignableFrom(supportedType);

                implementsGenericList = supportedType.FindInterfaces
                (
                    (type, criterea) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IList<>),
                    null
                ).Length > 0;

                if (implementsList || implementsGenericList)
                {
                    if (memberInfo == null)
                    {
                        throw new XmlSerializerException(error);
                    }

                    return new ListTypeSerializer(memberInfo, cache, options);
                }

                return new ComplexTypeTypeSerializer(memberInfo, supportedType, cache, options);
            }
        }

        public override TypeSerializer NewTypeSerializerBySerializerType(Type serializerType, TypeSerializerCache cache)
        {
            return NewTypeSerializerBySerializerType(serializerType, null, cache);
        }

        public override TypeSerializer NewTypeSerializerBySerializerType(Type serializerType, SerializationMemberInfo memberInfo, TypeSerializerCache cache)
        {
            TypeSerializer retval = null;

            try
            {
                retval = (TypeSerializer)Activator.CreateInstance(serializerType, new object[] { cache, options });
            }
            catch (Exception)
            {
            }

            if (retval == null && memberInfo != null)
            {
                try
                {
                    retval = (TypeSerializer)Activator.CreateInstance(serializerType, new object[] { memberInfo, cache, options });
                }
                catch (Exception)
                {
                }
            }

            if (retval == null && memberInfo != null)
            {
                try
                {
                    retval = (TypeSerializer)Activator.CreateInstance(serializerType, new object[] { memberInfo.ReturnType });
                }
                catch (Exception)
                {
                }
            }

            if (retval == null)
            {
                try
                {
                    retval = (TypeSerializer)Activator.CreateInstance(serializerType, new object[0]);
                }
                catch (Exception)
                {
                }
            }

            if (retval == null)
            {
                throw new XmlSerializerException("Unable to create TypeSerializer: " + serializerType.GetType().ToString());
            }

            return retval;
        }
    }
}
