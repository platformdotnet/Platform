using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{
	[TestFixture]
	public class InheritanceTests
	{
		[XmlElement]
		public class Animal
		{
			public Animal()
			{
				this.Name = "Mars";
			}

			[XmlElement]
			public string Name
			{
				get;
				private set;
			}
		}

		[XmlElement]
		public class Cat
			: Animal
		{
			[XmlElement]
			public string Growl
			{
				get;
				set;
			}
		}

		[Test]
		public void Test_Serialization_Of_Inherited_Types()
		{
			var cat = new Cat()
			{
				Growl = "meow"
			};

			var serializer = XmlSerializer<Cat>.New();

			var s = serializer.SerializeToString(cat);

			var cat2 = serializer.Deserialize(s);

			Assert.AreEqual("meow", cat2.Growl);
			Assert.AreEqual("Mars", cat2.Name);
		}
	}
}
