namespace Platform.Validation
{
	public abstract class PropertyValidatorProvider
	{
		public abstract PropertyValidator<OBJECT_TYPE, PROPERTY_TYPE> GetPropertyValidator<OBJECT_TYPE, PROPERTY_TYPE>(ValidationAttribute validationAttribute);
	}
}
