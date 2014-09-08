using System.Linq;
using NUnit.Framework;
using Platform.Collections;

namespace Platform.Tests
{
	[TestFixture]
	public class ListUtilsTests
	{
		[Test]
		public static void Test_Fast_Index_Of()
		{
			var s = "Hello World World".ToList();
			
			var x = s.FastIndexOf("World".ToList());
			Assert.AreEqual(6, x);

			x = s.FastIndexOf(7, "World".ToList());
			Assert.AreEqual(12, x);
		}
	}
}
