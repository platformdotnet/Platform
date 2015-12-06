using System;
using System.Text.RegularExpressions;

namespace Platform.Xml.Serialization
{
	public class XmlEnvironmentVariableSubstitutor
		: IVariableSubstitutor
	{
		private static readonly Regex staticRegex;

		static XmlEnvironmentVariableSubstitutor()
		{
			staticRegex = new Regex
			(
				@"
					\$\((?<name>([a-zA-Z]+[a-zA-Z_]*))\)
				",
				RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace
			);
		}

		protected static string EnvironmentVariableMatchEvaluator(Match value)
		{
			var name = value.Groups["name"].Value;

			return Environment.GetEnvironmentVariable(name);
		}

		public virtual string Substitute(string value) => staticRegex.Replace(value, EnvironmentVariableMatchEvaluator);
	}
}