using System;
using System.Linq;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]	
	public class XmlVerifyRuntimeTypeAttribute
		: XmlSerializationAttribute
	{
		public Type[] Types { get; set; }

		public XmlVerifyRuntimeTypeAttribute(params Type[] types)
			: this(LogicalOperation.All, types)
		{
		}

		public XmlVerifyRuntimeTypeAttribute(LogicalOperation logicalCheck, params Type[] types)
		{
			this.Types = types;
		}

		public virtual bool VerifiesAgainst(Type type)
		{
			return this.Types.Any(t => t.IsAssignableFrom(type));
		}
	}
}