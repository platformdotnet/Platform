using System;
using System.Linq.Expressions;

namespace Platform.Validation
{
	/// <summary>
	/// An attribute that specifies the size contraints for a certain type or property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Parameter)]
	public class SizeConstraintAttribute
		: ValidationAttribute
	{
		private int minimumLength = int.MinValue;
		private int maximumLength = int.MaxValue;

		public SizeFlexibility SizeFlexibility
		{
			get;
			set;
		}

		public int MinimumLength
		{
			get
			{
				return minimumLength;
			}
			set
			{
				this.minimumLength = value;
				this.maximumLength = Math.Max(this.minimumLength, this.maximumLength);
			}
		}

		public int MaximumLength
		{
			get
			{
				return this.maximumLength;
			}
			set
			{
				this.maximumLength = value;
				this.minimumLength = Math.Min(this.minimumLength, this.maximumLength);
			}
		}

		public override string CreateExceptionString(IPropertyValidationContext context)
		{
			var value = context.PropertyValue as string;

			return $@"The value's length must be between {this.MinimumLength} and {this.MaximumLength} (inclusive) but is {(value == null ? "null" : value.Length.ToString())}";
		}

		public override Expression<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>> GetValidateExpression<OBJECT_TYPE, PROPERTY_TYPE>()
		{
			if (typeof(PROPERTY_TYPE) != typeof(string))
			{
				throw new InvalidOperationException($"The type {typeof(PROPERTY_TYPE).Name} must be a string type");
			}

			var propertyValidationContext = Expression.Parameter(typeof(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>), "propertyValidationContext");

			var minimumCheck = Expression.Call
			(
				null,
				ValidationResult.CreateResultMethodInfo,
				Expression.GreaterThanOrEqual(Expression.Property(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>.GetCurrentValueExpression(propertyValidationContext), "Length"), Expression.Constant(MinimumLength)),
				Expression.Convert(propertyValidationContext, typeof(IPropertyValidationContext))
			);

			var maximumCheck = Expression.Call
			(
				null,
				ValidationResult.CreateResultMethodInfo,
				Expression.LessThanOrEqual(Expression.Property(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>.GetCurrentValueExpression(propertyValidationContext), "Length"), Expression.Constant(MaximumLength)),
				Expression.Convert(propertyValidationContext, typeof(IPropertyValidationContext))
			);

			var ifNullResult = Expression.Call
			(
				null,
				ValidationResult.CreateResultMethodInfo,
				Expression.Constant(false),
				Expression.Convert(propertyValidationContext, typeof(IPropertyValidationContext))
			);

			var body = Expression.Condition
			(
				Expression.Equal(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>.GetCurrentValueExpression(propertyValidationContext), Expression.Constant(null)),
				ifNullResult,
				Expression.Call(minimumCheck, ValidationResult.MergeWithMethodInfo, maximumCheck)
			);

			return Expression.Lambda<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>>(body, propertyValidationContext);
		}
	}
}
