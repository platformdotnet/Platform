using System;
using System.Collections.Generic;

namespace Platform.Reflection
{
	internal class ParameterTypeComparer
		: IEqualityComparer<Type>
	{
		private readonly Type[] genericTypes;

		public ParameterTypeComparer(Type[] genericTypes)
		{
			this.genericTypes = genericTypes;
		}

		public bool Equals(Type x, Type y)
		{
			if (x == y)
			{
				return true;
			}

			if (y.ContainsGenericParameters && x.ContainsGenericParameters)
			{
				;
			}

			if (y.IsGenericParameter && x == this.genericTypes[y.GenericParameterPosition])
			{
				return true;
			}

			if (y.ContainsGenericParameters && y.HasElementType && y.IsByRef)
			{
				if (x == this.genericTypes[y.GetElementType().GenericParameterPosition])
				{
					return true;
				}
			}

			return false;
		}

		public int GetHashCode(Type obj)
		{
			return obj?.GetHashCode() ?? 0;
		}
	}
}