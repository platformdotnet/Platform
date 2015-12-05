using System;

namespace Platform.Validation
{
	public class ValidationException
		: Exception
	{
		public IPropertyValidationContext PropertyValidationContext
		{
			get;
			private set;
		}
        
		public static string CreateExceptionString(IPropertyValidationContext context)
		{
			return $"The property '{context.PropertyInfo.ReflectedType.Name}.{context.PropertyInfo.Name}' with the value '{(context.PropertyValue == null ? "null" : Convert.ToString(context.PropertyValue))}' failed {context.ValidationAttribute.Name} validation with result: {context.ValidationAttribute.CreateExceptionString(context)}";
		}

		public ValidationException(IPropertyValidationContext context)
			: base(CreateExceptionString(context))
		{
			this.PropertyValidationContext = context;
		}

		public string ValidationMessage
		{
			get
			{
				return this.PropertyValidationContext.ValidationAttribute.CreateExceptionString(this.PropertyValidationContext);
			}
		}
	}
}
