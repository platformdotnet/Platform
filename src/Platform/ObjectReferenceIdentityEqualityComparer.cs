// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Collections.Generic;

namespace Platform
{
	public class ObjectReferenceIdentityEqualityComparer<T>
		: IEqualityComparer<T>
		where T : class
	{
		public static readonly ObjectReferenceIdentityEqualityComparer<T> Default = new ObjectReferenceIdentityEqualityComparer<T>();

		public bool Equals(T x, T y)
		{
			return object.ReferenceEquals(x, y);
		}

		public int GetHashCode(T obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			return obj.GetHashCode();
		}
	}
}
