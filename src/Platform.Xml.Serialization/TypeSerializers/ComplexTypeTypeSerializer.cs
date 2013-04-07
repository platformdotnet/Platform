using System;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// 
	/// </summary>
	public class ComplexTypeTypeSerializer
		: TypeSerializer
	{
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
		public ComplexTypeTypeSerializer(Type type, TypeSerializerCache cache, SerializerOptions options)
		{
			m_Type = type;

			m_ElementMembersMap = new Hashtable(0x10);
			m_AttributeMembersMap = new Hashtable(0x10);

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
				if (serializationMemberInfo.Namespace.Length > 0)
				{
					m_ElementMembersMap[serializationMemberInfo.Namespace + (char)0xff + serializationMemberInfo.SerializedName] = serializationMemberInfo;
				}
				else
				{
					m_ElementMembersMap[serializationMemberInfo.SerializedName] = serializationMemberInfo;
				}

				return;
			}
			else  if (serializationMemberInfo.SerializedNodeType == XmlNodeType.Attribute)
			{
				if (!(serializationMemberInfo.Serializer is TypeSerializerWithSimpleTextSupport))
				{
					throw new InvalidOperationException("Serializer for member doesn't support serializing to an attribute.");
				}

				if (serializationMemberInfo.Namespace.Length > 0)
				{
					m_AttributeMembersMap[serializationMemberInfo.Namespace + (char)0xff + serializationMemberInfo.SerializedName] = serializationMemberInfo;
				}
				else
				{
					m_AttributeMembersMap[serializationMemberInfo.SerializedName] = serializationMemberInfo;
				}
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
 
				foreach (SerializationMemberInfo smi in m_AttributeMembersMap.Values)
				{
					object val;
					 
					val = smi.GetValue(obj);
 
					if (smi.TreatAsNullIfEmpty)
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
 
				foreach (SerializationMemberInfo smi in m_ElementMembersMap.Values)
				{
					object val;
 
					val = smi.GetValue(obj);

					if (smi.TreatAsNullIfEmpty)
					{
						if (smi.LogicalType.IsValueType)
						{
							if (Activator.CreateInstance(smi.LogicalType).Equals(val))
							{
								val = null;
							}
						}
					}

					simpleSerializer = smi.Serializer as TypeSerializerWithSimpleTextSupport;
					
					if (state.ShouldSerialize(val))
					{
						if (smi.Namespace.Length > 0)
						{
							writer.WriteStartElement(state.Parameters.Namespaces.GetPrefix(smi.Namespace), smi.SerializedName, smi.Namespace);
						}
						else
						{
							writer.WriteStartElement(smi.SerializedName, "");
						}
						
						if (smi.SerializeAsValueNodeAttributeName != null)
						{
							writer.WriteAttributeString(smi.SerializeAsValueNodeAttributeName, val.ToString());
						}
						else if (smi.SerializeAsCData)
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
			SerializationMemberInfo serializationMember;
			ISerializationUnhandledMarkupListener uhm;
			
			retval = Activator.CreateInstance(m_Type);

			state.DeserializationStart(retval);

			uhm = retval as ISerializationUnhandledMarkupListener;

			if (reader.AttributeCount > 0)
			{
				for (int i = 0; i < reader.AttributeCount; i++)
				{
					reader.MoveToAttribute(i);

					if (reader.Prefix == "xmlns")
					{
						continue;
					}

					if (reader.Prefix.Length > 0)
					{
						serializationMember = (SerializationMemberInfo)m_AttributeMembersMap[state.Parameters.Namespaces.GetNamespace(reader.Prefix) + (char)0xff + reader.LocalName];
					}
					else
					{
						serializationMember = (SerializationMemberInfo)m_AttributeMembersMap[reader.Name];
					}

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
					if (reader.Prefix.Length > 0)
					{
						serializationMember = (SerializationMemberInfo)m_ElementMembersMap[state.Parameters.Namespaces.GetNamespace(reader.Prefix) + (char)0xff + reader.LocalName];
					}
					else
					{
						serializationMember = (SerializationMemberInfo)m_ElementMembersMap[reader.LocalName];
					}
					
					if (serializationMember == null)
					{						
						// Unknown element.
						reader.Read();
						XmlReaderHelper.ReadAndApproachMatchingEndElement(reader);
					}
					else
					{
						if (serializationMember.SerializeAsValueNodeAttributeName != null
							&& serializationMember.Serializer is TypeSerializerWithSimpleTextSupport)
						{
							string s;

							s = reader.GetAttribute(serializationMember.SerializeAsValueNodeAttributeName);

							serializationMember.SetValue(retval, ((TypeSerializerWithSimpleTextSupport)(serializationMember.Serializer)).Deserialize(s, state));

							XmlReaderHelper.ReadAndConsumeMatchingEndElement(reader);
						}
						else
						{
							serializationMember.SetValue(retval, serializationMember.Serializer.Deserialize(reader, state));
						}
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
