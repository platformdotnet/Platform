using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Platform.Validation
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
	public class ValueRangeConstraintAttribute
		: ValidationAttribute
	{
		public bool AllowDefault { get; set; }
		public object MinimumValue { get; set; }
		public object MaximumValue { get; set; }

		public override string CreateExceptionString(IPropertyValidationContext context)
		{
			return $@"Value must be between {this.MinimumValue ?? "MinValue"} and {this.MaximumValue ?? "MaxValue"} (inclusive) but is {context.PropertyValue}";
		}

		private static object ChangeType(object value, Type type)
		{
			if (value == null)
			{
				return null;
			}

			var underlyingType = Nullable.GetUnderlyingType(type);

			if (underlyingType != null)
			{
				return Convert.ChangeType(value, underlyingType);
			}
			else
			{
				return Convert.ChangeType(value, type);
			}
		}

		public override Expression<Func<PropertyValidationContext<OBJECT_TYPE,PROPERTY_TYPE>,ValidationResult>>  GetValidateExpression<OBJECT_TYPE,PROPERTY_TYPE>()
		{
			var type = typeof(PROPERTY_TYPE);

			if (Nullable.GetUnderlyingType(type) != null)
			{
				type = Nullable.GetUnderlyingType(type);
			}

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					break;
				default:
					throw new NotSupportedException("ValueConstraint: " + typeof(PROPERTY_TYPE));
			}

			PROPERTY_TYPE minimumValue, maximumValue;

			if (this.MinimumValue != null)
			{
				minimumValue = (PROPERTY_TYPE)ChangeType(this.MinimumValue, typeof(PROPERTY_TYPE));
			}
			else
			{
				minimumValue = (PROPERTY_TYPE)typeof(PROPERTY_TYPE).GetDefaultValue();
			}

			if (this.MaximumValue != null)
			{
				maximumValue = (PROPERTY_TYPE)ChangeType(this.MaximumValue, typeof(PROPERTY_TYPE));
			}
			else
			{
				var underlyingType = Nullable.GetUnderlyingType(typeof(PROPERTY_TYPE)) ?? typeof(PROPERTY_TYPE);

				var field = underlyingType.GetField("MaxValue", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

				if (field != null)
				{
					maximumValue = (PROPERTY_TYPE)field.GetValue(null);
				}
				else
				{
					maximumValue = (PROPERTY_TYPE)typeof(PROPERTY_TYPE).GetDefaultValue();
				}
			}

			var propertyValidationContext = Expression.Parameter(typeof(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>), "propertyValidationContext");

			Expression body = null;
            
			var minimumCheck = Expression.Call
			(
				null,
				ValidationResult.CreateResultMethodInfo,
				Expression.GreaterThanOrEqual(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>.GetCurrentValueExpression(propertyValidationContext), Expression.Constant(minimumValue, typeof(PROPERTY_TYPE))),
				Expression.Convert(propertyValidationContext, typeof(IPropertyValidationContext))
			);

			var maximumCheck = Expression.Call
			(
				null,
				ValidationResult.CreateResultMethodInfo,
				Expression.LessThanOrEqual(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>.GetCurrentValueExpression(propertyValidationContext), Expression.Constant(maximumValue, typeof(PROPERTY_TYPE))),
				Expression.Convert(propertyValidationContext, typeof(IPropertyValidationContext))
			);

			body = Expression.Call(minimumCheck, ValidationResult.MergeWithMethodInfo, maximumCheck);

			if (Nullable.GetUnderlyingType(typeof(PROPERTY_TYPE)) != null)
			{
				var nullCheck = Expression.Equal(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>.GetCurrentValueExpression(propertyValidationContext), Expression.Constant(null));
				
				body = Expression.Condition(nullCheck, Expression.Field(null, typeof(ValidationResult).GetField("Success", BindingFlags.Static | BindingFlags.Public)), body);
			}	

			return Expression.Lambda<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>>(body, propertyValidationContext);
		}
	}
}
