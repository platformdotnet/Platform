using Platform.Validation;

namespace Platform.Tests.Validation
{
	public class PageRequest
	{
		[ValueRangeConstraint(MinimumValue = 0, AllowDefault = true)]
		public long UserId
		{
			get;
			set;
		}

		[ValueRangeConstraint(MinimumValue = 0)]
		public int Skip
		{
			get;
			set;
		}

		[DefaultValue(20)]
		[ValueRangeConstraint(MinimumValue = 0)]
		public int Take
		{
			get;
			set;
		}
	}
}