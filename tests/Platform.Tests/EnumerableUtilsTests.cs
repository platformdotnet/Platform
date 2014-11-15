using NUnit.Framework;

namespace Platform.Tests
{
	[TestFixture]
	public class EnumerableUtilsTests
	{
		[Test]
		public void Test_Fold1()
		{
			var x = new int[] { 1, 2, 3 };

			Assert.AreEqual(6, x.Fold((a, b) => a + b));
		}

		[Test]
		public void Test_Fold2()
		{
			var x = new int[] { 1, 2, 3 };

			Assert.AreEqual(10, x.Fold(4, (a, b) => a + b));
		}

		[Test]
		public void Test_Fold_String()
		{
			var array = new [] { "a", "b", "c" };

			Assert.AreEqual("a,b,c", array.Fold((x,y) => x + "," + y));
		}
	}
}
