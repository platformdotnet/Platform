using System;
using System.Linq.Expressions;

namespace Platform.Validation
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
	public class ValueRequiredAttribute
		: ValidationAttribute
	{
		public bool Required { get; set; }

		public ValueRequiredAttribute()
			: this(true)
		{
		}

		public ValueRequiredAttribute(bool required)
		{
			this.Required = required;
		}

		public override string CreateExceptionString(IPropertyValidationContext context)
		{
			return String.Format("Must not be null, default or empty string");
		}

		public override Expression<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>> GetValidateExpression<OBJECT_TYPE, PROPERTY_TYPE>()
		{
			var defaultValue = (PROPERTY_TYPE)typeof(PROPERTY_TYPE).GetDefaultValue();

			if (typeof(PROPERTY_TYPE).IsValueType)
			{
				return value => value.PropertyValue.Equals(defaultValue) ? new ValidationResult(new ValidationException(value)) : ValidationResult.Success;
			}
			else if (typeof(PROPERTY_TYPE) == typeof(string))
			{
				return value => String.IsNullOrEmpty((string)(object)value.PropertyValue) ? new ValidationResult(new ValidationException(value)) : ValidationResult.Success;
			}
			else if (typeof(PROPERTY_TYPE).GetProperty("Count") != null)
			{
				var parameter = Expression.Parameter(typeof(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>), "propertyValidationContext");

				var nulltest = Expression.Equal(Expression.Property(parameter, "PropertyValue"), Expression.Constant(null, typeof(PROPERTY_TYPE)));
				var counttest = Expression.Equal(Expression.Property(Expression.Property(parameter, "PropertyValue"), "Count"), Expression.Constant(0));

				var newValidationException = Expression.New(typeof(ValidationException).GetConstructor(new[] { typeof(IPropertyValidationContext) }), Expression.Convert(parameter, typeof(IPropertyValidationContext)));

				var iftrue = Expression.New(typeof(ValidationResult).GetConstructor(new[] { typeof(ValidationException) }), newValidationException);

				var iffalse = Expression.Field(null, typeof(ValidationResult).GetField("Success", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));

				var body = Expression.Condition(Expression.OrElse(nulltest, counttest), iftrue, iffalse);

				return Expression.Lambda<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>>(body, parameter);
			}
			else
			{
				return value => value.PropertyValue == null ? new ValidationResult(new ValidationException(value)) : ValidationResult.Success;
			}
		}
	}
}
