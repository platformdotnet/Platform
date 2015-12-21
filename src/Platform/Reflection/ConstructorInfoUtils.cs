using System;
using System.Linq;
using System.Reflection;

namespace Platform.Reflection
{
	public static class ConstructorInfoUtils
	{
		public static ConstructorInfo GetConstructorOnGenericType(this ConstructorInfo constructorInfo)
		{
			return constructorInfo.GetConstructorOnTypeReplacingTypeGenericArgs();
		}
		
		public static ConstructorInfo GetConstructorOnTypeReplacingTypeGenericArgs(this ConstructorInfo constructorInfo, params Type[] types)
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
			
			var realisedTypeFromGenericParam1 = genericDeclaringType.GetGenericArguments()
				.Select((k, i) => new { k, v = constructorInfo.DeclaringType?.GetGenericArguments()[i] })
				.ToDictionary(c => c.k, c => c.v);

			var realisedTypeFromGenericParam2 = genericDeclaringType.GetGenericArguments()
				.Select((k, i) => new { k, v = types[i] })
				.ToDictionary(c => c.k, c => c.v);

			var result1 = genericDeclaringType
				.GetConstructors()
				.Where(c => c.GetParameters().Length == constructorInfoParameters.Length)
				.Select(c => new { constructor = c, currentTypes = TypeUtils.Substitute(c.GetParameters().Select(d => d.ParameterType), realisedTypeFromGenericParam1), newTypes = TypeUtils.Substitute(c.GetParameters().Select(d => d.ParameterType), realisedTypeFromGenericParam2) })
				.SingleOrDefault(c => c.currentTypes.SequenceEqual(constructorInfoParameters.Select(d => d.ParameterType)));

			if (result1 == null)
			{
				return null;
			}

			return genericDeclaringType.MakeGenericType(types).GetConstructor(result1.newTypes);
		}
	}
}
