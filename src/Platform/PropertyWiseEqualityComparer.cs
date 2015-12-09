using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Platform.Reflection;

namespace Platform
{
	public class PropertyWiseEqualityComparer<T>
		: IEqualityComparer<T>
	{
		private readonly Func<Expression, Expression, Expression> comparerBuilder;
		public static readonly PropertyWiseEqualityComparer<T> Default = new PropertyWiseEqualityComparer<T>();
		public static readonly PropertyWiseEqualityComparer<T> DefaultUsingReferenceEqualty = new PropertyWiseEqualityComparer<T>(Expression.ReferenceEqual);

		private Func<T, T, bool> comparerFunction;

		public PropertyWiseEqualityComparer()
			: this((x, y) => Expression.Call(Expression.Property(null, typeof(EqualityComparer<>).MakeGenericType(x.Type).GetProperty("Default", BindingFlags.Public | BindingFlags.Static)),
								TypeUtils.GetMethod<IEqualityComparer<object>>(c => c.Equals(null, null)).GetMethodFromTypeWithNewGenericArg(x.Type), x, y))
		{
        }

		public PropertyWiseEqualityComparer(Func<Type, object> equalityComparerFromType)
			: this((x, y) => Expression.Call(Expression.Constant(equalityComparerFromType(x.Type)), TypeUtils.GetMethod<IEqualityComparer<object>>(c => c.Equals(null, null)).GetMethodFromTypeWithNewGenericArg(x.Type), x, y))
		{
		}

		public PropertyWiseEqualityComparer(Func<Expression, Expression, Expression> comparerBuilder)
		{
			this.comparerBuilder = comparerBuilder;
		}

		public virtual bool Equals(T x, T y)
		{
			if (comparerFunction == null)
			{
				Expression body = null;
				var left = Expression.Parameter(typeof(T), "left");
				var right = Expression.Parameter(typeof(T), "right");
				
				foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
				{
					var expression = comparerBuilder(Expression.Property(left, property), Expression.Property(right, property));

					body = body == null ? expression : Expression.AndAlso(body, expression);
				}

				if (body != null)
				{
					comparerFunction = Expression.Lambda<Func<T, T, bool>>(body, left, right).Compile();
				}
				else
				{
					comparerFunction = Expression.Lambda<Func<T, T, bool>>(Expression.Constant(true), left, right).Compile();
				}
			}

			return comparerFunction(x, y);
		}

		public virtual int GetHashCode(T obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			return obj.GetHashCode();
		}
	}
}