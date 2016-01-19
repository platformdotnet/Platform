using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Platform.Linq
{
	public abstract class ExpressionVisitor
	{
		protected virtual Expression Visit(Expression expression)
		{
			if (expression == null)
			{
				return null;
			}

			switch (expression.NodeType)
			{
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
			case ExpressionType.Not:
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
			case ExpressionType.ArrayLength:
			case ExpressionType.Quote:
			case ExpressionType.TypeAs:
				return VisitUnary((UnaryExpression)expression);
			case ExpressionType.Add:
			case ExpressionType.AddChecked:
			case ExpressionType.Subtract:
			case ExpressionType.SubtractChecked:
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyChecked:
			case ExpressionType.Divide:
			case ExpressionType.Modulo:
			case ExpressionType.And:
			case ExpressionType.AndAlso:
			case ExpressionType.Or:
			case ExpressionType.OrElse:
			case ExpressionType.LessThan:
			case ExpressionType.LessThanOrEqual:
			case ExpressionType.GreaterThan:
			case ExpressionType.GreaterThanOrEqual:
			case ExpressionType.Equal:
			case ExpressionType.NotEqual:
			case ExpressionType.Coalesce:
			case ExpressionType.ArrayIndex:
			case ExpressionType.RightShift:
			case ExpressionType.LeftShift:
			case ExpressionType.ExclusiveOr:
				return VisitBinary((BinaryExpression)expression);
			case ExpressionType.TypeIs:
				return VisitTypeIs((TypeBinaryExpression)expression);
			case ExpressionType.Conditional:
				return VisitConditional((ConditionalExpression)expression);
			case ExpressionType.Constant:
				return VisitConstant((ConstantExpression)expression);
			case ExpressionType.Parameter:
				return VisitParameter((ParameterExpression)expression);
			case ExpressionType.MemberAccess:
				return VisitMemberAccess((MemberExpression)expression);
			case ExpressionType.Call:
				return VisitMethodCall((MethodCallExpression)expression);
			case ExpressionType.Lambda:
				return VisitLambda((LambdaExpression)expression);
			case ExpressionType.New:
				return VisitNew((NewExpression)expression);
			case ExpressionType.NewArrayInit:
			case ExpressionType.NewArrayBounds:
				return VisitNewArray((NewArrayExpression)expression);
			case ExpressionType.Invoke:
				return VisitInvocation((InvocationExpression)expression);
			case ExpressionType.MemberInit:
				return VisitMemberInit((MemberInitExpression)expression);
			case ExpressionType.ListInit:
				return VisitListInit((ListInitExpression)expression);
			case ExpressionType.Extension:
				return VisitExtension(expression);
			case ExpressionType.Block:
				return this.VisitBlock((BlockExpression)expression);
			case ExpressionType.Assign:
				return this.VisitBinary((BinaryExpression)expression);
			case ExpressionType.Goto:
				return this.VisitGoto((GotoExpression)expression);
			case ExpressionType.Label:
				return this.VisitLabel((LabelExpression)expression);
			default:
				throw new Exception($"Unhandled expression type: '{expression.NodeType}'");
			}
		}

		protected virtual Expression VisitLabel(LabelExpression labelExpression)
		{
			var defaultValue = this.Visit(labelExpression.DefaultValue);

			if (defaultValue != labelExpression.DefaultValue)
			{
				return Expression.Label(labelExpression.Target, defaultValue);
			}

			return labelExpression;
		}

		protected virtual Expression VisitGoto(GotoExpression gotoExpression)
		{
			var value = this.Visit(gotoExpression.Value);

			if (value != gotoExpression.Value)
			{
				return Expression.Goto(gotoExpression.Target, value);
			}

			return gotoExpression;
		}

		protected virtual Expression VisitBlock(BlockExpression blockExpression)
		{
			var expressionList = this.VisitExpressionList(blockExpression.Expressions);

			if (expressionList != blockExpression.Expressions)
			{
				return Expression.Block(blockExpression.Variables, expressionList);
			}

			return blockExpression;
		}

		protected virtual Expression VisitExtension(Expression expression)
		{
			return expression;
		}

		protected virtual MemberBinding VisitBinding(MemberBinding binding)
		{
			switch (binding.BindingType)
			{
			case MemberBindingType.Assignment:
				return VisitMemberAssignment((MemberAssignment)binding);
			case MemberBindingType.MemberBinding:
				return VisitMemberMemberBinding((MemberMemberBinding)binding);
			case MemberBindingType.ListBinding:
				return VisitMemberListBinding((MemberListBinding)binding);
			default:
				throw new Exception($"Unhandled binding type '{binding.BindingType}'");
			}
		}

		protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
		{
			var arguments = VisitExpressionList(initializer.Arguments);

			if (arguments != initializer.Arguments)
			{
				return Expression.ElementInit(initializer.AddMethod, arguments);
			}

			return initializer;
		}

		protected virtual Expression VisitUnary(UnaryExpression unaryExpression)
		{
			var operand = Visit(unaryExpression.Operand);

			if (operand != unaryExpression.Operand)
			{
				return Expression.MakeUnary(unaryExpression.NodeType, operand, unaryExpression.Type, unaryExpression.Method);
			}

			return unaryExpression;
		}

		protected virtual Expression VisitBinary(BinaryExpression binaryExpression)
		{
			var left = Visit(binaryExpression.Left);
			var right = Visit(binaryExpression.Right);
			var conversion = Visit(binaryExpression.Conversion);

			if (left != binaryExpression.Left || right != binaryExpression.Right || conversion != binaryExpression.Conversion)
			{
				if (binaryExpression.NodeType == ExpressionType.Coalesce && binaryExpression.Conversion != null)
				{
					return Expression.Coalesce(left, right, conversion as LambdaExpression);
				}
				else
				{
					return Expression.MakeBinary(binaryExpression.NodeType, left, right, binaryExpression.IsLiftedToNull, binaryExpression.Method);
				}
			}

			return binaryExpression;
		}

		protected virtual Expression VisitTypeIs(TypeBinaryExpression expression)
		{
			var expr = Visit(expression.Expression);

			if (expr != expression.Expression)
			{
				return Expression.TypeIs(expr, expression.TypeOperand);
			}

			return expression;
		}

		protected virtual Expression VisitConstant(ConstantExpression constantExpression)
		{
			return constantExpression;
		}

		protected virtual Expression VisitConditional(ConditionalExpression expression)
		{
			var test = Visit(expression.Test);
			var ifTrue = Visit(expression.IfTrue);
			var ifFalse = Visit(expression.IfFalse);

			if (test != expression.Test || ifTrue != expression.IfTrue || ifFalse != expression.IfFalse)
			{
				return Expression.Condition(test, ifTrue, ifFalse);
			}

			return expression;
		}

		protected virtual Expression VisitParameter(ParameterExpression expression)
		{
			return expression;
		}

		protected virtual Expression VisitMemberAccess(MemberExpression memberExpression)
		{
			var exp = Visit(memberExpression.Expression);

			if (exp != memberExpression.Expression)
			{
				return Expression.MakeMemberAccess(exp, memberExpression.Member);
			}

			return memberExpression;
		}

		protected virtual Expression VisitMethodCall(MethodCallExpression methodCallExpression)
		{
			var obj = Visit(methodCallExpression.Object);

			IEnumerable<Expression> args = VisitExpressionList(methodCallExpression.Arguments);

			if (obj != methodCallExpression.Object || args != methodCallExpression.Arguments)
			{
				return Expression.Call(obj, methodCallExpression.Method, args);
			}

			return methodCallExpression;
		}

		protected virtual IReadOnlyList<Expression> VisitExpressionList(IReadOnlyList<Expression> original)
		{
			return this.VisitExpressionList<Expression>(original);
		}

		protected virtual IReadOnlyList<T> VisitExpressionList<T>(IReadOnlyList<T> original)
			where T : Expression
		{
			List<T> list = null;

			if (original == null)
			{
				return null;
			}

			var count = original.Count;

			for (var i = 0; i < count; i++)
			{
				var item = original[i];

				var p = (T)Visit(item);

				if (list != null)
				{
					if (p != null)
					{
						list.Add(p);
					}
				}
				else if (p != item)
				{
					list = new List<T>(count);

					for (var j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}

					if (p != null)
					{
						list.Add(p);
					}
				}
			}

			return list?.AsReadOnly() ?? original;
		}

		protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			var e = Visit(assignment.Expression);

			return e != assignment.Expression ? Expression.Bind(assignment.Member, e) : assignment;
		}

		protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			var bindings = VisitBindingList(binding.Bindings);

			return bindings != binding.Bindings ? Expression.MemberBind(binding.Member, bindings) : binding;
		}

		protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			var initializers = VisitElementInitializerList(binding.Initializers);

			return initializers != binding.Initializers ? Expression.ListBind(binding.Member, initializers) : binding;
		}

		protected virtual IReadOnlyList<MemberBinding> VisitBindingList(IReadOnlyList<MemberBinding> original)
		{
			List<MemberBinding> list = null;

			var count = original.Count;

			for (var i = 0; i < count; i++)
			{
				var item = original[i];

				var b = VisitBinding(item);

				if (list != null)
				{
					list.Add(b);
				}
				else if (b != item)
				{
					list = new List<MemberBinding>(count);

					for (var j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}

					list.Add(b);
				}
			}

			return list ?? original;
		}

		protected virtual IReadOnlyList<ElementInit> VisitElementInitializerList(IReadOnlyList<ElementInit> original)
		{
			List<ElementInit> list = null;

			var count = original.Count;

			for (var i = 0; i < count; i++)
			{
				var item = original[i];
				var init = VisitElementInitializer(item);

				if (list != null)
				{
					list.Add(init);
				}
				else if (init != item)
				{
					list = new List<ElementInit>(count);

					for (var j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}

					list.Add(init);
				}
			}

			return list ?? original;
		}

		protected virtual Expression VisitLambda(LambdaExpression expression)
		{
			var body = Visit(expression.Body);

			return body != expression.Body ? Expression.Lambda(expression.Type, body, expression.Parameters) : expression;
		}

		protected virtual Expression VisitNew(NewExpression expression)
		{
			var args = VisitExpressionList(expression.Arguments);

			if (args != expression.Arguments)
			{
				if (expression.Members != null)
				{
					return Expression.New(expression.Constructor, args, expression.Members);
				}

				return Expression.New(expression.Constructor, args);
			}

			return expression;
		}

		protected virtual Expression VisitMemberInit(MemberInitExpression expression)
		{
			var n = VisitNew(expression.NewExpression);

			var bindings = VisitBindingList(expression.Bindings);

			if (n != expression.NewExpression || bindings != expression.Bindings)
			{
				return Expression.MemberInit((NewExpression)n, bindings);
			}

			return expression;
		}

		protected virtual Expression VisitListInit(ListInitExpression expression)
		{
			var n = VisitNew(expression.NewExpression);
			var initializers = VisitElementInitializerList(expression.Initializers);

			if (n != expression.NewExpression || initializers != expression.Initializers)
			{
				return Expression.ListInit((NewExpression)n, initializers);
			}

			return expression;
		}

		protected virtual Expression VisitNewArray(NewArrayExpression expression)
		{
			var exprs = VisitExpressionList(expression.Expressions);

			if (exprs != expression.Expressions)
			{
				if (expression.NodeType == ExpressionType.NewArrayInit)
				{
					return Expression.NewArrayInit(expression.Type.GetElementType(), exprs);
				}

				return Expression.NewArrayBounds(expression.Type.GetElementType(), exprs);
			}

			return expression;
		}

		protected virtual Expression VisitInvocation(InvocationExpression expression)
		{
			var args = VisitExpressionList(expression.Arguments);

			var expr = Visit(expression.Expression);

			if (args != expression.Arguments || expr != expression.Expression)
			{
				return Expression.Invoke(expr, args);
			}

			return expression;
		}
	}
}