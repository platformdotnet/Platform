using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;

namespace Platform.Xml.Serialization
{
	public class ListTypeSerializer
		: ComplexTypeTypeSerializer
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
				return listType;
			}
		}

		private Type listType; 
		private readonly IDictionary<Type, ListItem> typeToItemMap;
		private readonly IDictionary<string, ListItem> aliasToItemMap;

	    private class ListItem
	    {
	        private string _alias;

	        public string Alias
	        {
	            get
	            {
	                if (this.Attribute == null)
	                    return _alias;
	                if (this.Attribute.MakeNameLowercase)
	                    return _alias.ToLower();
	                return _alias;
	            }
	            set { _alias = value; }
	        }

			public XmlListElementAttribute Attribute;
			public TypeSerializer Serializer;			
		}

		private readonly TypeSerializerCache cache;
		private readonly IXmlListElementDynamicTypeProvider dynamicTypeResolver;
		
		public ListTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
			: base(memberInfo, memberInfo.ReturnType, cache, options)
		{
			typeToItemMap = new Dictionary<Type, ListItem>();
			aliasToItemMap = new Dictionary<string, ListItem>();

			var attribute = (XmlListElementDynamicTypeProviderAttribute)memberInfo.GetFirstApplicableAttribute(typeof(XmlListElementDynamicTypeProviderAttribute));

			if (attribute != null)
			{
				if (dynamicTypeResolver == null)
				{
					try
					{
						dynamicTypeResolver  = (IXmlListElementDynamicTypeProvider)Activator.CreateInstance(attribute.ProviderType, new object[0]);
					}
					catch (Exception)
					{
					}
				}

				if (dynamicTypeResolver == null)
				{
					dynamicTypeResolver = (IXmlListElementDynamicTypeProvider)Activator.CreateInstance(attribute.ProviderType, new object[] {memberInfo, cache, options});
				}
			}

			serializationMemberInfo = memberInfo;

			this.cache = cache;

			Scan(memberInfo, cache, options);
		}

		private readonly SerializationMemberInfo serializationMemberInfo;

		protected virtual void Scan(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			XmlSerializationAttribute[] attribs;

			var attributes = new List<Attribute>();

			// Get the ElementType attributes specified on the type itself as long
			// as we're not the type itself!

			if (memberInfo.MemberInfo != memberInfo.ReturnType)
			{
				var smi = new SerializationMemberInfo(memberInfo.ReturnType, options, cache);

				attribs = smi.GetApplicableAttributes(typeof(XmlListElementAttribute));

				foreach (var a in attribs)
				{
					attributes.Add(a);
				}
			}
			
			// Get the ElementType attributes specified on the member.

			attribs = memberInfo.GetApplicableAttributes(typeof(XmlListElementAttribute));

			foreach (var a in attribs)
			{
				attributes.Add(a);
			}
			
			foreach (XmlListElementAttribute attribute in attributes)
			{
				var listItem = new ListItem();
				
				if (attribute.Type == null)
				{
					if (serializationMemberInfo.ReturnType.IsArray)
					{
						attribute.Type = serializationMemberInfo.ReturnType.GetElementType();
					}
					else if (serializationMemberInfo.ReturnType.IsGenericType)
					{
						attribute.Type = serializationMemberInfo.ReturnType.GetGenericArguments()[0];
					}
				}
			
				var smi2 = new SerializationMemberInfo(attribute.ItemType, options, cache);

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

				typeToItemMap[attribute.ItemType] = listItem;
				aliasToItemMap[attribute.Alias] = listItem;
			}

			if (typeToItemMap.Count == 0)
			{
				if (memberInfo.ReturnType.IsArray)
				{
					var listItem = new ListItem();
					var elementType = memberInfo.ReturnType.GetElementType();
					var sm = new SerializationMemberInfo(elementType, options, cache);

					listItem.Alias = sm.SerializedName;

					listItem.Serializer = cache.GetTypeSerializerBySupportedType(elementType, new SerializationMemberInfo(elementType, options, cache));

					typeToItemMap[elementType] = listItem;
					aliasToItemMap[listItem.Alias] = listItem;
				}
			}

			if (memberInfo.ReturnType.IsGenericType)
			{
				var elementType = memberInfo.ReturnType.GetGenericArguments()[0];

				if (!typeToItemMap.ContainsKey(elementType) && dynamicTypeResolver == null && !(elementType.IsAbstract || elementType.IsInterface))
				{
					var listItem = new ListItem();
					var sm = new SerializationMemberInfo(elementType, options, cache);

					listItem.Alias = sm.SerializedName;

					listItem.Serializer = cache.GetTypeSerializerBySupportedType(elementType, new SerializationMemberInfo(elementType, options, cache));

					typeToItemMap[elementType] = listItem;
					aliasToItemMap[listItem.Alias] = listItem;
				}
			}

			if (typeToItemMap.Count == 0 && this.dynamicTypeResolver == null)
			{
			    throw new InvalidOperationException(
			        string.Format(
			            "Must specify at least one XmlListElemenType or an XmlListElementTypeSerializerProvider for field {0}",
			             ((Type)memberInfo.MemberInfo).FullName));
			}

			listType = memberInfo.ReturnType;		
		}

		protected override void SerializeElements(object obj, XmlWriter writer, SerializationContext state)
		{
			base.SerializeElements (obj, writer, state);

			foreach (var item in (System.Collections.IEnumerable)obj)
			{
				if (state.ShouldSerialize(item, this.serializationMemberInfo))
				{
					ListItem listItem;

					if (typeToItemMap.TryGetValue(item.GetType(), out listItem))
					{
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
					}
					else
					{
						if (this.dynamicTypeResolver == null)
						{
							throw new XmlSerializerException();
						}
						else
						{
							var type = this.dynamicTypeResolver.GetType(item);

							if (type == null)
							{
								throw new XmlSerializerException();
							}

							var serializer = cache.GetTypeSerializerBySupportedType(type);

							writer.WriteStartElement(this.dynamicTypeResolver.GetName(item));

							serializer.Serialize(item, writer, state);
						}
					}

					writer.WriteEndElement();
				}
			}
		}

		protected override object CreateInstance(XmlReader reader, SerializationContext state)
		{
			if (listType.IsArray)
			{
				return new List<object>();
			}
			else
			{
				return Activator.CreateInstance(serializationMemberInfo.GetReturnType(reader));
			}
		}

		protected override bool CanDeserializeElement(object obj, XmlReader reader, SerializationContext state)
		{
			if (aliasToItemMap[reader.Name] != null)
			{
				return true;
			}

			return base.CanDeserializeElement (obj, reader, state);
		}

		protected override void DeserializeElement(object obj, XmlReader reader, SerializationContext state)
		{
			ListItem listItem;
			bool isGenericList;
			MethodInfo methodInfo;
			System.Collections.IList list;

			if (obj is System.Collections.IList)
			{
				methodInfo = null;
				isGenericList = false;
				list = (System.Collections.IList)obj;
			}
			else
			{
				list = null;
				isGenericList = true;

				methodInfo = obj.GetType().GetMethod("Add");
			}

			if (base.CanDeserializeElement(obj, reader, state))
			{
				base.DeserializeElement (obj, reader, state);

				return;
			}

			if (aliasToItemMap.TryGetValue(reader.Name, out listItem))
			{	
				if (listItem.Attribute != null 
					&& listItem.Attribute.SerializeAsValueNode
					&& listItem.Attribute.ValueNodeAttributeName != null
					&& listItem.Serializer is TypeSerializerWithSimpleTextSupport)
				{
					var s = reader.GetAttribute(listItem.Attribute.ValueNodeAttributeName);

					if (isGenericList)
					{
						methodInfo.Invoke(obj, new object[] { ((TypeSerializerWithSimpleTextSupport)listItem.Serializer).Deserialize(s, state) });
					}
					else
					{
						list.Add(((TypeSerializerWithSimpleTextSupport)listItem.Serializer).Deserialize(s, state));
					}

					XmlReaderHelper.ReadAndConsumeMatchingEndElement(reader);
				}
				else
				{
					if (isGenericList)
					{
						methodInfo.Invoke(obj, new object[] { listItem.Serializer.Deserialize(reader, state) });
					}
					else
					{
						list.Add(listItem.Serializer.Deserialize(reader, state));
					}
				}
			}
			else
			{
				TypeSerializer serializer = null;

				if (this.dynamicTypeResolver != null)
				{
					var type = dynamicTypeResolver.GetType(reader);

					if (type != null)
					{								
						serializer = cache.GetTypeSerializerBySupportedType(type);
					}
				}

				if (serializer == null)
				{
					base.DeserializeElement (obj, reader, state);
				}
				else
				{
					if (isGenericList)
					{
						methodInfo.Invoke(obj, new object[] { serializer.Deserialize(reader, state) });
					}
					else
					{
						list.Add(serializer.Deserialize(reader, state));
					}
				}
			}
		}

		public override object Deserialize(XmlReader reader, SerializationContext state)
		{
			var retval = base.Deserialize(reader, state);

			if (listType.IsArray)
			{
				int count;

				if (retval is System.Collections.IList)
				{					
					count = ((System.Collections.IList)retval).Count;
				}
				else
				{
					count = (int)retval.GetType().GetProperty("Count", BindingFlags.Instance | BindingFlags.Public).GetValue(retval, new object[0]);
				}

				var array = Array.CreateInstance(listType.GetElementType(), count);

				state.DeserializationStart(retval);

				if (retval is System.Collections.IList)
				{
					((System.Collections.IList)retval).CopyTo(array, 0);
				}
				else
				{
					retval.GetType().GetMethod
						(
							"CopyTo",
							BindingFlags.Instance | BindingFlags.Public
						).Invoke(retval, new object[] { array, 0 });
				}
				
				retval = array;
			}

			state.DeserializationEnd(retval);

			return retval;
		}
	}
}

