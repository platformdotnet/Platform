using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Platform.Linq
{
	public class ExpressionComparer
		: ExpressionVisitor
	{
		private bool result;
		private object currentObject;
	    private readonly ExpressionComparerOptions options;
		private readonly Func<Expression, Expression, bool?> prefilter;

		protected ExpressionComparer(Expression toCompareTo, ExpressionComparerOptions options, Func<Expression, Expression, bool?> prefilter)
		{
			this.result = true;
			this.currentObject = toCompareTo;
		    this.options = options;
			this.prefilter = prefilter;
		}

		public static bool Equals(Expression left, Expression right, ExpressionComparerOptions options = ExpressionComparerOptions.None, Func<Expression, Expression, bool?> prefilter = null)
		{
			var visitor = new ExpressionComparer(right, options, prefilter);

			visitor.Visit(left);

			return visitor.result;
		}

		protected override Expression Visit(Expression expression)
		{
			Expression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			if (this.prefilter?.Invoke(expression, current) == false)
			{
				this.result = false;

				return expression;
			}

			return base.Visit(expression);
		}

		protected bool TryGetCurrent<T>(T paramValue, out T current)
			where T : class
		{
			if (paramValue == null && this.currentObject == null)
			{
				current = null;

				return false;
			}

			current = this.currentObject as T;

			if (paramValue == null)
			{
				this.result = false;

				return false;
			}

			if (current == null)
			{
				this.result = false;

				return false;
			}

			return true;
		}

		protected override MemberBinding VisitBinding(MemberBinding binding)
		{
			MemberBinding current;

			if (!this.TryGetCurrent(binding, out current))
			{
				return binding;
			}

			if (current.BindingType != binding.BindingType)
			{
				this.result = false;

				return binding;
			}

			if (current.Member != binding.Member)
			{
				this.result = false;

				return binding;
			}

			return binding;
		}

		protected override ElementInit VisitElementInitializer(ElementInit initializer)
		{
			ElementInit current;

			if (!this.TryGetCurrent(initializer, out current))
			{
				return initializer;
			}

			if (current.Arguments.Count != initializer.Arguments.Count)
			{
				this.result = false;

				return initializer;
			}

			for (var i = 0; i < current.Arguments.Count; i++)
			{
				this.currentObject = current.Arguments[i];
				this.Visit(initializer.Arguments[i]);
			}

			this.currentObject = current;

			return initializer;
		}

		protected override Expression VisitUnary(UnaryExpression unaryExpression)
		{
			UnaryExpression current;

			if (!this.TryGetCurrent(unaryExpression, out current))
			{
				return unaryExpression;
			}

			this.result = this.result && (current.IsLifted == unaryExpression.IsLifted
								&& current.IsLiftedToNull == unaryExpression.IsLiftedToNull
								&& current.Method == unaryExpression.Method);

			if (this.result)
			{
				this.currentObject = current.Operand;

				this.Visit(unaryExpression.Operand);
			}

			return unaryExpression;
		}

		protected override Expression VisitBinary(BinaryExpression binaryExpression)
		{
			BinaryExpression current;

			if (!this.TryGetCurrent(binaryExpression, out current))
			{
				return binaryExpression;
			}

			this.currentObject = current.Left;
			this.Visit(binaryExpression.Left);
			this.currentObject = current.Right;
			this.Visit(binaryExpression.Right);
			this.currentObject = current;

			return binaryExpression;
		}

		protected override Expression VisitTypeIs(TypeBinaryExpression expression)
		{
			TypeBinaryExpression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			this.result = this.result && (current.TypeOperand == expression.TypeOperand);

			if (this.result)
			{
				this.currentObject = current.Expression;

				this.Visit(expression.Expression);

				this.currentObject = current;
			}

			return expression;
		}

		protected override Expression VisitConstant(ConstantExpression constantExpression)
		{
			ConstantExpression current;

			if (!this.TryGetCurrent(constantExpression, out current))
			{
				return constantExpression;
			}

			if ((this.options & ExpressionComparerOptions.IgnoreConstantValues) != 0)
			{
				this.result = constantExpression.Type != current.Type;

				return constantExpression;
			}

			if (!Object.Equals(current.Value, constantExpression.Value))
			{
				this.result = false;
			}

			return constantExpression;
		}

		protected override Expression VisitConditional(ConditionalExpression expression)
		{
			ConditionalExpression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			if (this.result)
			{
				this.currentObject = current.Test;
				this.Visit(expression.Test);
			}

			if (this.result)
			{
				this.currentObject = current.IfTrue;
				this.Visit(expression.IfTrue);
			}

			if (this.result)
			{
				this.currentObject = current.IfFalse;
				this.Visit(expression.IfFalse);
			}

			this.currentObject = expression;

			return expression;
		}

		protected override Expression VisitParameter(ParameterExpression expression)
		{
			ParameterExpression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			this.result = this.result && (current.Name == expression.Name);

			return expression;
		}

		protected override Expression VisitMemberAccess(MemberExpression memberExpression)
		{
			MemberExpression current;

			if (!this.TryGetCurrent(memberExpression, out current))
			{
				return memberExpression;
			}

			this.result = this.result && (current.Member == memberExpression.Member);

			if (this.result)
			{
				this.currentObject = current.Expression;
				this.Visit(memberExpression.Expression);
				this.currentObject = current;
			}

			return memberExpression;
		}

		protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
		{
			MethodCallExpression current;

			if (!this.TryGetCurrent(methodCallExpression, out current))
			{
				return methodCallExpression;
			}

			this.result = this.result && (current.Method == methodCallExpression.Method);

			if (this.result)
			{
				this.currentObject = current.Object;
				this.Visit(methodCallExpression.Object);
				this.currentObject = current;
			}

			if (this.result)
			{
				this.currentObject = current.Arguments;
				this.VisitExpressionList(methodCallExpression.Arguments);
				this.currentObject = current;
			}

			return methodCallExpression;
		}
		
		protected override IReadOnlyList<Expression> VisitExpressionList(IReadOnlyList<Expression> original)
		{
			IReadOnlyList<Expression> current;

			if (!this.TryGetCurrent(original, out current))
			{
				return original;
			}

			if (!(this.result = (current.Count == original.Count)))
			{
			    return original;
			}

			var count = current.Count;

			for (var i = 0; i < count && this.result; i++)
			{
				this.currentObject = current[i];
				this.Visit(original[i]);
			}

			this.currentObject = current;

			return original;
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			MemberAssignment current;

			if (!this.TryGetCurrent(assignment, out current))
			{
				return assignment;
			}

			this.result = this.result && (current.BindingType == assignment.BindingType
								&& current.Member == assignment.Member);

			if (this.result)
			{
				this.currentObject = current.Expression;
				this.Visit(assignment.Expression);
				this.currentObject = current;
			}

			return assignment;
		}

		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			MemberMemberBinding current;

			if (!this.TryGetCurrent(binding, out current))
			{
				return binding;
			}

			this.result = this.result && (current.BindingType == binding.BindingType
								&& current.Member == binding.Member);

			if (this.result)
			{
				this.currentObject = current.Bindings;
				this.VisitBindingList(binding.Bindings);
				this.currentObject = current;
			}

			return binding;
		}

		protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			MemberListBinding current;

			if (!this.TryGetCurrent(binding, out current))
			{
				return binding;
			}

			this.result = this.result && (current.BindingType == binding.BindingType
								&& current.Member == binding.Member);

			if (this.result)
			{
				this.currentObject = current.Initializers;
				this.VisitElementInitializerList(binding.Initializers);
				this.currentObject = current;
			}

			return binding;
		}

		protected override IReadOnlyList<MemberBinding> VisitBindingList(IReadOnlyList<MemberBinding> original)
		{
			IReadOnlyList<MemberBinding> current;

			if (!this.TryGetCurrent(original, out current))
			{
				return original;
			}

			if (!(this.result = this.result && (current.Count == original.Count)))
			{
				return original;
			}

			var count = original.Count;

			for (var i = 0; i < count && this.result; i++)
			{
				this.currentObject = current[i];
				this.VisitBinding(original[i]);
			}

			this.currentObject = current;

			return original;
		}

		protected override IReadOnlyList<ElementInit> VisitElementInitializerList(IReadOnlyList<ElementInit> original)
		{
			IReadOnlyList<ElementInit> current;

			if (!this.TryGetCurrent(original, out current))
			{
				return original;
			}

			if (!(this.result = this.result && (current.Count == original.Count)))
			{
				return original;
			}

			var count = original.Count;

			for (var i = 0; i < count && this.result; i++)
			{
				this.currentObject = current[i];
				this.VisitElementInitializer(original[i]);
			}

			this.currentObject = current;
			
			return original;
		}

		protected override Expression VisitLambda(LambdaExpression expression)
		{
			LambdaExpression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			if (!(this.result = this.result && (current.Parameters.Count == expression.Parameters.Count)))
			{
				return expression;
			}

			this.currentObject = current.Body;
			this.Visit(expression.Body);

			if (!this.result)
			{
				return expression;
			}

			var count = expression.Parameters.Count;

			for (var i = 0; i < count && this.result; i++)
			{
				this.currentObject = current.Parameters[i];
				this.Visit(expression.Parameters[i]);
			}

			this.currentObject = current;
			
			return expression;
		}

		protected override Expression VisitNew(NewExpression expression)
		{
			NewExpression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			if (!(this.result = this.result && (current.Constructor == expression.Constructor
								&& current.Arguments.Count == expression.Arguments.Count)))
			{
				return expression;
			}

			var count = expression.Arguments.Count;

            for (var i = 0; i < count && this.result; i++)
            {
	            this.currentObject = current.Arguments[i];
	            this.Visit(expression.Arguments[i]);
			}

			this.currentObject = current;

			return expression;
		}

		protected override Expression VisitMemberInit(MemberInitExpression expression)
		{
			MemberInitExpression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			if (!(this.result = this.result && (current.Bindings.Count == expression.Bindings.Count)))
			{
				return expression;
			}

			this.currentObject = current.NewExpression;

			this.Visit(expression.NewExpression);

			if (this.result)
			{
				this.currentObject = current.Bindings;
				this.VisitBindingList(expression.Bindings);
			}

			this.currentObject = current;

			return expression;
		}

		protected override Expression VisitListInit(ListInitExpression expression)
		{
			ListInitExpression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			this.result = this.result && (current.Initializers.Count == expression.Initializers.Count);

			if (this.result)
			{
				this.currentObject = current.Initializers;
				this.VisitElementInitializerList(expression.Initializers);
				this.currentObject = current;
			}

			return expression;
		}

		protected override Expression VisitNewArray(NewArrayExpression expression)
		{
			NewArrayExpression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			this.result = this.result && (current.Expressions.Count == expression.Expressions.Count);

			if (this.result)
			{
				this.currentObject = current.Expressions;
				this.VisitExpressionList(expression.Expressions);
				this.currentObject = current;
			}

			return expression;
		}

		protected override Expression VisitInvocation(InvocationExpression expression)
		{
			InvocationExpression current;

			if (!this.TryGetCurrent(expression, out current))
			{
				return expression;
			}

			this.result = this.result && (current.Arguments.Count == expression.Arguments.Count);

			if (this.result)
			{
				this.currentObject = current.Expression;
				this.Visit(expression.Expression);
			}

			return expression;
		}
	}
}
