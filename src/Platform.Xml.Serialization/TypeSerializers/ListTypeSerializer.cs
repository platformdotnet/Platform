using System;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Describes the types of the items in a list to be serialized.
	/// </summary>
	/// <remarks>
	/// <p>
	/// You need to mark any IList field or property to be serialized with this attribute
	/// at least once.  The attribute is used to map an element name to the type
	/// of object contained in the list.
	/// </p>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlListElementAttribute
		: XmlElementAttribute
	{
		/// <summary>
		///
		/// </summary>
		public string Alias
		{
			get
			{
				return this.Name;
			}
			
			set
			{
				this.Name = value;
			}
		}

		/// <summary>
		///
		/// </summary>
		public Type ItemType
		{
			get
			{
				return this.Type;
			}
			
			set
			{
				this.Type = value;
			}
		}

		/// <summary>
		/// Specifies a list item's type.
		/// </summary>
		/// <remarks>
		/// The type's name will be used as the alias for all elements with the type.
		/// If the type has been attributed with an XmlElement attribute then the alias
		/// specified in that attribute will be used.
		/// </remarks>
		/// <param name="itemType">The type of element the list can contain.</param>
		public XmlListElementAttribute(Type itemType)
			: this(itemType, itemType.Name)
		{			
		}

		/// <summary>
		/// Specifies a list item's type.
		/// </summary>
		/// <remarks>
		/// The supplied alias will be used to map the actual element <c>Type</c> with
		/// an XML element.
		/// </remarks>
		/// <param name="itemType"></param>
		/// <param name="alias"></param>
		public XmlListElementAttribute(Type itemType, string alias)
			: base(alias, itemType)
		{
		}
	}


	/// <summary>
	/// 
	/// </summary>
	public class ListTypeSerializer
		: TypeSerializer
	{
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
				return typeof(IList);
			}
		}

		private IDictionary m_TypeToItemMap;
		private IDictionary m_AliasToItemMap;		
		private Type m_ListType;

		public class ListItem
		{
			public string Alias;
			public XmlListElementAttribute Attribute;
			public TypeSerializer Serializer;			
		}

		public ListTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			m_TypeToItemMap = new Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
			m_AliasToItemMap = new Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);

			Scan(memberInfo, cache, options);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <param name="cache"></param>
		/// <param name="options"></param>
		private void Scan(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			IList attributes;
			SerializationMemberInfo smi;
			XmlSerializationAttribute[] attribs;
			
			attributes = new ArrayList(10);
			
			// Get the ElementType attributes specified on the type itself as long
			// as we're not the type itself!

			if (memberInfo.MemberInfo != memberInfo.LogicalType)
			{
				smi = new SerializationMemberInfo(memberInfo.LogicalType, options, cache);

				attribs = smi.GetApplicableAttributes(typeof(XmlListElementAttribute));

				foreach (Attribute a in attribs)
				{
					attributes.Add(a);
				}
			}
			
			// Get the ElementType attributes specified on the member.

			attribs = memberInfo.GetApplicableAttributes(typeof(XmlListElementAttribute));

			foreach (Attribute a in attribs)
			{
				attributes.Add(a);
			}

			
			foreach (XmlListElementAttribute attribute in attributes)
			{
				SerializationMemberInfo smi2;
				ListItem listItem = new ListItem();
								
				smi2 = new SerializationMemberInfo(attribute.ItemType, options, cache);
				
				if (attribute.Alias == null)
				{
					attribute.Alias = smi2.SerializedName;
				}
				
				listItem.Attribute = attribute;
				listItem.Alias = attribute.Alias;

				// Check if a specific type of serializer is specified.

				if (attribute.SerializerType == null)
				{
					// Figure out the serializer based on the type of the element.

					listItem.Serializer = cache.GetTypeSerializerBySupportedType(attribute.ItemType, smi2);
				}
				else
				{
					// Get the type of serializer they specify.

					listItem.Serializer = cache.GetTypeSerializerBySerializerType(attribute.SerializerType, smi2);
				}

				m_TypeToItemMap[attribute.ItemType] = listItem;
				m_AliasToItemMap[attribute.Alias] = listItem;
			}

			if (m_TypeToItemMap.Count == 0)
			{
				if (memberInfo.LogicalType.IsArray)
				{					
					ListItem listItem;
					Type elementType;

					listItem = new ListItem();
					
					elementType = memberInfo.LogicalType.GetElementType();
					listItem.Alias = elementType.Name;

					listItem.Serializer = cache.GetTypeSerializerBySupportedType(elementType, new SerializationMemberInfo(elementType, options, cache));

					m_TypeToItemMap[elementType] = listItem;
					m_AliasToItemMap[listItem.Alias] = listItem;
				}
			}

			if (m_TypeToItemMap.Count == 0)
			{
				throw new InvalidOperationException("Must specify at least one XmlListElementype.");
			}

			m_ListType = memberInfo.LogicalType;

			if (m_ListType.IsAbstract)
			{
				m_ListType = typeof(ArrayList);
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
			foreach (object item in (IList)obj)
			{
				ListItem listItem;

				listItem = (ListItem)m_TypeToItemMap[item.GetType()];

				writer.WriteStartElement(listItem.Alias);

				if (listItem.Attribute != null 
					&& listItem.Attribute.SerializeAsValueNode
					&& listItem.Attribute.ValueNodeAttributeName != null
					&& listItem.Serializer is TypeSerializerWithSimpleTextSupport)
				{
					writer.WriteAttributeString(listItem.Attribute.ValueNodeAttributeName,
						((TypeSerializerWithSimpleTextSupport)listItem.Serializer).Serialize(item, state));
				}
				else
				{
					listItem.Serializer.Serialize(item, writer, state);
				}

				writer.WriteEndElement();
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
			IList retval = null;

			if (m_ListType.IsArray)
			{
				retval = new ArrayList();
			}
			else
			{
				retval = (IList)Activator.CreateInstance(m_ListType);

				state.DeserializationStart(retval);
			}

			// Go thru all elements and deserialize each one.
					
			if (reader.IsEmptyElement)
			{
				reader.ReadStartElement();
			}
			else
			{
				reader.ReadStartElement();

				for (;;)
				{
					ListItem listItem;

					XmlReaderHelper.ReadUntilAnyTypesReached(reader, XmlReaderHelper.ElementOrEndElement);

					if (reader.NodeType == XmlNodeType.Element)
					{
						listItem = (ListItem)m_AliasToItemMap[reader.Name];
	 
						if (listItem.Attribute != null 
							&& listItem.Attribute.SerializeAsValueNode
							&& listItem.Attribute.ValueNodeAttributeName != null
							&& listItem.Serializer is TypeSerializerWithSimpleTextSupport)
						{
							string s;

							s = reader.GetAttribute(listItem.Attribute.ValueNodeAttributeName);

							retval.Add(((TypeSerializerWithSimpleTextSupport)listItem.Serializer).Deserialize(s, state));

							XmlReaderHelper.ReadAndConsumeMatchingEndElement(reader);
						}
						else
						{
							retval.Add(listItem.Serializer.Deserialize(reader, state));
						}
					}
					else
					{
						if (reader.NodeType == XmlNodeType.EndElement)
						{
							reader.ReadEndElement();
						}
	 
						break;
					}
				}
			}
			
			if (m_ListType.IsArray)
			{
				Array array = Array.CreateInstance(m_ListType.GetElementType(), retval.Count);

				state.DeserializationStart(retval);

				retval.CopyTo(array, 0);

				retval = array;
			}

			state.DeserializationEnd(retval);

			return retval;
		}
	}
}

