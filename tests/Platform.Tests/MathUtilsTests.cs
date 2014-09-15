using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platform.Tests
{
	[TestFixture]
	public class MathUtilsTests
	{
		[Test]
		public void Test_Modulus()
		{
			Assert.AreEqual(-2, MathUtils.Modulus(4, -3));
			Assert.AreEqual(2, MathUtils.Modulus(-4, 3));
			Assert.AreEqual(1, MathUtils.Modulus(-11, 3));
			Assert.AreEqual(-1, MathUtils.Modulus(11, -3));

			Assert.AreEqual(0, MathUtils.Modulus(9, 3));
			Assert.AreEqual(1, MathUtils.Modulus(9, 2));
			Assert.AreEqual(1, MathUtils.Modulus(-9, 2));
			Assert.AreEqual(0, MathUtils.Modulus(-9, 3));
			Assert.AreEqual(3, MathUtils.Modulus(-9, 4));
			Assert.AreEqual(-1, MathUtils.Modulus(-9, -4));
		}
	}
}
