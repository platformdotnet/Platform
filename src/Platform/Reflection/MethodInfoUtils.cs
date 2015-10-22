using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Platform.Reflection
{
	public static class MethodInfoUtils
	{
		public static MethodInfo MakeMethodWithChangedDeclaringTypeGeneric<T>(this MethodInfo methodInfo)
		{
			return methodInfo.MakeMethodWithChangedDeclaringTypeGeneric(typeof(T));
		}

		private static IEnumerable<Type> NormalizeGenericTypes(ParameterInfo[] values, Type genericArgument)
		{
			return values.Select(c => c.ParameterType.IsGenericParameter ? genericArgument : c.ParameterType);
		}

        public static MethodInfo MakeMethodWithChangedDeclaringTypeGeneric(this MethodInfo methodInfo, Type type)
		{
			if (methodInfo.DeclaringType == null)
			{
				throw new ArgumentException("Does not have a declaring genericArgument", nameof(methodInfo));
			}

	        var genericDeclaringType = methodInfo
		        .DeclaringType
		        .GetGenericTypeDefinition();

	        var genericArgument = methodInfo.DeclaringType.GetGenericArguments()[0];

	        var result = genericDeclaringType
		        .GetMethods()
		        .Where(c => c.Name == methodInfo.Name)
		        .Select(c => new { method = c, currentTypes = NormalizeGenericTypes(c.GetParameters(), genericArgument).ToArray(), newTypes = NormalizeGenericTypes(c.GetParameters(), type).ToArray() })
		        .Single(c => c.currentTypes.SequenceEqual(methodInfo.GetParameters().Select(d => d.ParameterType)));

			return genericDeclaringType
				.MakeGenericType(type)
				.GetMethod(result.method.Name, (methodInfo.IsStatic ? BindingFlags.Static : BindingFlags.Instance) | (methodInfo.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic), null,  result.newTypes, null);
		}
	}
}
