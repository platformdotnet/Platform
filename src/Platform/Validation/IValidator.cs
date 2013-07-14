namespace Platform.Validation
{
	public interface IValidator
	{
		ValidationResult Validate(object value);
	}
}
