using System;
using System.Xml;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Abstract base class for classes that support serializing of objects.
	/// </summary>
	/// <remarks>
	/// Serializers of this type also support seralizing objects to a simple text representation.
	/// Only types are serialized with a TypeSerializerWithSimpleTextSupport can be serialized
	/// to an XML attribute.
	/// </remarks>
	public abstract class TypeSerializerWithSimpleTextSupport
		: TypeSerializer
	{
		public abstract string Serialize(object obj, SerializationContext state);
		public abstract object Deserialize(string value, SerializationContext state);

		public override void Serialize(object obj, XmlWriter writer, SerializationContext state)
		{
			writer.WriteString(Serialize(obj, state));
		}

		public override object Deserialize(XmlReader reader, SerializationContext state)
		{
			var s = XmlReaderHelper.ReadCurrentNodeValue(reader);

			if (state.GetCurrentMemberInfo().Substitutor != null)
			{
				s = state.GetCurrentMemberInfo().Substitutor.Substitute(s);
			}

			return Deserialize(s, state);
		}
	}
}
