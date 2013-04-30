using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{
	[TestFixture]
	public class BasicSerializerTests
	{
		[Test]
		public virtual void TestSimpleSerialization()
		{
			var serializer = XmlSerializer<Person>.New();

			var person1 = new Person
			{
				Name = "Tum"
			};

			var xml = serializer.SerializeToString(person1);

			var person2 = serializer.Deserialize(xml);

			Assert.AreEqual(person1, person2);
		}

		[Test]
		public virtual void TestDateAndNullableDate()
		{
			var serializer = XmlSerializer<Person>.New();

			var person1 = new Person
			{
				Name = "Person 1",
				Birthdate = new DateTime(1980, 1, 1),
				Deathdate = null
			};

			var xml = serializer.SerializeToString(person1);

			var person2 = serializer.Deserialize(xml);

			Assert.AreEqual(person1.Deathdate, person2.Deathdate);
			Assert.AreEqual(person1, person2);

			// Try with non-null deathdate

			person1 = new Person
			{
				Name = "Dinosaur",
				Birthdate = DateTime.MinValue,
				Deathdate = DateTime.MinValue.AddYears(1000)
			};

			xml = serializer.SerializeToString(person1);

			person2 = serializer.Deserialize(xml);

			Assert.AreEqual(person1.Deathdate, person2.Deathdate);

			Assert.AreEqual(person1, person2);
		}

		[Test]
		public virtual void TestPropertyWithDynamicType()
		{
			var serializer = XmlSerializer<FriendlyPerson>.New();

			var employee = new Employee
			{
				Name = "Employee 1"
			};

			var person1 = new FriendlyPerson
			{
				Name = "Friendly Person",
				Friend = employee
			};

			var xml = serializer.SerializeToString(person1);
			
			Console.WriteLine(xml);

			var person2 = serializer.Deserialize(xml);

			Assert.AreEqual(person1, person2);
		}
	}
}
