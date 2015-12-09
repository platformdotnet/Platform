using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Platform.Linq
{
	public class ExpressionHasher
		: ExpressionVisitor
	{
		private int hashCode;
		private readonly ExpressionComparerOptions options;
		private readonly Func<Expression, int> prefilter;

		protected ExpressionHasher(ExpressionComparerOptions options, Func<Expression, int> prefilter)
		{
			this.options = options;
			this.prefilter = prefilter;
		}

		public static int Hash(Expression expression)
		{
			return Hash(expression, ExpressionComparerOptions.None);
		}

		public static int Hash(Expression expression, ExpressionComparerOptions options, Func<Expression, int> prefilter = null)
		{
			var hasher = new ExpressionHasher(options, prefilter);

			hasher.Visit(expression);

			return hasher.hashCode;
		}

		protected override Expression Visit(Expression expression)
		{
			if (expression != null)
			{
				this.hashCode ^= this.prefilter?.Invoke(expression) ?? 0;
				this.hashCode ^= (int)expression.NodeType << 24;
			}

			return base.Visit(expression);
		}

		protected override MemberBinding VisitBinding(MemberBinding binding)
		{
			this.hashCode ^= (int)binding.BindingType;
			this.hashCode ^= binding.Member.GetHashCode();

			return base.VisitBinding(binding);
		}

		protected override ElementInit VisitElementInitializer(ElementInit initializer)
		{
			this.hashCode ^= (int)initializer.Arguments.Count << 16;

			return base.VisitElementInitializer(initializer);
		}

		protected override Expression VisitUnary(UnaryExpression unaryExpression)
		{
			if (unaryExpression.Method != null)
			{
				this.hashCode ^= unaryExpression.Method.GetHashCode();
			}

			return base.VisitUnary(unaryExpression);
		}

		protected override Expression VisitBinary(BinaryExpression binaryExpression)
		{
			if (binaryExpression.Method != null)
			{
				this.hashCode ^= binaryExpression.Method.GetHashCode();
			}

			return base.VisitBinary(binaryExpression);
		}

		protected override Expression VisitTypeIs(TypeBinaryExpression expression)
		{
			this.hashCode ^= expression.TypeOperand.GetHashCode();

			return base.VisitTypeIs(expression);
		}

		protected override Expression VisitConstant(ConstantExpression constantExpression)
		{
			var type = constantExpression.Type;

			this.hashCode ^= type.GetHashCode();

			if ((this.options & ExpressionComparerOptions.IgnoreConstantValues) != 0)
			{
				return constantExpression;
			}

			if (type.IsValueType)
			{
				if (constantExpression.Value != null)
				{
					this.hashCode ^= constantExpression.Value.GetHashCode();
				}
			}

			return constantExpression;
		}

		protected override Expression VisitParameter(ParameterExpression expression)
		{
			this.hashCode ^= expression?.Name.GetHashCode() ?? 0;

			return base.VisitParameter(expression);
		}

		protected override Expression VisitMemberAccess(MemberExpression memberExpression)
		{
			this.hashCode ^= memberExpression.Member.GetHashCode();

			return base.VisitMemberAccess(memberExpression);
		}

		protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
		{
			this.hashCode ^= methodCallExpression.Arguments.Count;

			return base.VisitMethodCall(methodCallExpression);
		}

		protected override IReadOnlyList<T> VisitExpressionList<T>(IReadOnlyList<T> original)
		{
			if (original == null)
			{
				return null;
			}

			this.hashCode ^= original.Count;

			return base.VisitExpressionList<T>(original);
		}

		protected override IReadOnlyList<Expression> VisitExpressionList(IReadOnlyList<Expression> original)
		{
			if (original == null)
			{
				return null;
			}

			this.hashCode ^= original.Count;

			return base.VisitExpressionList(original);
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			this.hashCode ^= (int)assignment.BindingType;
			this.hashCode ^= assignment.Member.GetHashCode();

			return base.VisitMemberAssignment(assignment);
		}

		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			if (binding == null)
			{
				return null;
			}

			this.hashCode ^= binding.Bindings.Count;
			this.hashCode ^= binding.Member.GetHashCode();

			return base.VisitMemberMemberBinding(binding);
		}

		protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			if (binding == null)
			{
				return null;
			}

			this.hashCode ^= (int)binding.BindingType;
			this.hashCode ^= binding.Initializers.Count;
			this.hashCode ^= binding.Member.GetHashCode();

			return base.VisitMemberListBinding(binding);
		}

		protected override IReadOnlyList<MemberBinding> VisitBindingList(IReadOnlyList<MemberBinding> original)
		{
			if (original == null)
			{
				return null;
			}

			this.hashCode ^= original.Count;

			return base.VisitBindingList(original);
		}

		protected override IReadOnlyList<ElementInit> VisitElementInitializerList(IReadOnlyList<ElementInit> original)
		{
			if (original == null)
			{
				return null;
			}

			this.hashCode ^= original.Count;

			return base.VisitElementInitializerList(original);
		}

		protected override Expression VisitNew(NewExpression expression)
		{
			this.hashCode ^= expression.Arguments.Count;

			return base.VisitNew(expression);
		}

		protected override Expression VisitMemberInit(MemberInitExpression expression)
		{
			this.hashCode ^= expression.Bindings.Count << 8;

			return base.VisitMemberInit(expression);
		}

		protected override Expression VisitListInit(ListInitExpression expression)
		{
			this.hashCode ^= expression.Initializers.Count;

			return base.VisitListInit(expression);
		}

		protected override Expression VisitNewArray(NewArrayExpression expression)
		{
			this.hashCode ^= expression.Expressions.Count;

			return base.VisitNewArray(expression);
		}

		protected override Expression VisitInvocation(InvocationExpression expression)
		{
			this.hashCode ^= expression.Arguments.Count << 8;

			return base.VisitInvocation(expression);
		}
	}
}
