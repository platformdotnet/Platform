using System;

namespace Platform.Validation
{
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Parameter)]
	public class UnsetValueAttribute
		: BaseValidationAttribute
	{
		public object Value { get; set; }
		public string ValueExpression { get; set; }

		public UnsetValueAttribute()
		{
		}

		public UnsetValueAttribute(object valueExpression)
		{
			this.Value = valueExpression;
		}

		public UnsetValueAttribute(string valueExpression)
		{
			this.ValueExpression = valueExpression;
		}
	}
}
