using System;
using System.Collections.Generic;
using NUnit.Framework;
using Platform.Reflection;

namespace Platform.Tests
{
	[TestFixture]
	public class MethodInfoUtilsTests
	{
		[Test]
		public void Test1()
		{
			string x;

			var method = TypeUtils.GetMethod<Dictionary<string, string>>(c => c.TryGetValue("", out x));

			var method2 = method.GetGenericTypeDefinitionMethod();

			Assert.AreEqual(typeof(Dictionary<,>).GetMethod("TryGetValue"), method2);

			var method3 = method.GetMethodFromTypeWithNewGenericArgs(typeof(int), typeof(int));
		}

		private struct Test<T, U>
		{
			public Test(IEnumerable<U> x)
			{ }
		}

		[Test]
		public void Test2()
		{
			var ctor = TypeUtils.GetConstructor(() => new Test<int, int>(new int[1]));

			ctor = ctor.GetConstructorFromTypeWithNewGenericArgs(typeof(string), typeof(string));

			Assert.IsNotNull(ctor);
		}
	}
}
