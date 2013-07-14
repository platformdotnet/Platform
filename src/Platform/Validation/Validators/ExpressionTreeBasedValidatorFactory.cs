namespace Platform.Validation.Validators
{
	public class ExpressionTreeBasedValidatorFactory
		: ValidatorFactory
	{
		protected override Validator<T> DoNewValidator<T>(ValidatorOptions options)
		{
			return new ExpressionTreeBasedValidator<T>(options);
		}
	}
}
