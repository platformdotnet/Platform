using System.Linq.Expressions;
using System.Reflection;

namespace Platform.Validation
{
	public struct PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>
		: IPropertyValidationContext
	{
		public BaseValidationAttribute ValidationAttribute
		{
			get;
			private set;
		}

		public ValidationContext<OBJECT_TYPE> ValidationContext
		{
			get;
			private set;
		}

		object IPropertyValidationContext.ObjectValue
		{
			get
			{
				return this.ObjectValue;
			}
		}

		public OBJECT_TYPE ObjectValue
		{
			get
			{
				return this.ValidationContext.ObjectValue;
			}
		}

		public PropertyInfo PropertyInfo
		{
			get;
			internal set;
		}

		public PROPERTY_TYPE PropertyValue
		{
			get;
			private set;
		}

		object IPropertyValidationContext.PropertyValue
		{
			get
			{
				return this.PropertyValue;
			}
		}

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
