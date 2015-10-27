using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Platform.Reflection
{
	public static class MethodInfoUtils
	{
		private static IEnumerable<Type> NormalizeGenericTypes(ParameterInfo[] values, Type[] genericParameters)
		{
			return values.Select(c => (c.ParameterType.IsGenericParameter || c.ParameterType.ContainsGenericParameters) ? genericParameters[c.Position] : c.ParameterType);
		}
		
		public static MethodInfo GetGenericTypeDefinitionMethod(this MethodInfo methodInfo)
		{
			return methodInfo.GetMethodFromTypeWithNewGenericArgs();
		}

		public static MethodInfo GetMethodFromTypeWithNewGenericArg<T>(this MethodInfo methodInfo)
		{
			return methodInfo.GetMethodFromTypeWithNewGenericArgs(typeof(T));
		}

		public static MethodInfo GetMethodFromTypeWithNewGenericArg(this MethodInfo methodInfo, Type type)
		{
			return GetMethodFromTypeWithNewGenericArgs(methodInfo, type);
		}

		public static MethodInfo GetMethodFromTypeWithNewGenericArgs(this MethodInfo methodInfo, params Type[] types)
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
			var methodInfoParameterTypes = methodInfoParameters.Select(c => c.ParameterType).ToArray();

	        var result = genericDeclaringType
				.GetMethods()
				.Where(c => c.Name == methodInfo.Name)
				.Where(c => c.GetParameters().Length == methodInfoParameters.Length)
				.Select(c => new { method = c, currentTypes = NormalizeGenericTypes(c.GetParameters(), methodInfoParameterTypes).ToArray(), matchedMethod = c})
		        .SingleOrDefault(c => c.currentTypes.SequenceEqual(methodInfo.GetParameters().Select(d => d.ParameterType)));

			if (result == null)
			{
				return null;
			}

			return genericDeclaringType
				.MakeGenericType(types)
				.GetMethod(result.method.Name, (methodInfo.IsStatic ? BindingFlags.Static : BindingFlags.Instance) | (methodInfo.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic), null,  result.matchedMethod.GetParameters().Select(c => c.ParameterType).ToArray(), null);
		}
	}
}
