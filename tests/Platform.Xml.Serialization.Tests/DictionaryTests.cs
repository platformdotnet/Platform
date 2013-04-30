using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{
	[TestFixture]
	public class DictionaryTests
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
			[XmlElement("Options", typeof(Dictionary<string, int>))]
			[XmlDictionaryElementType(typeof(int), "int")]

			public Dictionary<string, int> Dictionary { get; set; }

			public Foo()
			{
				this.Dictionary = new Dictionary<string, int>();

				this.Dictionary["One"] = 1;
				this.Dictionary["Two"] = 2;
			}
		}

		[Test]
		public void Test_Basic_Dictionary_Serialization()
		{
			var foo = new Foo();

			var serializer = XmlSerializer<Foo>.New();

			serializer.Serialize(foo, Console.Out);
			var s1 = serializer.SerializeToString(foo);

			foo = serializer.Deserialize(s1);
			serializer.Serialize(foo, Console.Out);
			var s2 = serializer.SerializeToString(foo);

			Assert.AreEqual(s1, s2);
		}
	}
}
