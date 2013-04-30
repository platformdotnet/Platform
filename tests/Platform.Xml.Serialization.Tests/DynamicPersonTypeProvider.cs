using System;

namespace Platform.Xml.Serialization.Tests
{
	public class DynamicPersonTypeProvider
		: IXmlDynamicTypeProvider
	{
		public virtual Type GetType(object instance)
		{
			return instance.GetType();
		}

		public virtual Type GetType(System.Xml.XmlReader reader)
		{
			if (!reader.HasAttributes)
			{
				return null;
			}

			var typeName = reader.GetAttribute("type");

			if (typeName == null)
			{
				return null;
			}

			var type = Type.GetType(this.GetType().Namespace + "." + typeName + "Person", false);

			return type;
		}
	}
}
