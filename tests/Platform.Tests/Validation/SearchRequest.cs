using Platform.Validation;

namespace Platform.Tests.Validation
{
	public class SearchRequest
	{
		public int Start
		{
			get;
			set;
		}

		[ValueExpressionConstraint("{value} > {Start}")]
		public int End
		{
			get;
			set;
		}
	}
}
