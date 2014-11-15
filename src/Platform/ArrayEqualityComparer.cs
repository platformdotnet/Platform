// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System.Linq;
using System.Collections.Generic;

namespace Platform
{
	public class ArrayEqualityComparer<T>
		: IEqualityComparer<T[]>
	{
		public static readonly ArrayEqualityComparer<T> Default = new ArrayEqualityComparer<T>(EqualityComparer<T>.Default);
		
		private readonly IEqualityComparer<T> elementComparer;

		public ArrayEqualityComparer(IEqualityComparer<T> elementComparer)
		{
			this.elementComparer = elementComparer;
		}

		public virtual bool Equals(T[] first, T[] second)
		{
			if (first == second)
			{
				return true;
			}

			if (first == null || second == null)
			{
				return false;
			}

			if (first.Length != second.Length)
			{
				return false;
			}

			for (var i = 0; i < first.Length; i++)
			{
				if (!this.elementComparer.Equals(first[i], second[i]))
				{
					return false;
				}
			}

			return true;
		}

		public virtual int GetHashCode(T[] array)
		{
			if (array == null)
			{
				return 0;
			}

			return array.Aggregate(0, (current, element) => current ^ this.elementComparer.GetHashCode(element));
		}
	}
}
