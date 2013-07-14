using System;
using System.Linq.Expressions;
using Platform.Validation.Validators;

namespace Platform.Validation
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
	public class ValueExpressionConstraintAttribute
		: ValidationAttribute
	{
		public string ConstraintExpression
		{
			get;
			set;
		}
		
		public ValueExpressionConstraintAttribute(string constraintExpression)
		{
			this.ConstraintExpression = constraintExpression;
		}

		public override string CreateExceptionString(IPropertyValidationContext context)
		{
			return String.Format(@"Value is '{0}' but must satisfy: {1}", context.PropertyValue, this.ConstraintExpression);
		}

		public override Expression<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>> GetValidateExpression<OBJECT_TYPE, PROPERTY_TYPE>()
		{
			var propertyValidationContext = Expression.Parameter(typeof(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>), "propertyValidationContext");

			var parsedConstraintExpression = StringExpressionParser.Parse(propertyValidationContext, this.ConstraintExpression);

			var constraintExpression = Expression.Call
			(
				null,
				ValidationResult.CreateResultMethodInfo,
				parsedConstraintExpression,
				Expression.Convert(propertyValidationContext, typeof(IPropertyValidationContext))
			);
			
			return Expression.Lambda<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>>(constraintExpression, propertyValidationContext);
		}
	}
}
