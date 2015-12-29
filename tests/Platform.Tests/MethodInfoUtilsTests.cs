using System;
using System.Collections.Generic;
using NUnit.Framework;
using Platform.Reflection;

namespace Platform.Tests
{
	[TestFixture]
	public class MethodInfoUtilsTests
	{
		private struct Test<Z, U>
		{
			public Test(IEnumerable<U> x)
			{
			}

			public void Foo(out Z z, U[] u)
			{
				z = default(Z);
			}
		}

		[Test]
		public void Test1()
		{
			string x;

			
			var method = TypeUtils.GetMethod<Dictionary<string, string>>(c => c.TryGetValue("", out x));

			var method2 = method.GetMethodOnGenericType();

			Assert.AreEqual(typeof(Dictionary<,>).GetMethod("TryGetValue"), method2);

			var method3 = method.GetMethodOnTypeReplacingTypeGenericArgs(typeof(int), typeof(int));
		}

		[Test]
		public void Test2()
		{
			var ctor = TypeUtils.GetConstructor(() => new Test<int, int>(new int[1]));

			ctor = ctor.GetConstructorOnTypeReplacingTypeGenericArgs(typeof(int), typeof(string));

			Assert.IsNotNull(ctor);
			Assert.AreEqual(typeof(int), ctor.DeclaringType?.GetGenericArguments()[0]);
			Assert.AreEqual(typeof(string), ctor.DeclaringType?.GetGenericArguments()[1]);
		}

		[Test]
		public void Test3()
		{
			int i;
			var method = TypeUtils.GetMethod<Test<int, string>>(c => c.Foo(out i, new string[0]));

			method = method.GetMethodOnTypeReplacingTypeGenericArgs(typeof(string), typeof(long));

			Assert.IsNotNull(method);
			Assert.AreEqual(typeof(string), method.DeclaringType?.GetGenericArguments()[0]);
			Assert.AreEqual(typeof(long), method.DeclaringType?.GetGenericArguments()[1]);
		}
	}
}
