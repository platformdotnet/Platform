using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Platform.Reflection;

namespace Platform.Validation.Validators
{
	public class ExpressionTreeBasedValidator<T>
		: Validator<T>
	{
		private Func<ValidationContext<T>, ValidationResult> ValidationFunc;

		public ExpressionTreeBasedValidator(ValidatorOptions validatorOptions)
			: base(validatorOptions)
		{
			BuildValidator();
		}

		private void BuildValidator()
		{
			Expression body = null;
			var delegates = new List<Delegate>();
			var propertyInfos = new List<PropertyInfo>();
			var defaultAttributes = new List<DefaultValueAttribute>();
			var validationContext = Expression.Parameter(typeof(ValidationContext<T>), "validationContext");
			var currentObject = Expression.Property(validationContext, typeof(ValidationContext<T>).GetProperty("ObjectValue"));
			
			foreach (var propertyInfo in GetProperties(typeof(T)))
			{
				var attributes = (ValidationAttribute[])propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), true);

				if (attributes.Length != 0)
				{
					var propertyValue = Expression.Property(currentObject, propertyInfo.GetGetMethod());

					foreach (var attribute in attributes)
					{
						var propertyValidationContext = Expression.New
						(
							typeof(PropertyValidationContext<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType).GetConstructor
							(
								new[]
								 {
									validationContext.Type, typeof(PropertyInfo), typeof(BaseValidationAttribute), propertyInfo.PropertyType
								 }
							),
							validationContext,
							Expression.Constant(propertyInfo),
							Expression.Constant(attribute),
							propertyValue
						);

						MethodCallExpression methodCallExpression;
						var methodInfo = attribute.GetType().GetMethod("GetValidateExpression");
						var realisedMethodInfo = methodInfo.MakeGenericMethod(typeof(T), propertyInfo.PropertyType);

						var expression = (LambdaExpression)realisedMethodInfo.Invoke(attribute, null);

						if (expression != null)
						{
							var compiledLambda = expression.Compile();

							methodCallExpression = Expression.Call(Expression.Constant(compiledLambda), compiledLambda.GetType().GetMethod("Invoke"), propertyValidationContext);
						}
						else
						{
							methodCallExpression = Expression.Call(Expression.Constant(attribute), attribute.GetType().GetMethod("Validate").MakeGenericMethod(typeof(T), propertyInfo.PropertyType), propertyValidationContext);
						}

						if (body == null)
						{
							body = methodCallExpression;
						}
						else
						{
							body = Expression.Call(body, typeof(ValidationResult).GetMethod("MergeWith"), methodCallExpression);
						}
					}
				}
			}

			var endLabel = new Label();
			ILGenerator generator = null;
			DynamicMethod setDefaultsDynamicMethod = null;
			var validationContextInitiased = false;

			foreach (var propertyInfo in GetProperties(typeof(T)))
			{
				var defaultValueAttribute = propertyInfo.GetFirstCustomAttribute<DefaultValueAttribute>(true);
				
				var propertyValue = Expression.Property(currentObject, propertyInfo.GetGetMethod());
				
				propertyInfos.Add(propertyInfo);

				if (defaultValueAttribute != null)
				{
					var unsetValueAttribute = propertyInfo.GetFirstCustomAttribute<UnsetValueAttribute>(true);
				
					defaultAttributes.Add(defaultValueAttribute);

					var propertyValidationContext = Expression.New
					(
						typeof(PropertyValidationContext<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType).GetConstructor
						(
							new[]
						 {
							validationContext.Type, typeof(PropertyInfo), typeof(ValidationAttribute), propertyInfo.PropertyType
						 }
						),
						validationContext,
						Expression.Constant(propertyInfo),
						Expression.Constant(defaultValueAttribute),
						propertyValue
					);

					if (setDefaultsDynamicMethod == null)
					{
						setDefaultsDynamicMethod = new DynamicMethod("SetDefaults", typeof(ValidationResult), new[] { typeof(ValidationResult), typeof(T), typeof(List<Delegate>), typeof(List<PropertyInfo>), typeof(List<DefaultValueAttribute>), typeof(ValidationContext<T>) });
						generator = setDefaultsDynamicMethod.GetILGenerator();
						endLabel = generator.DefineLabel();
					}

					var propertyValidationContextLocal = generator.DeclareLocal(propertyValidationContext.Type);

					var initPropertyValidationContext = (Action)
						delegate
						{
							if (validationContextInitiased)
							{
								return;
							}

							validationContextInitiased = true;

							generator.Emit(OpCodes.Ldloca, propertyValidationContextLocal);

							var constructor = typeof(PropertyValidationContext<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType).GetConstructor
							(
								new[]
							 {
								validationContext.Type, typeof(PropertyInfo), typeof(ValidationAttribute), propertyInfo.PropertyType
							 }
							);

							// Load ValidationContext
							generator.Emit(OpCodes.Ldarg_S, 5);

							// Load PropertyInfo
							generator.Emit(OpCodes.Ldarg_3);
							generator.Emit(OpCodes.Ldc_I4, propertyInfos.Count - 1);
							generator.Emit(OpCodes.Callvirt, typeof(List<PropertyInfo>).GetMethod("get_Item"));

							// Load ValidationAttribute
							generator.Emit(OpCodes.Ldarg_S, 4);
							generator.Emit(OpCodes.Ldc_I4, defaultAttributes.Count - 1);
							generator.Emit(OpCodes.Callvirt, typeof(List<DefaultValueAttribute>).GetMethod("get_Item"));

							// Load Value
							generator.Emit(OpCodes.Ldarg_1);
							generator.Emit(OpCodes.Call, propertyInfo.GetGetMethod());

							// Call constructor
							generator.Emit(OpCodes.Call, constructor);
						};

					generator.Emit(OpCodes.Ldarga, 0);
					generator.Emit(OpCodes.Call, typeof(ValidationResult).GetProperty("IsSuccess").GetGetMethod());
					generator.Emit(OpCodes.Brfalse, endLabel);

					var label = generator.DefineLabel();

					generator.Emit(OpCodes.Ldarg_1);
					generator.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod());

					var valueLocal = generator.DeclareLocal(propertyInfo.PropertyType);
					var currentPropertyValueLocal = generator.DeclareLocal(propertyInfo.PropertyType);

					generator.Emit(OpCodes.Stloc, currentPropertyValueLocal);
					
					if (unsetValueAttribute != null && unsetValueAttribute.ValueExpression != null)
					{
						initPropertyValidationContext();

						var unsetValueDelegate = GetDelegateFromExpression(unsetValueAttribute.ValueExpression, propertyValidationContext);

						delegates.Add(unsetValueDelegate);

						generator.Emit(OpCodes.Ldarg_2);
						generator.Emit(OpCodes.Ldc_I4, delegates.Count - 1);
						generator.Emit(OpCodes.Callvirt, typeof(List<Delegate>).GetMethod("get_Item"));
						generator.Emit(OpCodes.Castclass, unsetValueDelegate.GetType());
						
						generator.Emit(OpCodes.Ldloc, propertyValidationContextLocal);
						generator.Emit(OpCodes.Callvirt, unsetValueDelegate.GetType().GetMethod("Invoke"));

						if (unsetValueDelegate.Method.ReturnType != propertyInfo.PropertyType)
						{
							if (unsetValueDelegate.Method.ReturnType.IsValueType)
							{
								generator.Emit(OpCodes.Box, propertyInfo.PropertyType);
							}

							generator.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
							generator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static));
							generator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object), typeof(Type) }, null));

							if (unsetValueDelegate.Method.ReturnType.IsValueType)
							{
								generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
							}
							else
							{
								generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
							}
						}

						generator.Emit(OpCodes.Stloc, valueLocal
							);

					}
					else if (unsetValueAttribute != null && unsetValueAttribute.ValueExpression == null)
					{
						LoadValue(generator, propertyInfo.PropertyType, unsetValueAttribute.Value);
						generator.Emit(OpCodes.Stloc, valueLocal);
					}
					else
					{
						if (propertyInfo.PropertyType == typeof(string))
						{
							generator.Emit(OpCodes.Ldnull);
							generator.Emit(OpCodes.Stloc, valueLocal);
						}
						else if (propertyInfo.PropertyType.IsValueType)
						{
							generator.Emit(OpCodes.Ldloca, valueLocal);
							generator.Emit(OpCodes.Initobj, propertyInfo.PropertyType);
						}
						else
						{
							generator.Emit(OpCodes.Ldnull);
							generator.Emit(OpCodes.Stloc, valueLocal);
						}
					}

					if (propertyInfo.PropertyType.IsValueType)
					{
						if (propertyInfo.PropertyType.IsPrimitive)
						{
							generator.Emit(OpCodes.Ldloc, currentPropertyValueLocal);
							generator.Emit(OpCodes.Ldloc, valueLocal);

							generator.Emit(OpCodes.Ceq);
						}
						else
						{
							generator.Emit(OpCodes.Ldloc, currentPropertyValueLocal);
							generator.Emit(OpCodes.Box, currentPropertyValueLocal.LocalType);
							generator.Emit(OpCodes.Ldloc, valueLocal);
							generator.Emit(OpCodes.Box, valueLocal.LocalType);
							generator.Emit(OpCodes.Call, typeof(Object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public, null, new [] { typeof(Object), typeof(Object) }, null));
						}
					}
					else if (propertyInfo.PropertyType == typeof(string) && unsetValueAttribute == null)
					{
						generator.Emit(OpCodes.Ldloc, currentPropertyValueLocal);
						generator.Emit(OpCodes.Call, typeof(String).GetMethod("IsNullOrEmpty", BindingFlags.Public | BindingFlags.Static));
					}
					else
					{
						generator.Emit(OpCodes.Ldloc, currentPropertyValueLocal);
						generator.Emit(OpCodes.Ldloc, valueLocal);
						generator.Emit(OpCodes.Ceq);
					}

					generator.Emit(OpCodes.Brfalse, label);

					generator.Emit(OpCodes.Ldarg_1);

					if (defaultValueAttribute.ValueExpression == null)
					{
						LoadValue(generator, propertyInfo.PropertyType, defaultValueAttribute.Value);
					}
					else
					{
						var defaultValueDelegate = GetDelegateFromExpression(defaultValueAttribute.ValueExpression, propertyValidationContext);
						
						delegates.Add(defaultValueDelegate);

						generator.Emit(OpCodes.Ldarg_2);
						generator.Emit(OpCodes.Ldc_I4, delegates.Count - 1);
						generator.Emit(OpCodes.Callvirt, typeof(List<Delegate>).GetMethod("get_Item"));
						generator.Emit(OpCodes.Castclass, defaultValueDelegate.GetType());

						initPropertyValidationContext();
						
						// Load PropertyValidationContext
						generator.Emit(OpCodes.Ldloc, propertyValidationContextLocal);

						generator.Emit(OpCodes.Callvirt, defaultValueDelegate.GetType().GetMethod("Invoke"));
						
						if (defaultValueDelegate.Method.ReturnType != propertyInfo.PropertyType)
						{
							generator.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
							generator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static));
							generator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", BindingFlags.Static | BindingFlags.Public));
							generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
						}
					}

					generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());

					generator.MarkLabel(label);
				}
			}
			
			// Create "SetDefaults" method and add it to body
			if (setDefaultsDynamicMethod != null)
			{
				generator.MarkLabel(endLabel);
				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Ret);

				var setDefaultsFunc = setDefaultsDynamicMethod.CreateDelegate(typeof(Func<,,,,,,>).MakeGenericType(typeof(ValidationResult), typeof(T), typeof(List<Delegate>), typeof(List<PropertyInfo>), typeof(List<DefaultValueAttribute>), typeof(ValidationContext<T>), typeof(ValidationResult)));

				body = Expression.Call(Expression.Constant(setDefaultsFunc), setDefaultsFunc.GetType().GetMethod("Invoke"), body ?? Expression.Constant(ValidationResult.Success), currentObject, Expression.Constant(delegates), Expression.Constant(propertyInfos), Expression.Constant(defaultAttributes), validationContext);
			}
			
			if (body == null)
			{
				body = Expression.Constant(ValidationResult.Success);
			}

			var lambdaExpression = Expression.Lambda(body, validationContext);

			this.ValidationFunc = (Func<ValidationContext<T>, ValidationResult>)lambdaExpression.Compile();
		}

		public Delegate GetDelegateFromExpression(string expressionString, Expression propertyValidationContextExpression)
		{
			var parameter = Expression.Parameter(propertyValidationContextExpression.Type, "propertyValidationContext");

			var body = StringExpressionParser.Parse(parameter, expressionString);

			var lambdaExpression = Expression.Lambda(typeof(Func<,>).MakeGenericType(parameter.Type, body.Type), body, parameter).Compile();

			return lambdaExpression;
		}

		public static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			foreach (var parentType in type.WalkHierarchy(true, true).Prepend(type))
			{
				foreach (var propertyInfo in parentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					yield return propertyInfo;
				}
			}
		}
		
		public override ValidationResult Validate(T value)
		{
			return this.ValidationFunc(new ValidationContext<T>(value));
		}

		private void LoadValue(ILGenerator generator, Type type, object value)
		{
			Type nullableType = null;
			var underlyingType = Nullable.GetUnderlyingType(type);
			
			if (underlyingType != null)
			{
				nullableType = type;
				type = underlyingType;
			}

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean:
					generator.Emit(OpCodes.Ldc_I4, (bool)Convert.ChangeType(value, typeof(bool)) ? 1 : 0);
					break;
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
					generator.Emit(OpCodes.Ldc_I4, (int)Convert.ChangeType(value, typeof(Int32)));
					break;
				case TypeCode.Int64:
				case TypeCode.UInt64:
					generator.Emit(OpCodes.Ldc_I8, (long)Convert.ChangeType(value, typeof(Int64)));
					break;
				case TypeCode.Single:
					generator.Emit(OpCodes.Ldc_R4, (float)Convert.ChangeType(value, typeof(Single)));
					break;
				case TypeCode.String:
					generator.Emit(OpCodes.Ldstr, (string)Convert.ChangeType(value, typeof(string)));
					break;
				case TypeCode.Double:
					generator.Emit(OpCodes.Ldc_R8, (double)Convert.ChangeType(value, typeof(double)));
					break;
				case TypeCode.Decimal:
					generator.Emit(OpCodes.Ldc_R8, (double)Convert.ChangeType(value, typeof(double)));
					generator.Emit(OpCodes.Newobj, typeof(Decimal).GetConstructor(new[] { typeof(Double) }));
					break;
				default:
					throw new NotSupportedException("DefaultValue for property of type: " + type);
			}

			if (nullableType != null)
			{
				var constructorInfo = nullableType.GetConstructor(new[] { type });
				generator.Emit(OpCodes.Newobj, constructorInfo);
			}
		}
	}
}
