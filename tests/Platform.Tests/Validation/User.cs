using System.Collections.Generic;
using Platform.Validation;

namespace Platform.Tests.Validation
{
	public class User
	{
		[DefaultValue(Value = "Fernando")]
		[ValuePatternConstraint("^[A-Za-z]+$", AllowDefault = true)]
		public string Name
		{
			get;
			set;
		}

		[ValueRequired]
		public List<string> Tags
		{
			get;
			set;
		}

		[ValueRequired]
		[ValueRangeConstraint(MinimumValue = 0, AllowDefault = false)]
		public decimal? HairLength
		{
			get;
			set;
		}

		[ValueRangeConstraint(MinimumValue = 1, AllowDefault = true)]
		public long? Length
		{
			get;
			set;
		}
	}
}
