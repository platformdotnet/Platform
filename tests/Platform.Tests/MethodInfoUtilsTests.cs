using System.Collections.Generic;
using NUnit.Framework;
using Platform.Reflection;

namespace Platform.Tests
{
	[TestFixture]
	public class MethodInfoUtilsTests
	{
		[Test]
		public void Test()
		{
			string x;

			var method = TypeUtils.GetMethod<Dictionary<string, string>>(c => c.TryGetValue("", out x));

			var method2 = method.GetGenericTypeDefinitionMethod();

			Assert.AreEqual(typeof(Dictionary<,>).GetMethod("TryGetValue"), method2);

			var method3 = method.GetMethodFromTypeWithNewGenericArgs(typeof(int), typeof(int));
		}
	}
}
