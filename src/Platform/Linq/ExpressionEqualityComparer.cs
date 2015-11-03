using System.Collections.Generic;
using System.Linq.Expressions;

namespace Platform.Linq
{
	public class ExpressionEqualityComparer
		: IEqualityComparer<Expression>
	{
		private readonly ExpressionComparerOptions options;
		public static readonly ExpressionEqualityComparer Default = new ExpressionEqualityComparer();

		public ExpressionEqualityComparer()
			: this(ExpressionComparerOptions.None)
		{
		}

		public ExpressionEqualityComparer(ExpressionComparerOptions options)
		{
			this.options = options;
		}

		public bool Equals(Expression x, Expression y)
		{
			return ExpressionComparer.Equals(x, y, this.options);
		}

		public int GetHashCode(Expression obj)
		{
			return ExpressionHasher.Hash(obj, this.options);
		}
	}
}