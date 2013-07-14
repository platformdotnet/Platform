using System;

namespace Platform.Validation
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
	public class UniqueAttribute
		: ValidationAttribute
	{
		public bool Unique { get; set; }

		public UniqueAttribute()
			: this(true)
		{
		}

		public UniqueAttribute(bool unique)
		{
			this.Unique = unique;
		}
	}
}
