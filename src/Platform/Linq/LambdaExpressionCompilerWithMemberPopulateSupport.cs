using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Linq
{
	public class LambdaExpressionCompilerWithMemberPopulateSupport
		: ExpressionVisitor
	{
		private readonly Type delegateType;
		private readonly ILGenerator generator;
		private readonly DynamicMethod dynamicMethod;
		private readonly LambdaExpression lambdaExpression;
		private readonly Dictionary<ParameterExpression, int> parameterIndexes;

		public LambdaExpressionCompilerWithMemberPopulateSupport(LambdaExpression lambdaExpression)
		{
			this.lambdaExpression = lambdaExpression;

			switch (lambdaExpression.Parameters.Count)
			{
				case 0:
					delegateType = typeof(Func<>);
					break;
				case 1:
					delegateType = typeof(Func<,>);
					break;
				case 2:
					delegateType = typeof(Func<,,>);
					break;
				case 3:
					delegateType = typeof(Func<,,,>);
					break;
				case 4:
					delegateType = typeof(Func<,,,,>);
					break;
				default:
					throw new NotSupportedException("LambdaExpression has too many arguments");
			}

			delegateType = delegateType.MakeGenericType(lambdaExpression.Parameters.Select(c => c.Type).Append(lambdaExpression.Body.Type).ToArray());

			dynamicMethod = new DynamicMethod("", lambdaExpression.Body.Type, lambdaExpression.Parameters.Select(c => c.Type).ToArray(), true);
			generator = dynamicMethod.GetILGenerator();

			parameterIndexes = new Dictionary<ParameterExpression, int>();

			for (int i = 0; i < lambdaExpression.Parameters.Count; i++)
			{
				parameterIndexes[lambdaExpression.Parameters[i]] = i;
			}
		}

		public virtual Delegate Compile()
		{
			this.Visit(this.lambdaExpression.Body);

			Console.WriteLine("Ret");
			generator.Emit(OpCodes.Ret);

			return dynamicMethod.CreateDelegate(delegateType);
		}

		protected override Expression Visit(Expression expression)
		{
			if (expression == null)
			{
				return expression;
			}

			switch (expression.NodeType)
			{
				case (ExpressionType)MemberPopulateExpression.MemberPopulateExpressionType:
					return VisitMemberPopulate((MemberPopulateExpression)expression);
			}

			return base.Visit(expression);
		}

		protected virtual Expression VisitMemberPopulate(MemberPopulateExpression memberPopulateExpression)
		{
			this.Visit(memberPopulateExpression.Source);

			foreach (var memberBinding in memberPopulateExpression.Bindings)
			{
				Console.WriteLine("Dup");
				generator.Emit(OpCodes.Dup);

				this.VisitBinding(memberBinding);
			}

			return memberPopulateExpression;
		}

		public static Delegate Compile(LambdaExpression lambdaExpression)
		{
			var compiler = new LambdaExpressionCompilerWithMemberPopulateSupport(lambdaExpression);

			return compiler.Compile();
		}

		protected override NewExpression VisitNew(NewExpression expression)
		{
			this.VisitExpressionList(expression.Arguments);

			Console.WriteLine("NewObj " + expression.Type.GetConstructor(expression.Arguments.Select(c => c.Type).ToArray()));
			generator.Emit(OpCodes.Newobj, expression.Type.GetConstructor(expression.Arguments.Select(c => c.Type).ToArray()));

			return expression;
		}

		protected override Expression VisitUnary(UnaryExpression unaryExpression)
		{
			if (unaryExpression.NodeType == ExpressionType.Convert)
			{
				this.Visit(unaryExpression.Operand);

				if (unaryExpression.Operand.Type.IsValueType && unaryExpression.Type.IsClass)
				{
					Console.WriteLine("box " + unaryExpression.Operand.Type);
					generator.Emit(OpCodes.Box, unaryExpression.Operand.Type);

					return unaryExpression;
				}

				if (unaryExpression.Type.IsValueType && unaryExpression.Operand.Type.IsClass)
				{
					Console.WriteLine("unbox_any " + unaryExpression.Type);
					generator.Emit(OpCodes.Unbox_Any, unaryExpression.Type);

					return unaryExpression;
				}

				Console.WriteLine("castclass " + unaryExpression.Type);
				generator.Emit(OpCodes.Castclass, unaryExpression.Type);
			}
			else
			{
				throw new NotSupportedException("UnaryExpression: " + unaryExpression.NodeType);
			}

			return unaryExpression;
		}

		protected override Expression VisitMemberInit(System.Linq.Expressions.MemberInitExpression expression)
		{
			this.VisitNew(expression.NewExpression);

			foreach (var memberBinding in expression.Bindings)
			{
				Console.WriteLine("Dup");
				generator.Emit(OpCodes.Dup);

				this.VisitBinding(memberBinding);
			}

			return expression;
		}

		protected override MemberBinding VisitBinding(MemberBinding binding)
		{
			switch (binding.BindingType)
			{
				case MemberBindingType.Assignment:
					return VisitMemberAssignment((MemberAssignment)binding);
				default:
					throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
			}
		}

		protected override Expression VisitParameter(ParameterExpression expression)
		{
			Console.WriteLine("ldarg " + parameterIndexes[expression]);
			generator.Emit(OpCodes.Ldarg, parameterIndexes[expression]);

			return expression;
		}

		protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
		{
			if (!(methodCallExpression.Object == null
				|| (methodCallExpression.NodeType == ExpressionType.Constant && ((ConstantExpression)methodCallExpression.Object).Value == null)))
			{
				this.Visit(methodCallExpression.Object);
			}

			foreach (Expression expression in methodCallExpression.Arguments)
			{
				this.Visit(expression);
			}

			if (methodCallExpression.Method.IsStatic)
			{
				Console.WriteLine("call " + methodCallExpression.Method);
				generator.Emit(OpCodes.Call, methodCallExpression.Method);
			}
			else
			{
				Console.WriteLine("callvirt " + methodCallExpression.Method + " from " + methodCallExpression.Method.DeclaringType);

				generator.Emit(OpCodes.Callvirt, methodCallExpression.Method);
			}

			return methodCallExpression;
		}

		private static readonly MethodInfo TypeGetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(RuntimeTypeHandle) }, null);

		protected override Expression VisitConstant(ConstantExpression constantExpression)
		{
			switch (Type.GetTypeCode(constantExpression.Type))
			{
				case TypeCode.Int16:
					Console.WriteLine("ldc_i4 ");
					generator.Emit(OpCodes.Ldc_I4, (Int16)constantExpression.Value);
					break;
				case TypeCode.Int32:
					Console.WriteLine("ldc_i4 ");
					generator.Emit(OpCodes.Ldc_I4, (Int32)constantExpression.Value);
					break;
				case TypeCode.Int64:
					Console.WriteLine("ldc_i8 ");
					generator.Emit(OpCodes.Ldc_I8, (Int64)constantExpression.Value);
					break;
				case TypeCode.String:
					Console.WriteLine("Ldstr ");
					generator.Emit(OpCodes.Ldstr, (string)constantExpression.Value);
					break;
				case TypeCode.UInt16:
					Console.WriteLine("ldc_i4 ");
					generator.Emit(OpCodes.Ldc_I4, (UInt16)constantExpression.Value);
					break;
				case TypeCode.UInt32:
					Console.WriteLine("ldc_i4 ");
					generator.Emit(OpCodes.Ldc_I4, (UInt32)constantExpression.Value);
					break;
				case TypeCode.UInt64:
					Console.WriteLine("ldc_i8 ");
					generator.Emit(OpCodes.Ldc_I8, (UInt64)constantExpression.Value);
					break;
				case TypeCode.Boolean:
					Console.WriteLine("ldc_i4 ");
					generator.Emit(OpCodes.Ldc_I4, ((bool)constantExpression.Value) ? 1 : 0);
					break;
				case TypeCode.Byte:
					Console.WriteLine("ldc_i4 ");
					generator.Emit(OpCodes.Ldc_I4, (byte)constantExpression.Value);
					break;
				case TypeCode.Char:
					Console.WriteLine("ldc_i4 ");
					generator.Emit(OpCodes.Ldc_I4, (char)constantExpression.Value);
					break;
				case TypeCode.Double:
					Console.WriteLine("ldc_i8 ");
					generator.Emit(OpCodes.Ldc_R8, (double)constantExpression.Value);
					break;
				case TypeCode.Single:
					Console.WriteLine("ldc_r4 ");
					generator.Emit(OpCodes.Ldc_R4, (float)constantExpression.Value);
					break;
				default:
					if (typeof(Type).IsAssignableFrom(constantExpression.Type))
					{
						Console.WriteLine("ldtoken " + ((Type)constantExpression.Value));
						generator.Emit(OpCodes.Ldtoken, ((Type)constantExpression.Value));
						Console.WriteLine("call GetTypeFromHandle");
						generator.Emit(OpCodes.Call, TypeGetTypeFromHandle);

						break;
					}

					if (constantExpression.Value == null)
					{
						Console.WriteLine("ldnull");

						generator.Emit(OpCodes.Ldnull);

						break;
					}

					throw new NotSupportedException("ConstantExpression Type: " + constantExpression.Type);
			}

			return constantExpression;
		}

		protected override Expression VisitBinary(BinaryExpression binaryExpression)
		{
			if (binaryExpression.NodeType == ExpressionType.Equal)
			{
				this.Visit(binaryExpression.Left);
				this.Visit(binaryExpression.Right);

				Console.WriteLine("ceq");
				generator.Emit(OpCodes.Ceq);
			}
			else
			{
				throw new NotSupportedException("BinaryExpression: " + binaryExpression.NodeType);
			}

			return binaryExpression;
		}

		protected override Expression VisitConditional(ConditionalExpression expression)
		{
			var falseLabel = generator.DefineLabel();
			var endLabel = generator.DefineLabel();

			this.Visit(expression.Test);
			Console.WriteLine("br.false");
			generator.Emit(OpCodes.Brfalse, falseLabel);
			this.Visit(expression.IfTrue);
			Console.WriteLine("br");
			generator.Emit(OpCodes.Br, endLabel);
			generator.MarkLabel(falseLabel);
			this.Visit(expression.IfFalse);
			generator.MarkLabel(endLabel);

			return expression;
		}

		protected override Expression VisitMemberAccess(MemberExpression memberExpression)
		{
			this.Visit(memberExpression.Expression);

			switch (memberExpression.Member.MemberType)
			{
				case MemberTypes.Field:
					Console.WriteLine("ldfld " + memberExpression.Member);
					generator.Emit(OpCodes.Ldfld, (FieldInfo)memberExpression.Member);
					break;
				case MemberTypes.Property:
					var method = ((PropertyInfo)memberExpression.Member).GetGetMethod();
					Console.WriteLine("callvirt {0} {1}", method.DeclaringType.Name, method.Name);
					generator.Emit(OpCodes.Callvirt, ((PropertyInfo)memberExpression.Member).GetGetMethod());
					break;
				default:
					throw new NotSupportedException("Unsupported member assignment type: " + memberExpression.Member.MemberType);
			}

			return memberExpression;
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			this.Visit(assignment.Expression);

			switch (assignment.Member.MemberType)
			{
				case MemberTypes.Field:
					Console.WriteLine("stfld " + assignment.Member);
					generator.Emit(OpCodes.Stfld, (FieldInfo)assignment.Member);
					break;
				case MemberTypes.Property:
					var method = ((PropertyInfo)assignment.Member).GetSetMethod();
					Console.WriteLine("callvirt {0} {1}", method.DeclaringType.Name, method.Name);
					generator.Emit(OpCodes.Callvirt, ((PropertyInfo)assignment.Member).GetSetMethod());
					break;
				default:
					throw new NotSupportedException("Unsupported member assignment type: " + assignment.Member.MemberType);
			}

			return assignment;
		}
	}
}
