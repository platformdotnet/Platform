using System;

namespace Platform.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
	public class BaseValidationAttribute
		: Attribute, INamed
	{
		public virtual string Name
		{
			get
			{
				if (this.GetType().Name.EndsWith("Attribute"))
				{
					return this.GetType().Name;
				}
				else
				{
					var name = this.GetType().Name;

					return name.Substring(0, name.Length - "Attribute".Length);
				}
			}
		}

		public virtual string CreateExceptionString(IPropertyValidationContext context)
		{
			return String.Format(@"{0}", this.Name);
		}
	}
}
