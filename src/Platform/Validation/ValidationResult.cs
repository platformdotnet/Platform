using System.Collections.Generic;
using System.Reflection;

namespace Platform.Validation
{
	public struct ValidationResult
	{
		public static readonly ValidationResult Success = new ValidationResult(new ValidationException[0]);

		public bool IsSuccess
		{
			get
			{
				return this.ValidationExceptions.Count == 0;
			}
		}

		public List<ValidationException> ValidationExceptions { get; private set; }

		public static readonly MethodInfo CreateResultMethodInfo = typeof(ValidationResult).GetMethod("CreateResult", BindingFlags.Static | BindingFlags.Public);

		public static ValidationResult CreateResult(bool success, IPropertyValidationContext propertyValidationContext)
		{
			if (success)
			{
				return ValidationResult.Success;
			}

			return new ValidationResult(new ValidationException(propertyValidationContext));
		}


		public ValidationResult(IEnumerable<ValidationException> exceptions)
			: this()
		{
			this.ValidationExceptions = new List<ValidationException>(exceptions);
		}

		public ValidationResult(ValidationException exception)
			: this()
		{
			this.ValidationExceptions = new List<ValidationException>() { exception };
		}

		public ValidationResult(params ValidationException[] exceptions)
			: this()
		{
			this.ValidationExceptions = new List<ValidationException>(exceptions);
		}

		public ValidationResult(List<ValidationException> exceptions)
			: this()
		{
			this.ValidationExceptions = exceptions ?? new List<ValidationException>(0);
		}

		public static readonly MethodInfo MergeWithMethodInfo = typeof(ValidationResult).GetMethod("MergeWith");

		public ValidationResult MergeWith(ValidationResult validationResult)
		{
			if (validationResult.IsSuccess)
			{
				return this;
			}
            
			if (validationResult.ValidationExceptions.Count == 0)
			{
				return this;
			}

			return new ValidationResult(this.ValidationExceptions.Append(validationResult.ValidationExceptions));
		}
	}
}
