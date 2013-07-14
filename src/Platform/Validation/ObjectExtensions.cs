namespace Platform.Validation
{
	public static class ObjectExtensions
	{
		public static ValidationResult Validate<T>(this T obj)
			where T : class
		{
			return Validator<T>.NewValidator().Validate(obj);
		}

		public static ValidationResult Validate<T>(this T obj, ValidatorOptions options)
			where T : class
		{
			return Validator<T>.NewValidator(options).Validate(obj);
		}
	}
}
