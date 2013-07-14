using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Platform.Validation
{
	public abstract class PropertyValidator<OBJECT_TYPE, PROPERTY_TYPE>
	{
		public static MethodInfo ValidateMethod = typeof(PropertyValidator<OBJECT_TYPE, PROPERTY_TYPE>).GetMethod("Validate", BindingFlags.Static | BindingFlags.Public);
		public static MethodInfo GetValidationExpressionMethod = typeof(PropertyValidator<OBJECT_TYPE, PROPERTY_TYPE>).GetMethod("GetValidationExpression", BindingFlags.Static | BindingFlags.Public);

		public abstract ValidationResult Validate(PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE> propertyValidationContext);
		public abstract Expression<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>> GetValidationExpression();
	}
}
