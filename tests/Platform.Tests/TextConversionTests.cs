using System.Text;
using NUnit.Framework;
using Platform.Text;

namespace Platform.Tests
{
	[TestFixture]
	public class TextConversionTests
	{
		[Test]
		public void Test_HexStringConversion()
		{
			const string complex = "http://github.com/+123%$";

			var s = TextConversion.ToEscapedHexString(complex);

			Assert.AreEqual("http%3A%2F%2Fgithub%2Ecom%2F%2B123%25%24", s);

			s = TextConversion.FromEscapedHexString(s);

			Assert.AreEqual(complex, s);
		}

		[Test]
		public void Test_Base64()
		{
			var bytes = Encoding.ASCII.GetBytes("");
			var s = TextConversion.ToBase64String(bytes);

			bytes = Encoding.ASCII.GetBytes("H");
			s = TextConversion.ToBase64String(bytes);
			Assert.AreEqual("SA==", s);
			Assert.AreEqual(TextConversion.FromBase64String(s), bytes);

			bytes = Encoding.ASCII.GetBytes("He");
			s = TextConversion.ToBase64String(bytes);
			Assert.AreEqual("SGU=", s);
			Assert.AreEqual(TextConversion.FromBase64String(s), bytes);

			bytes = Encoding.ASCII.GetBytes("Hel");
			s = TextConversion.ToBase64String(bytes);
			Assert.AreEqual("SGVs", s);
			Assert.AreEqual(TextConversion.FromBase64String(s), bytes);

			bytes = Encoding.ASCII.GetBytes("Hell");
			s = TextConversion.ToBase64String(bytes);
			Assert.AreEqual("SGVsbA==", s);
			Assert.AreEqual(TextConversion.FromBase64String(s), bytes);

			bytes = Encoding.ASCII.GetBytes("Hello");
			s = TextConversion.ToBase64String(bytes);
			Assert.AreEqual("SGVsbG8=", s);
			Assert.AreEqual(TextConversion.FromBase64String(s), bytes);
		}

		[Test]
		public void Test_Soundex1()
		{
			const string name1 = "Euler";
			const string name2 = "Gauss";

			var soundex1 = TextConversion.ToSoundex(name1);
			var soundex2 = TextConversion.ToSoundex(name2);

			Assert.AreEqual("E460", soundex1);
			Assert.AreEqual("G200", soundex2);
		}

		[Test]
		public void Test_Soundex2()
		{
			const string name1 = "Hilbert";
			const string name2 = "Heilbronn";

			var soundex1 = TextConversion.ToSoundex(name1);
			var soundex2 = TextConversion.ToSoundex(name2);

			Assert.AreEqual(soundex1, soundex2);
		}
	}
}
