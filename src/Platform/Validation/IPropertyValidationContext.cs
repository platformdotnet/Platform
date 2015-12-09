using System.Reflection;

namespace Platform.Validation
{
	public interface IPropertyValidationContext
	{
		BaseValidationAttribute ValidationAttribute { get; }
		object ObjectValue { get; }
		PropertyInfo PropertyInfo { get; }
		object PropertyValue { get; }
	}
}
