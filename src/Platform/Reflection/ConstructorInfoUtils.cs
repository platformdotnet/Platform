using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Platform.Reflection
{
	public static class ConstructorInfoUtils
	{
		private static IEnumerable<Type> NormalizeGenericTypes(ParameterInfo[] values, Type[] genericParameters)
		{
			return values.Select(c => (c.ParameterType.IsGenericParameter || c.ParameterType.ContainsGenericParameters) ? genericParameters[c.Position] : c.ParameterType);
		}

		public static ConstructorInfo GetGenericTypeDefinitionConstructor(this ConstructorInfo constructorInfo)
		{
			return constructorInfo.GetConstructorFromTypeWithNewGenericArgs();
		}

		public static ConstructorInfo GetConstructorFromTypeWithNewGenericArg<T>(this ConstructorInfo constructorInfo)
		{
			return constructorInfo.GetConstructorFromTypeWithNewGenericArgs(typeof(T));
		}

		public static ConstructorInfo GetConstructorFromTypeWithNewGenericArg(this ConstructorInfo constructorInfo, Type type)
		{
			return GetConstructorFromTypeWithNewGenericArgs(constructorInfo, type);
		}

		public static ConstructorInfo GetConstructorFromTypeWithNewGenericArgs(this ConstructorInfo constructorInfo, params Type[] types)
		{
			if (constructorInfo.DeclaringType == null)
			{
				throw new ArgumentException("Does not have a declaring genericArgument", nameof(constructorInfo));
			}

			if (!constructorInfo.DeclaringType.IsGenericType)
			{
				throw new ArgumentException("Declaring type of constructor is not generic", nameof(constructorInfo));
			}

			var genericDeclaringType = constructorInfo
				.DeclaringType
				.GetGenericTypeDefinition();

			if (types == null || types.Length == 0)
			{
				types = genericDeclaringType.GetGenericArguments();
			}

			var constructorInfoParameters = constructorInfo.GetParameters();
			var constructorInfoParameterTypes = constructorInfoParameters.Select(c => c.ParameterType).ToArray();

			var result = genericDeclaringType
				.GetConstructors()
				.Where(c => c.GetParameters().Length == constructorInfoParameters.Length)
				.Select(c => new { constructor = c, currentTypes = NormalizeGenericTypes(c.GetParameters(), constructorInfoParameterTypes).ToArray(), matchedConstructor = c })
				.SingleOrDefault(c => c.currentTypes.SequenceEqual(constructorInfo.GetParameters().Select(d => d.ParameterType)));

			if (result == null)
			{
				return null;
			}

			var newParams = result.matchedConstructor.GetParameters().Select(c => c.ParameterType).ToArray();

			return genericDeclaringType
				.MakeGenericType(types)
				.GetConstructors(BindingFlags.Instance | (constructorInfo.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic))
				.SingleOrDefault(c => c.GetParameters().Select(d => d.ParameterType).SequenceEqual(newParams, new ParameterTypeComparer(types)));
		}
	}
}
