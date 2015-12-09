using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Platform.Validation
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
	public class ValuePatternConstraint
		: ValidationAttribute
	{
		public string Pattern
		{
			get
			{
				return this.pattern;
			}
			set
			{
				this.pattern = value;
				patternRegex = new Regex(value, RegexOptions.Compiled);
			}
		}
		private string pattern;
		internal Regex patternRegex;

		public bool AllowDefault
		{
			get;
			set;
		}
        
		public ValuePatternConstraint()
			: this("")
		{
		}

		public ValuePatternConstraint(string pattern)
		{
			this.Pattern = pattern;
		}

		public override string CreateExceptionString(IPropertyValidationContext context)
		{
			return $@"The value does not match the regex pattern: {this.pattern}";
		}

		public override Expression<Func<PropertyValidationContext<OBJECT_TYPE, PROPERTY_TYPE>, ValidationResult>> GetValidateExpression<OBJECT_TYPE, PROPERTY_TYPE>()
		{
			return c => (this.AllowDefault && c.PropertyValue == null) ? ValidationResult.Success : (this.patternRegex.IsMatch(c.PropertyValue.ToString()) ? ValidationResult.Success : new ValidationResult(new ValidationException(c)));
		}
	}
}
