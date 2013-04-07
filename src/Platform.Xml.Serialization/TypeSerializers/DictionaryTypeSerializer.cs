using System;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	public class DictionaryElementTypeAttribute
		: XmlSerializationAttribute
	{
	}

	/// <summary>
	/// Type serializer that supports serialization of objects supporting IDictionary.
	/// </summary>
	/// <example>
	/// <code>
	/// public class Pig
	/// {
	///   [XmlElement]
	///   public string Name = "piglet"
	/// }
	/// 
	/// public class Cow
	/// {
	///   [XmlAttribute]
	///   public string Name = "daisy"
	/// }
	/// 
	/// public class Farm
	/// {
	///   [DictionaryElementType(typeof(Pig), "pig")]
	///   [DictionaryElementType(typeof(Cow), "cow")]	
	///   public IDictionary Animals;
	///   
	///   public Farm()
	///   {
	///     IDictionary dictionary = new Hashtable();
	///     dictionary["Piglet"] = new Pig();
	///     dictionary["Daisy"] = new Cow();
	///   }
	/// }
	/// </code>
	/// 
	/// Serialized output of Farm:
	/// 
	/// <code>
	/// <Farm>
	///   <Animals>
	///      <Piglet typealias="pig"><name>piglet</name></Piglet>
	///      <Daisy typealias="cow" name="daisy"></Daisy>
	///   </Animals>
	/// </Farm>
	/// </code>
	/// If only one DictionaryElementAttribute is present, the dictionary is assumed to only
	/// contain the type specified by that attribute.  The typealias is therefore not needed
	/// and will be omitted from the serialized output.
	/// </example>
	public class DictionaryTypeSerializer
		: TypeSerializer
	{
		/// <summary>
		/// Returns true.
		/// </summary>
		public override bool MemberBound
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Returns <c>typeof(IDictionary)</c>.
		/// </summary>
		public override Type SupportedType
		{
			get
			{
				return typeof(IDictionary);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <param name="cache"></param>
		/// <param name="options"></param>
		public DictionaryTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
		}

		/// <summary>
		/// <see cref="TypeSerializer.Serialize(object, XmlWriter, SerializationState)"/>
		/// </summary>
		public override void Serialize(object obj, XmlWriter writer, SerializationState state)
		{
		}

		/// <summary>
		/// <see cref="TypeSerializer.Deserialize(XmlReader, SerializationState)"/>
		/// </summary>
		public override object Deserialize(XmlReader reader, SerializationState state)
		{
			return null;
		}
	}
}
