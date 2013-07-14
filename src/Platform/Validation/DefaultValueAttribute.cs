using System;

namespace Platform.Validation
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
	public class DefaultValueAttribute
		: BaseValidationAttribute
	{
		public object Value
		{
			get;
			set;
		}

		public string ValueExpression
		{
			get;
			set;
		}

		public DefaultValueAttribute()
		{
		}

		public DefaultValueAttribute(object value)
		{
			this.Value = value;
		}

		public DefaultValueAttribute(string valueExpression)
		{
		}
	}
}
