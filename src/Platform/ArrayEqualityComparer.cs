// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)

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

			var retval = 0;

			foreach (T element in array)
			{
				retval ^= this.elementComparer.GetHashCode(element);
			}

			return retval;
		}
	}
}
