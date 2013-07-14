using System;

namespace Platform.Validation
{
	public class Validator
	{
		public static IValidator NewValidator(Type type, ValidatorOptions options)
		{
			return ValidatorFactory.Default.NewValidator(type, options);
		}

		public static Validator<T> NewValidator<T>()
		{
			return Validator<T>.NewValidator();
		}
	}

	public abstract class Validator<T>
		: IValidator
	{
		public ValidatorOptions ValidatorOptions
		{
			get;
			private set;
		}

		protected Validator(ValidatorOptions validatorOptions)
		{
			this.ValidatorOptions = validatorOptions;
		}

		public ValidationResult Validate(object value)
		{
			return Validate((T)value);
		}

		public abstract ValidationResult Validate(T value);

		public static Validator<T> NewValidator()
		{
			return ValidatorFactory.Default.NewValidator<T>(ValidatorOptions.Empty);
		}

		public static IValidator NewValidator(Type type, ValidatorOptions options)
		{
			return ValidatorFactory.Default.NewValidator(type, options);
		}

		public static Validator<T> NewValidator(ValidatorOptions options)
		{
			return ValidatorFactory.Default.NewValidator<T>(options);
		}
	}
}
