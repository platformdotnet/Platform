// Copyright (c) 2014 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Linq.Expressions;

namespace Platform
{
	public class ActivatorUtils<T>
	{
		private static Func<T> constructor;

		public static T CreateInstance()
		{
			if (constructor != null)
			{
				return constructor();
			}
			else
			{
				var body = Expression.New(typeof(T));

				var ctor = Expression.Lambda<Func<T>>(body).Compile();

				constructor = ctor;

				return ctor();
			}
		}
	}
}
