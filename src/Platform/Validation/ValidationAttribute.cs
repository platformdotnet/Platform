using System;
using System.Linq.Expressions;

namespace Platform.Validation
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter)]
	public abstract class ValidationAttribute
		: BaseValidationAttribute
	{
		protected virtual string GetErrorCode()
		{
			return this.GetType().Name;
		}

		public virtual Expression<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>> GetValidateExpression<OBJECT_TYPE, PROPERTY_TYPE>()
		{
			return null;
		}

		public virtual ValidationResult Validate<OBJECT_TYPE, PROPERTY_TYPE>(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE> propertyValidationContext)
		{
			throw new NotImplementedException();
		}
	}
}
