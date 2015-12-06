using System.Reflection;

namespace Platform.Validation
{
	public class ValidationContext<OBJECT_TYPE>
	{
		public OBJECT_TYPE ObjectValue { get; }
		public PropertyInfo CurrentPropertyInfo { get; internal set; }
		public ValidatorOptions ValidatorOptions { get; internal set; }

		public U GetPropertyValue<U>(string propertyName)
		{
			return (U)this.ObjectValue.GetType().GetProperty(propertyName).GetValue(this.ObjectValue, null);
		}

		public ValidationContext(OBJECT_TYPE objectValue)
		{
			this.ObjectValue = objectValue;
		}
	}
}
