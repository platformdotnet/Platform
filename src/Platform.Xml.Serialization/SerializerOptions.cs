using System.Collections.Generic;

namespace Platform.Xml.Serialization
{
	public class SerializerOptions
	{
		public static readonly string WriteXmlheader = "WriteXmlheader";
		private static readonly SerializerOptions empty = new SerializerOptions();

		private readonly Dictionary<string, object> options = new Dictionary<string, object>();

		public static SerializerOptions Empty => empty;

		public object GetOption(string name)
		{
			return this.options[name];
		}

		public bool TryGetValue(string name, out object value)
		{
			return this.options.TryGetValue(name, out value);
		}

		public SerializerOptions(params object[] options)
		{
			for (var i = 0; i < options.Length; i += 2)
			{
				this.options[options[i].ToString()] = options[i + 1];
			}
		}
	}
}
