using System.Reflection;
using NUnit.Framework;

namespace Platform.Tests
{
	[TestFixture]
	public class TypeUtilsTests
	{
		[Test]
		public void Test_GetMethod_Static()
		{
			Assert.IsNotNull(TypeUtils.GetMethod(() => string.Format(string.Empty, new object[0])));
		}

		[Test]
		public void Test_GetConstructor()
		{
			Assert.IsNotNull(TypeUtils.GetMember(() => new TypeUtilsTests()) as ConstructorInfo);
		}
	}
}
