namespace Platform.Validation
{
	public class DefaultPropertyValidatorProvider
		: PropertyValidatorProvider
	{
		public override PropertyValidator<OBJECT_TYPE, PROPERTY_TYPE> GetPropertyValidator<OBJECT_TYPE, PROPERTY_TYPE>(ValidationAttribute validationAttribute)
		{
			return new DefaultPropertyValidator<OBJECT_TYPE, PROPERTY_TYPE>(validationAttribute);
		}
	}
}
