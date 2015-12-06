using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlVariableSubstitutionAttribute
		: XmlSerializationAttribute
	{
		public Type SubstitutorType { get; set; }

		public XmlVariableSubstitutionAttribute()
			: this(typeof(XmlEnvironmentVariableSubstitutor))
		{
		}

		public XmlVariableSubstitutionAttribute(Type substitutorType)
		{
			this.SubstitutorType = substitutorType;
		}	
	}
}
