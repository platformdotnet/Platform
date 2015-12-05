using System;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Stores pre calculated information about a member or type that is used when serializing that member.
	/// </summary>
	public class SerializationMemberInfo
		: IGetterSetter
	{
		protected MemberInfo memberInfo;
		protected Type returnType = null;
		protected IGetterSetter getterSetter;
		protected TypeSerializer typeSerializer = null;
		protected bool serializeAsCData = false;
		protected TypeSerializerCache typeSerializerCache;
		protected string serializeAsValueNodeAttributeName;
		protected bool treatAsNullIfEmpty = false;
		protected string serializedName = "";
		protected string serializedNamespace = "";
	    protected bool serializeIfNull = false;
		protected XmlSerializationAttribute[] applicableTypeAttributes;
		protected XmlSerializationAttribute[] applicableMemberAttributes;
		protected IXmlDynamicTypeProvider polymorphicTypeProvider;
		protected XmlNodeType serializedNodeType = XmlNodeType.None;

		public IVariableSubstitutor Substitutor { get; private set; }

		public virtual XmlApproachAttribute ApproachAttribute { get; set; }

		public virtual Type ReturnType
		{
			get { return returnType; }
		}

		public virtual bool TreatAsNullIfEmpty
		{
			get { return treatAsNullIfEmpty; }
		}

		public virtual string SerializeAsValueNodeAttributeName
		{
			get { return serializeAsValueNodeAttributeName; }
		}

		public virtual bool SerializeAsCData
		{
			get { return serializeAsCData; }
		}

	    public virtual bool SerializeIfNull
	    {
	        get { return serializeIfNull; }
	    }
		public virtual MemberInfo MemberInfo
		{
			get { return memberInfo; }
		}

		public virtual XmlNodeType SerializedNodeType
		{
			get { return serializedNodeType; }
		}

		public virtual string SerializedName
		{
			get { return serializedName; }
		}

		public virtual string Namespace
		{
			get { return serializedNamespace; }
		}

		public virtual bool Serializable
		{
			get { return serializedNodeType != XmlNodeType.None; }
		}

		public virtual IXmlDynamicTypeProvider PolymorphicTypeProvider
		{
			get { return polymorphicTypeProvider; }
		}

		public SerializationMemberInfo(MemberInfo memberInfo, SerializerOptions options, TypeSerializerCache cache)
			: this(memberInfo, options, cache, false)
		{
		}

		public virtual bool IncludeIfUnattributed
		{
			get { return includeIfUnattributed; }
			set { includeIfUnattributed = value; }
		}

		private bool includeIfUnattributed;

		public SerializationMemberInfo(MemberInfo memberInfo, SerializerOptions options, TypeSerializerCache cache, bool includeIfUnattributed)
		{
			typeSerializerCache = cache;
			this.memberInfo = memberInfo;

			this.includeIfUnattributed = includeIfUnattributed;

			Scan(options, includeIfUnattributed);
		}

		/// <summary>
		/// Prescans the type.
		/// </summary>
		protected virtual void Scan(SerializerOptions options, bool includeIfUnattributed)
		{
			XmlApproachAttribute approach = null;
			XmlElementAttribute elementAttribute;

			// Get the applicable attributes

			LoadAttributes(options);

			// Get the setter/getter and serializer

			if (memberInfo is FieldInfo)
			{
				getterSetter = new FieldGetterSetter(memberInfo);

				returnType = ((FieldInfo) memberInfo).FieldType;
			}
			else if (memberInfo is PropertyInfo)
			{
				var propertyInfo = (PropertyInfo) memberInfo;

				getterSetter = new PropertyGetterSetter(memberInfo);

				returnType = ((PropertyInfo) memberInfo).PropertyType;
			}
			else if (memberInfo is Type)
			{
				getterSetter = null;

				serializedName = memberInfo.Name;
				returnType = (Type) memberInfo;
			}
			else
			{
				throw new ArgumentException($"Unsupported member type: {this.memberInfo.MemberType.ToString()}");
			}

			// Get the [XmlExclude] [XmlAttribute] or [XmlElement] attribute

			var attribute = GetFirstApplicableAttribute(false, typeof (XmlExcludeAttribute),typeof(XmlTextAttribute), typeof (XmlAttributeAttribute), typeof (XmlElementAttribute));

			if (attribute != null)
			{
				if (attribute is XmlExcludeAttribute)
				{
					// This member needs to be excluded

					serializedNodeType = XmlNodeType.None;
				}
                else if (attribute is XmlTextAttribute)
                {
                    serializedNodeType = XmlNodeType.Text;
                }
				else if ((approach = attribute as XmlApproachAttribute) != null)
				{
					ApproachAttribute = approach;

					// This member needs to be included as an attribute or an element

					serializedNodeType = approach is XmlElementAttribute ? XmlNodeType.Element : XmlNodeType.Attribute;

					if (approach.Type != null)
					{
						returnType = approach.Type;
					}

					serializedName = approach.Name;
					serializedNamespace = approach.Namespace;

					if ((elementAttribute = approach as XmlElementAttribute) != null)
					{
						if (elementAttribute.SerializeAsValueNode)
						{
							serializeAsValueNodeAttributeName = elementAttribute.ValueNodeAttributeName;
						}
					}

					if (approach.SerializeUnattribted)
					{
						this.includeIfUnattributed = true;
					}

				    if (approach.SerializeIfNull)
				        this.serializeIfNull = true;
				}
			}
			else
			{
				if (includeIfUnattributed)
				{
					serializedName = memberInfo.Name;

					serializedNodeType = XmlNodeType.Element;
				}
				else
				{
					serializedNodeType = XmlNodeType.None;
				}
			}

			if (serializedNodeType == XmlNodeType.None)
			{
				return;
			}

			// Check if the member should be serialized as CDATA

			attribute = GetFirstApplicableAttribute(typeof (XmlCDataAttribute));

			if (attribute != null)
			{
				serializeAsCData = ((XmlCDataAttribute) attribute).Enabled;
			}

			attribute = GetFirstApplicableAttribute(typeof (XmlVariableSubstitutionAttribute));

			if (attribute != null)
			{
				Substitutor = (IVariableSubstitutor) Activator.CreateInstance(((XmlVariableSubstitutionAttribute) attribute).SubstitutorType);
			}

			// Set the serialized (element or attribute) name to the name of the member if it hasn't already been set

			if (serializedName.Length == 0)
			{
				if (approach != null && approach.UseNameFromAttributedType && memberInfo.MemberType == MemberTypes.TypeInfo)
				{
					serializedName = GetAttributeDeclaringType((Type) memberInfo, approach).Name;
				}
				else
				{
					serializedName = this.memberInfo.Name.Left(PredicateUtils.ObjectEquals('`').Not());
				}
			}

			// Make the serialized (element or attribute) name lowercase if requested

			if (approach != null)
			{
				if (approach.MakeNameLowercase)
				{
					serializedName = serializedName.ToLower();
				}
			}

			// Get the explicitly specified TypeSerializer if requested

			attribute = GetFirstApplicableAttribute(typeof (XmlTypeSerializerTypeAttribute));

			if (attribute != null)
			{
				if (((XmlTypeSerializerTypeAttribute) attribute).SerializerType != null)
				{
					typeSerializer = typeSerializerCache.GetTypeSerializerBySerializerType(((XmlTypeSerializerTypeAttribute) attribute).SerializerType, this);

					if (!returnType.IsAssignableFrom(typeSerializer.SupportedType))
					{
						throw new InvalidOperationException($"Explicitly specified serializer ({((XmlTypeSerializerTypeAttribute)attribute).SerializerType.Name}) doesn't support serializing of associated program element.");
					}
				}
			}
			else
			{
				typeSerializer = typeSerializerCache.GetTypeSerializerBySupportedType(returnType, this);
			}

			// Check if the member should be treated as a null value if it is empty

			treatAsNullIfEmpty = HasApplicableAttribute(typeof (XmlTreatAsNullIfEmptyAttribute));

			// Check if the member's declared type is polymorphic

			var polymorphicTypeAttribute = (XmlPolymorphicTypeAttribute) GetFirstApplicableAttribute(typeof (XmlPolymorphicTypeAttribute));

			if (polymorphicTypeAttribute != null)
			{
				polymorphicTypeProvider = (IXmlDynamicTypeProvider) Activator.CreateInstance(polymorphicTypeAttribute.PolymorphicTypeProvider);
			}
		}

		public virtual Type GetReturnType(object obj)
		{
			if (polymorphicTypeProvider == null)
			{
				return returnType;
			}

			return polymorphicTypeProvider.GetType(obj);
		}

		public virtual Type GetReturnType(XmlReader reader)
		{
			if (polymorphicTypeProvider == null)
			{
				return returnType;
			}

			return polymorphicTypeProvider.GetType(reader);
		}

		public virtual TypeSerializer GetSerializer(object obj)
		{
			if (polymorphicTypeProvider == null || obj == null)
			{
				return typeSerializer;
			}

			return typeSerializerCache.GetTypeSerializerBySupportedType(polymorphicTypeProvider.GetType(obj), this);
		}

		public virtual TypeSerializer GetSerializer(XmlReader reader)
		{
			Type type;

			if (polymorphicTypeProvider == null)
			{
				return typeSerializer;
			}

			if ((type = polymorphicTypeProvider.GetType(reader)) == null)
			{
				return typeSerializer;
			}

			return typeSerializerCache.GetTypeSerializerBySupportedType(type, this);
		}

		private static Type GetDeclaredType(MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo)
			{
				return ((FieldInfo) memberInfo).FieldType;
			}
			else if (memberInfo is MethodInfo)
			{
				return ((MethodInfo) memberInfo).ReturnType;
			}
			else if (memberInfo is PropertyInfo)
			{
				return ((PropertyInfo) memberInfo).PropertyType;
			}
			else if (memberInfo is EventInfo)
			{
				return ((EventInfo) memberInfo).EventHandlerType;
			}
			else
			{
				return null;
			}
		}

		private static Type GetAttributeDeclaringType(Type type, Attribute attribute)
		{
			while (true)
			{
				var attributes = type.GetCustomAttributes(attribute.GetType(), false);

				foreach (Attribute a in attributes)
				{
					if (a.Equals(attribute))
					{
						return type;
					}
				}

				type = type.BaseType;

				if (type == null)
				{
					break;
				}
			}

			return null;
		}

		private static object[] GetCustomAttributes(MemberInfo memberInfo, Type type, bool inherit)
		{
			if (memberInfo.MemberType == MemberTypes.Property && inherit)
			{
				var list = new ArrayList();

				var propertyInfo = (PropertyInfo) memberInfo;

				while (true)
				{
					// LAMESPEC: Why the hell isn't AddRange part of ArrayList and not IList?

					list.AddRange(propertyInfo.GetCustomAttributes(type, false));

					if (propertyInfo.DeclaringType.BaseType == null)
					{
						break;
					}

					//if (propertyInfo.GetGetMethod().IsVirtual || propertyInfo.GetSetMethod().IsVirtual)
					{
						propertyInfo = propertyInfo.DeclaringType.BaseType.GetProperty(propertyInfo.Name);

						if (propertyInfo == null)
						{
							break;
						}
					}
				}

				return list.ToArray();
			}
			else
			{
				return memberInfo.GetCustomAttributes(type, inherit);
			}
		}

		public virtual XmlSerializationAttribute[] GetApplicableAttributes(bool includeTypeAttributes, Type[] types)
		{
			return GetApplicableAttributes(includeTypeAttributes, false, types);
		}

		public virtual System.Collections.Generic.IEnumerable<Attribute> WalkApplicableAttributes(bool includeTypeAttributes, bool includeAttributeSubclasses, Type[] types)
		{
			ArrayList list = new ArrayList();

			foreach (XmlSerializationAttribute attribute in applicableMemberAttributes)
			{
				foreach (Type type in types)
				{
					if (type == attribute.GetType())
					{
						yield return attribute;
					}
					else if (includeAttributeSubclasses && type.IsInstanceOfType(attribute))
					{
						yield return attribute;
					}
				}
			}

			if (includeTypeAttributes && applicableTypeAttributes != applicableMemberAttributes)
			{
				foreach (XmlSerializationAttribute attribute in applicableTypeAttributes)
				{
					foreach (Type type in types)
					{
						if (type == attribute.GetType())
						{
							yield return attribute;
						}
						else if (includeAttributeSubclasses && type.IsInstanceOfType(attribute))
						{
							yield return attribute;
						}
					}
				}
			}
		}

		public virtual XmlSerializationAttribute[] GetApplicableAttributes(bool includeTypeAttributes, bool includeAttributeSubclasses, Type[] types)
		{
			ArrayList list = new ArrayList();

			foreach (XmlSerializationAttribute attribute in applicableMemberAttributes)
			{
				foreach (Type type in types)
				{
					if (type == attribute.GetType())
					{
						list.Add(attribute);
					}
					else if (includeAttributeSubclasses && type.IsInstanceOfType(attribute))
					{
						list.Add(attribute);
					}
				}
			}

			if (includeTypeAttributes && applicableTypeAttributes != applicableMemberAttributes)
			{
				foreach (XmlSerializationAttribute attribute in applicableTypeAttributes)
				{
					foreach (Type type in types)
					{
						if (type == attribute.GetType())
						{
							list.Add(attribute);
						}
						else if (includeAttributeSubclasses && type.IsInstanceOfType(attribute))
						{
							list.Add(attribute);
						}
					}
				}
			}

			return (XmlSerializationAttribute[]) list.ToArray(typeof (XmlSerializationAttribute));
		}

		public virtual XmlSerializationAttribute[] GetApplicableAttributes(Type[] types)
		{
			return GetApplicableAttributes(true, types);
		}

		public virtual XmlSerializationAttribute[] GetApplicableAttributes(bool includeTypeAttributes, Type type)
		{
			return GetApplicableAttributes(includeTypeAttributes, new Type[] {type});
		}

		public virtual XmlSerializationAttribute[] GetApplicableAttributes(Type type)
		{
			return GetApplicableAttributes(true, new Type[] {type});
		}

		private static XmlSerializationAttribute[] ExtractApplicableAttributes(object[] attributes, SerializerOptions options)
		{
			ArrayList list = new ArrayList();

			foreach (XmlSerializationAttribute attribute in attributes)
			{
				if (attribute.Applies(options))
				{
					list.Add(attribute);
				}
			}

			return (XmlSerializationAttribute[]) list.ToArray(typeof (XmlSerializationAttribute));
		}

		private void LoadAttributes(SerializerOptions options)
		{
			applicableMemberAttributes = ExtractApplicableAttributes(GetCustomAttributes(memberInfo, typeof (XmlSerializationAttribute), true), options);

			if (memberInfo is Type)
			{
				applicableTypeAttributes = applicableMemberAttributes;
			}
			else
			{
				applicableTypeAttributes = ExtractApplicableAttributes(GetCustomAttributes(GetDeclaredType(memberInfo), typeof (XmlSerializationAttribute), true), options);
			}
		}

		public XmlSerializationAttribute GetFirstApplicableAttribute(bool includeTypeAttributes, params Type[] types)
		{
			XmlSerializationAttribute[] attributes;

			attributes = GetApplicableAttributes(includeTypeAttributes, types);

			if (attributes.Length == 0)
			{
				return null;
			}
			else
			{
				return attributes[0];
			}
		}

		public XmlSerializationAttribute GetFirstApplicableAttribute(bool includeTypeAttributes, Type type)
		{
			return GetFirstApplicableAttribute(includeTypeAttributes, new Type[] {type});
		}

		public XmlSerializationAttribute GetFirstApplicableAttribute(Type type)
		{
			return GetFirstApplicableAttribute(true, type);
		}

		public XmlSerializationAttribute GetFirstApplicableAttribute(Type[] types)
		{
			return GetFirstApplicableAttribute(true, types);
		}

		public bool HasApplicableAttribute(Type attributeType)
		{
			return GetFirstApplicableAttribute(attributeType) != null;
		}

		public virtual object GetValue(object obj)
		{
			return getterSetter.GetValue(obj);
		}

		public virtual void SetValue(object obj, object val)
		{
			getterSetter.SetValue(obj, val);
		}

		public override int GetHashCode()
		{
			return memberInfo.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			var localSerializationMemberInfo = obj as SerializationMemberInfo;

			if (localSerializationMemberInfo == null)
			{
				return false;
			}

			return this.memberInfo == localSerializationMemberInfo.memberInfo;
		}
	}
}
