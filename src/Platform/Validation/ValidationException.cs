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
			return String.Format("The property '{0}.{1}' with the value '{2}' failed {3} validation with result: {4}", context.PropertyInfo.ReflectedType.Name, context.PropertyInfo.Name, context.PropertyValue == null ? "null" : Convert.ToString(context.PropertyValue), context.ValidationAttribute.Name, context.ValidationAttribute.CreateExceptionString(context));
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
