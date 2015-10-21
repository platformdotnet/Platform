// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Platform
{
	public static class TypeUtils
	{
		public static MemberInfo GetMember<T>(Expression<Func<T, object>> member)
		{
			switch (member.Body.NodeType)
			{
				case ExpressionType.Call:
					return GetMethod(member);
				case ExpressionType.MemberAccess:
					if (((MemberExpression)member.Body).Member is PropertyInfo)
					{
						return GetProperty(member);
					}
					else
					{
						return GetField(member);
					}
				default:
					throw new ArgumentException($"Argument {nameof(member)} needs to contain a method, property or field", nameof(member));
			}
		}

		public static MemberInfo GetMember<T>(Expression<Action<T>> member)
		{
			switch (member.Body.NodeType)
			{
				case ExpressionType.Call:
					return GetMethod(member);
				case ExpressionType.MemberAccess:
					if (((MemberExpression)member.Body).Member is PropertyInfo)
					{
						return GetProperty(member);
					}
					else
					{
						return GetField(member);
					}
				default:
					throw new ArgumentException($"Argument {nameof(member)} needs to contain a method, property or field", nameof(member));
			}
		}

		public static MethodInfo GetMethod<T>(Expression<Func<T, object>> method)
		{
			var expression = method.Body as MethodCallExpression;

			if (expression == null)
			{
				throw new ArgumentException($"Argument {nameof(method)} needs to contain a method", nameof(method));
			}

			return expression.Method;
		}

		public static MethodInfo GetMethod<T>(Expression<Action<T>> method)
		{
			var expression = method.Body as MethodCallExpression;

			if (expression == null)
			{
				throw new ArgumentException($"Argument {nameof(method)} needs to contain a method call", nameof(method));
			}

			return expression.Method;
		}

		public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> property)
		{
			var expression = property.Body as MemberExpression;

			if (!(expression?.Member is PropertyInfo))
			{
				throw new ArgumentException($"Argument {nameof(property)} needs to contain a property", nameof(property));
			}

			return (PropertyInfo)expression.Member;
		}

		public static PropertyInfo GetProperty<T>(Expression<Action<T>> property)
		{
			var expression = property.Body as MemberExpression;

			if (!(expression?.Member is PropertyInfo))
			{
				throw new ArgumentException($"Argument {nameof(property)} needs to contain a property", nameof(property));
			}

			return (PropertyInfo)expression.Member;
		}

		public static FieldInfo GetField<T>(Expression<Func<T, object>> field)
		{
			var expression = field.Body as MemberExpression;

			if (!(expression?.Member is FieldInfo))
			{
				throw new ArgumentException($"Argument {nameof(field)} needs to contain a field", nameof(field));
			}

			return (FieldInfo)expression.Member;
		}

		public static FieldInfo GetField<T>(Expression<Action<T>> field)
		{
			var expression = field.Body as MemberExpression;

			if (!(expression?.Member is FieldInfo))
			{
				throw new ArgumentException($"Argument {nameof(field)} needs to contain a field", nameof(field));
			}

			return (FieldInfo)expression.Member;
		}

		public static Type MakeNullable(this Type type)
		{
			if (type.IsClass)
			{
				return type;
			}

			if (Nullable.GetUnderlyingType(type) != null)
			{
				return type;
			}

			return typeof(Nullable<>).MakeGenericType(type);
		}

		public static Type GetSequenceType(this Type elementType)
		{
			return typeof(IEnumerable<>).MakeGenericType(elementType);
		}

		public static Type GetSequenceElementType(this Type sequenceType)
		{
			var retval = FindSequenceElementType(sequenceType);

			return retval;
		}

		public static Type GetUnwrappedNullableType(this Type type)
		{
			return Nullable.GetUnderlyingType(type) ?? type;
		}

		private static Type FindSequenceElementType(this Type sequenceType)
		{
			if (sequenceType == null || sequenceType == typeof(string))
			{
				return null;
			}

			if (sequenceType.IsArray)
			{
				return sequenceType.GetElementType();
			}

			if (sequenceType.IsGenericType)
			{
				var genericType = sequenceType.GetGenericTypeDefinition();


				if (genericType == typeof(List<>) || genericType == (typeof(IList<>)))
				{
					return sequenceType.GetGenericArguments()[0];
				}

				foreach (var genericArgument in sequenceType.GetGenericArguments())
				{
					var eumerable = typeof(IEnumerable<>).MakeGenericType(genericArgument);

					if (eumerable.IsAssignableFrom(sequenceType))
					{
						return genericArgument;
					}
				}
			}

			var interfaces = sequenceType.GetInterfaces();

			if (interfaces != null && interfaces.Length > 0)
			{
				foreach (var element in interfaces
					.Select(FindSequenceElementType)
					.Where(element => element != null))
				{
					return element;
				}
			}

			if (sequenceType.BaseType != null && sequenceType.BaseType != typeof(object))
			{
				return FindSequenceElementType(sequenceType.BaseType);
			}

			return null;
		}

		public static IEnumerable<Type> WalkHierarchy(this Type type, bool includeInterfaces, bool convertGenericsToGenericTypeDefinition)
		{
			var currentType = type;

			if (convertGenericsToGenericTypeDefinition)
			{
				while (currentType != null)
				{
					if (currentType.IsGenericType)
					{
						yield return currentType.GetGenericTypeDefinition();
					}

					currentType = currentType.BaseType;
				}
			}
			else
			{
				while (currentType != null)
				{
					yield return currentType;

					currentType = currentType.BaseType;
				}
			}

			if (includeInterfaces)
			{
				if (convertGenericsToGenericTypeDefinition)
				{

					foreach (var interfaceType in type.GetInterfaces().Where(interfaceType => interfaceType.IsGenericType))
					{
						yield return interfaceType.GetGenericTypeDefinition();
					}
				}
				else
				{
					foreach (var interfaceType in type.GetInterfaces())
					{
						yield return interfaceType;
					}
				}
			}
		}


		/// <summary>
		/// Returns true if the type is a numeric type (by default does not return true if it is a nullable numeric type).
		/// </summary>
		public static bool IsNumericType(this Type type)
		{
			return IsIntegerType(type, false) || IsRealType(type, false);
		}

		/// <summary>
		/// Returns true if the type is a numeric type (or nullable numeric type if checkNullable is True).
		/// </summary>
		public static bool IsNumericType(this Type type, bool checkNullable)
		{
			return IsIntegerType(type, checkNullable) || IsRealType(type, checkNullable);
		}

		/// <summary>
		/// Returns true if the type is a real type (by default does not
		/// return true if it is a nullable real type).
		/// </summary>
		public static bool IsRealType(this Type type)
		{
			return IsRealType(type, false);
		}

		public static bool IsRealType(this Type type, bool checkNullable)
		{
			Type underlyingType;

			if (checkNullable && (underlyingType = Nullable.GetUnderlyingType(type)) != null)
			{
				type = underlyingType;
			}

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					return true;
			}

			return false;
		}

		public static bool IsIntegerType(this Type type)
		{
			return IsIntegerType(type, false);
		}

		public static bool IsIntegerType(this Type type, bool checkNullable)
		{
			Type underlyingType;

			if (checkNullable && (underlyingType = Nullable.GetUnderlyingType(type)) != null)
			{
				type = underlyingType;
			}

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.Int16:
				case TypeCode.UInt32:
				case TypeCode.Int32:
				case TypeCode.UInt64:
				case TypeCode.Int64:
					return true;
			}

			return false;
		}
		
		public static object GetDefaultValue(this Type type)
		{
			return GetDefaultValue(type, true);
		}

		public static object GetDefaultValue(this Type type, bool nullablesAreNull)
		{
			if (type.IsValueType)
			{
				var underlying = Nullable.GetUnderlyingType(type);

				if (underlying != null)
				{
					if (nullablesAreNull)
					{
						return null;
					}
					else
					{
						type = underlying;
					}
				}

				if (type.IsEnum)
				{
					return Enum.ToObject(type, Activator.CreateInstance(type));
				}

				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Int16:
						return (short)0;
					case TypeCode.Int32:
						return 0;
					case TypeCode.Int64:
						return (long)0;
					case TypeCode.UInt16:
						return (ushort)0;
					case TypeCode.UInt32:
						return (uint)0;
					case TypeCode.UInt64:
						return (ulong)0;
					default:
						return Activator.CreateInstance(type);
				}
			}
			else
			{
				return null;
			}
		}

		public static bool IsAssignableFromIgnoreGenericParameters(this Type type, Type compareToType)
		{
			if (type.IsGenericType)
			{
				type = type.GetGenericTypeDefinition();
			}

			foreach (var walkedType in compareToType.WalkHierarchy(type.IsInterface, true))
			{
				if (walkedType == type || type.IsAssignableFrom(compareToType))
				{
					return true;
				}
			}

			return false;
		}
	}
}
