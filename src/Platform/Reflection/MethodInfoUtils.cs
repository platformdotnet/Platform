using System;
using System.Linq;
using System.Reflection;

namespace Platform.Reflection
{
	public static class MethodInfoUtils
	{
		public static MethodInfo GetGenericMethodOrRegular(this MethodInfo methodInfo)
		{
			return methodInfo.IsGenericMethod ? methodInfo.GetGenericMethodDefinition() : methodInfo;
		}
		
		public static MethodInfo GetMethodOnGenericType(this MethodInfo methodInfo)
		{
			return methodInfo.GetMethodOnTypeReplacingTypeGenericArgs();
		}
		
		public static MethodInfo GetMethodOnTypeReplacingTypeGenericArgs(this MethodInfo methodInfo, params Type[] types)
		{
			if (methodInfo.DeclaringType == null)
			{
				throw new ArgumentException("Does not have a declaring genericArgument", nameof(methodInfo));
			}

			if (!methodInfo.DeclaringType.IsGenericType)
			{
				throw new ArgumentException("Declaring type of method is not generic", nameof(methodInfo));
			}

			var genericDeclaringType = methodInfo
				.DeclaringType
				.GetGenericTypeDefinition();

			if (types == null || types.Length == 0)
			{
				types = genericDeclaringType.GetGenericArguments();
			}

			var methodInfoParameters = methodInfo.GetParameters();

			var realisedTypeFromGenericParam1 = genericDeclaringType.GetGenericArguments()
				.Select((k, i) => new { k, v = methodInfo.DeclaringType?.GetGenericArguments()[i] })
				.ToDictionary(c => c.k, c => c.v);

			var realisedTypeFromGenericParam2 = genericDeclaringType.GetGenericArguments()
				.Select((k, i) => new { k, v = types[i] })
				.ToDictionary(c => c.k, c => c.v);

			var result1 = genericDeclaringType
				.GetMethods()
				.Where(c => c.Name == methodInfo.Name)
				.Where(c => c.GetParameters().Length == methodInfoParameters.Length)
				.Select(c => new { constructor = c, currentTypes = TypeUtils.Substitute(c.GetParameters().Select(d => d.ParameterType), realisedTypeFromGenericParam1), newTypes = TypeUtils.Substitute(c.GetParameters().Select(d => d.ParameterType), realisedTypeFromGenericParam2) })
				.SingleOrDefault(c => c.currentTypes.SequenceEqual(methodInfoParameters.Select(d => d.ParameterType)));

			if (result1 == null)
			{
				return null;
			}

			return genericDeclaringType.MakeGenericType(types).GetMethod(methodInfo.Name, result1.newTypes);
		}
	}
}
