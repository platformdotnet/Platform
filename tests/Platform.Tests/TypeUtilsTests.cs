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
	}
}
