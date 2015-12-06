using System.Xml;
using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{
	[TestFixture]
	public class XmlNodeTests
	{
		[SetUp]
		public void SetUp()
		{
		}

		[TearDown]
		public void TearDown()
		{
		}

		[XmlElement]
		public class Foo
		{
			[XmlElement]
			public string S = "Hello";

			[XmlElement]
			public XmlNode T;

			[XmlElement]
			public string U = "There";

			[XmlElement]
			public XmlNode V;

			public Foo()
			{
				this.T = new XmlDocument().CreateElement("T");
				this.T.InnerText = "POKWER";

				this.V = new XmlDocument().CreateElement("V");
				this.V.InnerText = "HMM";
			}
		}

		[Test, Category("IgnoreOnMono")]
		public void Test1()
		{
			var foo = new Foo();
			var serializer = XmlSerializer<Foo>.New();

			var s1 = serializer.SerializeToString(foo);

			foo = serializer.Deserialize(s1);

			var s2 = serializer.SerializeToString(foo);

			Assert.AreEqual(s1, s2);
		}
	}
}
