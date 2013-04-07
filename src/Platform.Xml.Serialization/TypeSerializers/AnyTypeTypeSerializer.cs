using System;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// 
	/// </summary>
	public class AnyTypeTypeSerializer
		: TypeSerializer
	{
		private sealed class LightSerializationMember
		{
			public MemberGetter Getter;
			public MemberSetter Setter;
			public TypeSerializer Serializer;
			public bool SerializeAsCData;
			public MemberInfo MemberInfo;
			public string SerializedName;
			public bool XmlTreatAsNullIfEmpty;
			public Type LogicalType;

			public LightSerializationMember(SerializationMemberInfo memberInfo)
			{
				this.Getter = memberInfo.Getter;
				this.Setter = memberInfo.Setter;
				this.Serializer = memberInfo.Serializer;
				this.MemberInfo = memberInfo.MemberInfo;				
				this.SerializedName = memberInfo.GetSerializedName();
				this.SerializeAsCData = memberInfo.SerializeAsCData;
				this.XmlTreatAsNullIfEmpty = memberInfo.HasApplicableAttribute(typeof(XmlTreatAsNullIfEmptyAttribute));
				this.LogicalType = memberInfo.LogicalType;
			}

			public object GetValue(object obj)
			{
				return Getter(MemberInfo, obj);
			}

			public void SetValue(object obj, object value)
			{
				Setter(MemberInfo, obj, value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected IDictionary m_ElementMembersMap;	

		/// <summary>
		/// 
		/// </summary>
		protected IDictionary m_AttributeMembersMap;

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="cache"></param>
		/// <param name="options"></param>
		public AnyTypeTypeSerializer(Type type, TypeSerializerCache cache, SerializerOptions options)
		{
			m_Type = type;

			m_ElementMembersMap = new SortedList(0x10);
			m_AttributeMembersMap = new SortedList(0x10);

			cache.Add(this);

			Scan(cache, options);			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="options"></param>
		private void Scan(TypeSerializerCache cache, SerializerOptions options)
		{
			Type type;
			FieldInfo[] fields;
			PropertyInfo[] properties;

			type = m_Type;
						
			while (type != typeof(object) && type != null)
			{
				fields = m_Type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				properties = m_Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

				foreach (FieldInfo field in fields)
				{
					AddMember(field, cache, options);
				}

				foreach (PropertyInfo property in properties)
				{
					AddMember(property, cache, options);
				}
				
				object[] attribs;
				bool serializeBase = true;
				
				attribs = type.GetCustomAttributes(typeof(XmlSerializeBaseAttribute), false);
				
				foreach (XmlSerializeBaseAttribute attrib in attribs)
				{
					if (attrib.Applies(options))
					{
						if (attrib.SerializeBase)
						{
							serializeBase = true;
						}
					}
				}

				if (!serializeBase)
				{
					break;
				}

				type = type.BaseType;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <param name="cache"></param>
		/// <param name="options"></param>
		private void AddMember(MemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			SerializationMemberInfo serializationMemberInfo;

			serializationMemberInfo = new SerializationMemberInfo(memberInfo, options, cache);

			if (serializationMemberInfo.SerializedNodeType == XmlNodeType.Element)
			{
				m_ElementMembersMap[serializationMemberInfo.GetSerializedName()] = new LightSerializationMember(serializationMemberInfo);

				return;
			}
			else  if (serializationMemberInfo.SerializedNodeType == XmlNodeType.Attribute)
			{
				if (!(serializationMemberInfo.Serializer is TypeSerializerWithSimpleTextSupport))
				{
					throw new InvalidOperationException("Serializer for member doesn't support serializing to an attribute.");
				}

				m_AttributeMembersMap[serializationMemberInfo.GetSerializedName()] = new LightSerializationMember(serializationMemberInfo);
			}			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="writer"></param>
		/// <param name="state"></param>
		public override void Serialize(object obj, XmlWriter writer, SerializationState state)
		{
			TypeSerializerWithSimpleTextSupport simpleSerializer;

			state.SerializationStart(obj);

			try
			{				
				// Serialize attributes...
 
				foreach (LightSerializationMember smi in m_AttributeMembersMap.Values)
				{
					object val;
					 
					val = smi.GetValue(obj);
 
					if (smi.XmlTreatAsNullIfEmpty)
					{
						if (smi.LogicalType.IsValueType)
						{
							if (Activator.CreateInstance(smi.LogicalType).Equals(obj))
							{
								val = null;
							}
						}
					}

					if (state.ShouldSerialize(val))
					{
						simpleSerializer = (TypeSerializerWithSimpleTextSupport)smi.Serializer;

						writer.WriteStartAttribute(smi.SerializedName, "");
						writer.WriteString(simpleSerializer.Serialize(val, state));
						writer.WriteEndAttribute();
					}
				}

				// Serialize elements...
 
				foreach (LightSerializationMember smi in m_ElementMembersMap.Values)
				{
					object val;
 
					val = smi.GetValue(obj);

					if (smi.XmlTreatAsNullIfEmpty)
					{
						if (smi.LogicalType.IsValueType)
						{
							if (Activator.CreateInstance(smi.LogicalType).Equals(val))
							{
								val = null;
							}
						}
					}

					if (smi.SerializeAsCData)
					{
						simpleSerializer = smi.Serializer as TypeSerializerWithSimpleTextSupport;
					}
					else
					{
						simpleSerializer = null;
					}

					if (state.ShouldSerialize(val))
					{
						writer.WriteStartElement(smi.SerializedName, "");
						
						if (simpleSerializer != null)
						{
							writer.WriteCData(simpleSerializer.Serialize(val, state));
						}
						else
						{
							smi.Serializer.Serialize(val, writer, state);
						}

						writer.WriteEndElement();
					}
				}
				
			}
			finally
			{
				state.SerializationEnd(obj);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public override object Deserialize(XmlReader reader, SerializationState state)
		{
			object retval;
			LightSerializationMember serializationMember;
			ISerializationUnhandledMarkupListener uhm;
			
			retval = Activator.CreateInstance(m_Type);

			state.DeserializationStart(retval);

			uhm = retval as ISerializationUnhandledMarkupListener;

			if (reader.AttributeCount > 0)
			{
				for (int i = 0; i < reader.AttributeCount; i++)
				{
					reader.MoveToAttribute(i);

					serializationMember = (LightSerializationMember)m_AttributeMembersMap[reader.Name];

					if (serializationMember == null)
					{
						// Unknown attribute.						

						if (uhm != null)
						{
							uhm.UnhandledAttribute(reader.Name, reader.Value);
						}
					}
					else
					{
						serializationMember.SetValue(retval, serializationMember.Serializer.Deserialize(reader, state));
					}
				}

				reader.MoveToElement();
			}

			if (reader.IsEmptyElement)
			{
				reader.ReadStartElement();
 
				return retval;
			}
 
			reader.ReadStartElement();
 			
			// Read elements
 
			while (true)
			{
				XmlReaderHelper.ReadUntilAnyTypesReached(reader, 
					new XmlNodeType[] { XmlNodeType.Element, XmlNodeType.EndElement});
 
				if (reader.NodeType == XmlNodeType.Element)
				{
					serializationMember = (LightSerializationMember)m_ElementMembersMap[reader.Name];
 
					if (serializationMember == null)
					{
						// Unknown element.
					}
					else
					{
						serializationMember.SetValue(retval, serializationMember.Serializer.Deserialize(reader, state));
					}
				}
				else
				{
					if (reader.NodeType == XmlNodeType.EndElement)
					{
						reader.ReadEndElement();
					}
					else
					{
						if (uhm != null)
						{
							uhm.UnhandledOther(reader.ReadOuterXml());
						}
					}
 
					break;
				}
			}

			state.DeserializationEnd(retval);

			return retval;
		}
	}
}
