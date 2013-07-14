using System.Collections.Generic;

namespace Platform.Validation
{
	public class ValidatorOptions
	{
		public static readonly ValidatorOptions Empty = new ValidatorOptions();

		public IDictionary<string, string> Properties
		{
			get;
			private set;
		}

		public PropertyValidatorProvider PropertyValidatorProvider
		{
			get;
			private set;
		}

		public ValidatorOptions()
		{
			this.Properties = new SortedDictionary<string, string>();
			this.PropertyValidatorProvider = new DefaultPropertyValidatorProvider();
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}

			var typedObj = obj as ValidatorOptions;

			if (typedObj == null)
			{
				return false;
			}

			if (typedObj.Properties == this.Properties)
			{
				return true;
			}

			if (typedObj.Properties.Count != this.Properties.Count)
			{
				return false;
			}

			foreach (var keyValuePair in this.Properties)
			{
				string value;

				if (!typedObj.Properties.TryGetValue(keyValuePair.Key, out value))
				{
					return false;
				}

				if (value != keyValuePair.Value)
				{
					return false;
				}
			}

			return true;
		}

		public override int GetHashCode()
		{
			var retval = 0;

			foreach (var value in this.Properties.Values)
			{
				retval ^= value.GetHashCode();
			}

			return retval;
		}
	}
}
