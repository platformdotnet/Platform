using System;
using System.Xml;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// </summary>
	public interface ISerializationUnhandledMarkupListener
	{
		void UnhandledAttribute(string name, string value);
		void UnhandledOther(string markup);
	}
}
