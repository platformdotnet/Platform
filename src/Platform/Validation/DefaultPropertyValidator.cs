using System;
using System.Linq.Expressions;

namespace Platform.Validation
{
	public class DefaultPropertyValidator<OBJECT_TYPE, PROPERTY_TYPE>
		: PropertyValidator<OBJECT_TYPE, PROPERTY_TYPE>
	{
		private readonly ValidationAttribute attribute;

		public DefaultPropertyValidator(ValidationAttribute attribute)
		{
			this.attribute = attribute;
		}

		public override ValidationResult Validate(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE> propertyValidationContext)
		{
			return this.attribute.Validate(propertyValidationContext);
		}

		public override Expression<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>> GetValidationExpression()
		{
			return this.attribute.GetValidateExpression<OBJECT_TYPE, PROPERTY_TYPE>();
		}
	}
}
