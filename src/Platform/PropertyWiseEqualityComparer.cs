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
		private readonly Func<Type, object> equalityComparerFromType;
		public static readonly PropertyWiseEqualityComparer<T> Default = new PropertyWiseEqualityComparer<T>();
		public static readonly PropertyWiseEqualityComparer<T> DefaultUsingReferenceEqualty = new PropertyWiseEqualityComparer<T>(c => typeof(ObjectReferenceIdentityEqualityComparer<>).MakeGenericType(c).GetProperty("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null, null));

		private Func<T, T, bool> comparerFunction;

		public PropertyWiseEqualityComparer()
			: this(c => typeof(EqualityComparer<>).MakeGenericType(c).GetProperty("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null, null))
		{
		}

		public PropertyWiseEqualityComparer(Func<Type, object> equalityComparerFromType)
		{
			this.equalityComparerFromType = equalityComparerFromType;
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
					var expression = Expression.Call(Expression.Constant(equalityComparerFromType(property.PropertyType)), TypeUtils.GetMethod<IEqualityComparer<object>>(c => c.Equals(null, null)).GetMethodFromTypeWithNewGenericArg(property.PropertyType), Expression.Property(left, property), Expression.Property(right, property));

					if (body == null)
					{
						body = expression;
					}
					else
					{
						body = Expression.AndAlso(body, expression);
					}
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