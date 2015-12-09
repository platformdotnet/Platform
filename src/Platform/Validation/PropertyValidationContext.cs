using System.Linq.Expressions;
using System.Reflection;

namespace Platform.Validation
{
	public struct PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>
		: IPropertyValidationContext
	{
		public BaseValidationAttribute ValidationAttribute { get; }

		public ValidationContext<OBJECT_TYPE> ValidationContext { get; }

		object IPropertyValidationContext.ObjectValue => this.ObjectValue;

		public OBJECT_TYPE ObjectValue => this.ValidationContext.ObjectValue;

		public PropertyInfo PropertyInfo { get; internal set; }

		public PROPERTY_TYPE PropertyValue { get; }

		object IPropertyValidationContext.PropertyValue => this.PropertyValue;

		public static Expression GetCurrentValueExpression(ParameterExpression propertyValidationContextExpression)
		{
			return Expression.Property(propertyValidationContextExpression, "PropertyValue");
		}

		public PropertyValidationContext(ValidationContext<OBJECT_TYPE> validationContext, PropertyInfo currentPropertyInfo, BaseValidationAttribute validationAttribute, PROPERTY_TYPE propertyValue)
			: this()
		{
			this.PropertyValue = propertyValue;
			this.ValidationContext = validationContext;
			this.ValidationAttribute = validationAttribute;
			this.PropertyInfo = currentPropertyInfo;
		}
	}
}
