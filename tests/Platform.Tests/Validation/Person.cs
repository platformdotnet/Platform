using Platform.Validation;

namespace Platform.Tests.Validation
{
	public class Person
	{
		[ValueRequired]
		[SizeConstraint(MinimumLength = 1, MaximumLength = 10)]
		public string Name
		{
			get;
			set;
		}

		public string Email
		{
			get;
			set;
		}
       
		[UnsetValue(ValueExpression = "-1 * -1 - 2")]
		[DefaultValue(ValueExpression = "{Maximum} + 1")]
		public int Minimum
		{
			get;
			set;
		}

		public int Maximum
		{
			get;
			set;
		}

		[ValueRangeConstraint(MinimumValue = 0)]
		public long Id
		{
			get;
			set;
		}

		[ValueRequired]
		[ValueRangeConstraint(MinimumValue = 0, MaximumValue = 10)]
		public int Age
		{
			get;
			set;
		}

		[ValueRangeConstraint(MinimumValue = 0, MaximumValue = 10000, AllowDefault = true)]
		[DefaultValue(1001)]
		public int? LibraryAccountNo
		{
			get; 
			set;
		}
	}
}
