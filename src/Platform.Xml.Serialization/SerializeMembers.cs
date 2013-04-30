using System;

namespace Platform.Xml.Serialization
{
	[Flags]
	public enum SerializeMembers
	{
		None = 0,
		Attributed = 1,
		Fields = 2,
		Properties = 3,
		FieldsAndProperties = Fields | Properties
	}
}
