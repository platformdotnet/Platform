using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Platform.Validation.Validators;

namespace Platform.Validation
{
	public abstract class ValidatorFactory
	{
		protected struct ValidatorKey
		{
			public Type validatorType;
			public ValidatorOptions validatorOptions;

			public ValidatorKey(Type validatorType, ValidatorOptions options)
			{
				this.validatorType = validatorType;
				this.validatorOptions = options;
			}
		}

		protected class ValidatorKeyEqualityComparer
			: IEqualityComparer<ValidatorKey>
		{
			public static readonly ValidatorKeyEqualityComparer Default = new ValidatorKeyEqualityComparer();

			public bool Equals(ValidatorKey x, ValidatorKey y)
			{
				return x.validatorType == y.validatorType
				       && (x.validatorOptions == y.validatorOptions || x.validatorOptions.Equals(y.validatorOptions));
			}

			public int GetHashCode(ValidatorKey obj)
			{
				return obj.validatorType.GetHashCode() ^ obj.validatorOptions.GetHashCode();
			}
		}

		public static readonly ValidatorFactory Default = new ExpressionTreeBasedValidatorFactory();

		private static Dictionary<Type, Func<ValidatorOptions, IValidator>> validatorConstructorFunctions = new Dictionary<Type, Func<ValidatorOptions, IValidator>>();

		public virtual IValidator NewValidator(Type type, ValidatorOptions options)
		{
			Func<ValidatorOptions, IValidator> result;

			if (!validatorConstructorFunctions.TryGetValue(type, out result))
			{
				var parameter = Expression.Parameter(typeof(ValidatorOptions), "options");
				var methodInfo = typeof(ValidatorFactory).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(c => c.GetGenericArguments().Length == 1).MakeGenericMethod(type);
				var body = Expression.Call(Expression.Constant(this), methodInfo, parameter);

				result = Expression.Lambda<Func<ValidatorOptions, IValidator>>(body, parameter).Compile();

				var newValidatorConstructors = new Dictionary<Type, Func<ValidatorOptions, IValidator>>(validatorConstructorFunctions);

				newValidatorConstructors[type] = result;
				validatorConstructorFunctions = newValidatorConstructors;
			}

			return result(options);
		}

		public virtual Validator<T> NewValidator<T>(ValidatorOptions options)
		{
			var key = new ValidatorKey()
			          {
			          	validatorType = typeof(T),
			          	validatorOptions = options
			          };

			Validator<T> retval;
            
			if (!ValidatorCacheContainer<T>.validators.TryGetValue(key, out retval))
			{
				retval = DoNewValidator<T>(options);

				var newValidators = new Dictionary<ValidatorKey ,Validator<T>>();

				foreach (var keyValue in ValidatorCacheContainer<T>.validators)
				{
					newValidators[keyValue.Key] = keyValue.Value;
				}

				newValidators[key] = retval;

				ValidatorCacheContainer<T>.validators = newValidators;
			}

			return retval;
		}

		protected abstract Validator<T> DoNewValidator<T>(ValidatorOptions options);

		protected class ValidatorCacheContainer<T>
		{
			public static IDictionary<ValidatorKey, Validator<T>> validators = new Dictionary<ValidatorKey, Validator<T>>(ValidatorKeyEqualityComparer.Default);
		}
	}
}
