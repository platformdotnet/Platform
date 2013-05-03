using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Platform.Text;

namespace Platform.Tests
{
	[TestFixture, Category("IgnoreOnMono")]
	public class TextSerializerTests
	{
		[TextRecord]
		public struct Address
		{
			[TextField]
			public int Number
			{
				get;
				set;
			}

			[TextField]
			public char? FlatNumber
			{
				get;
				set;
			}

			[TextField]
			public string Street
			{
				get;
				set;
			}
		}

		public enum Breed
		{
			Unknown,
			Tabby,
			Persian,
			Black
		}

		[TextRecord]
		public struct Cat
		{
			[TextField]
			public Breed Breed
			{
				get;
				set;
			}

			public Cat(string name)
				: this()
			{
				this.Name = name;
			}

			[TextField]
			public string Biography
			{
				get;
				set;
			}

			[TextField]
			public string BiographyLiteral
			{
				get;
				set;
			}

			[TextField]
			public int Age
			{
				get;
				set;
			}

			[TextField]
			public List<string> Aliases
			{
				get;
				set;
			}

			[TextField]
			public DateTime? Birthdate
			{
				get;
				set;
			}

			[TextField]
			public List<Address> Addresses
			{
				get;
				set;
			}

			[TextField]
			public string Name
			{
				get;
				set;
			}

			[TextField]
			public string Salutation
			{
				get;
				set;
			}

			[TextField]
			public Address[] AddressesAsArray
			{
				get;
				set;
			}
		}

		[TextRecord]
		public struct Person
		{
			[TextField]
			public List<string> Aliases
			{
				get;
				set;
			}

			[TextField]
			public Address Address
			{
				get;
				set;
			}

			[TextField]
			public string Name
			{
				get;
				set;
			}
		}

		public class CustomDictionary<K, V>
			: Dictionary<K, V>
		{
		}

		[Test]
		public void TestCustomDictionary()
		{
			var serializer = TextSerializer.GetSerializer<CustomDictionary<string, List<int>>>();

			var x = new CustomDictionary<string, List<int>>()
			        	{
							{"one", new List<int>() {11, 12, 13}},
							{"two", new List<int>() {21, 22, 23}}
			        	};

			var s = serializer.Serialize(x);

			Console.Write(s);
			var y = serializer.Deserialize(s);

			Assert.AreEqual(typeof(CustomDictionary<string, int>).Name, y.GetType().Name);
			Assert.AreEqual(s, serializer.Serialize(y));
		}

		public class StringList
			: List<string>
		{
		}

		[Test]
		public void TestCustomList()
		{
			var serializer = TextSerializer.GetSerializer<StringList>();

			var x = new StringList()
			        	{
							"a","b", "c"
			        	};

			var s = serializer.Serialize(x);

			var y = serializer.Deserialize(s);

			Console.WriteLine(s);

			Assert.AreEqual(typeof(StringList).Name, y.GetType().Name);
			Assert.AreEqual(s, serializer.Serialize(y));
		}

		public class StringList2<T>
			: List<T>
		{
		}

		[Test]
		public void TestIList()
		{
			var serializer = TextSerializer.GetSerializer<IList<string>>();
			var list = new List<string>();

			list.Add("a");
			list.Add("b");

			var text = serializer.Serialize(list);

			var deserializedList = serializer.Deserialize(text);

			Assert.AreEqual(text, serializer.Serialize(deserializedList));
		}


		[Test]
		public void TestCustomList2()
		{
			var serializer = TextSerializer.GetSerializer<StringList2<string>>();

			var x = new StringList2<string>()
			        	{
							"a","b", "c"
			        	};

			var s = serializer.Serialize(x);

			var y = serializer.Deserialize(s);

			Assert.AreEqual(typeof(StringList2<string>).Name, y.GetType().Name);
			Assert.AreEqual(s, serializer.Serialize(y));
		}

		[Test]
		public void TestSerializeEnum()
		{
			var x = Breed.Tabby;
			var serializer = TextSerializer.GetSerializer<Breed>();

			var s = serializer.Serialize(x);
			Console.WriteLine(s);
			Assert.AreEqual(x, serializer.Deserialize(s));

			Breed? nx = null;
			var nserializer = TextSerializer.GetSerializer<Breed?>();
			s = nserializer.Serialize(nx);
			Console.WriteLine(s);
			Assert.AreEqual(nx, nserializer.Deserialize(s));

			s = nserializer.Serialize(Breed.Persian);
			Console.WriteLine(s);
			Assert.AreEqual(Breed.Persian, nserializer.Deserialize(s));
		}

		[Test]
		public void TestSerializeInt32()
		{
			var x = 10;

			var serializer = TextSerializer.GetSerializer<Int32>();

			var s = serializer.Serialize(x);
			Console.WriteLine(s);
			Assert.AreEqual(x, serializer.Deserialize(s));

			int? nx = null;
			var nserializer = TextSerializer.GetSerializer<int?>();
			s = nserializer.Serialize(nx);
			Console.WriteLine(s);
			Assert.AreEqual(nx, nserializer.Deserialize(s));
		}

		[Test, Category("IgnoreOnMono")]
		public void TestSerializePerson()
		{
			var person = new Person()
			{
				Address = new Address { Number = 18, Street = "Turnham Green" },
				Name = "Tum",
				Aliases = new List<string>() { "a", "b" }
			};

			var serializer = TextSerializer.GetSerializer<Person>();

			var text = serializer.Serialize(person);

			Console.WriteLine(text);

			var deserializedPerson = serializer.Deserialize(text);

			Assert.AreEqual("Tum", deserializedPerson.Name);
			Assert.AreEqual(18, deserializedPerson.Address.Number);
			Assert.AreEqual("Turnham Green", deserializedPerson.Address.Street);
		}

		[TextRecord]
		public struct Dev
		{
			[TextField]
			public int Age
			{
				get;
				set;
			}

			[TextField]
			public int Height
			{
				get;
				set;
			}
		}

		[Test]
		public void TestSerializeListStringInt()
		{
			var devserializer = TextSerializer.GetSerializer<List<Dev>>();
			var serializerListOfInts = TextSerializer.GetSerializer<List<Dictionary<string, int>>>();

			var list = new List<Dictionary<string, int>>();

			var dict = new Dictionary<string, int> { { "Age", 10 }, { "Height", 175 } };

			list.Add(dict);

			var s = serializerListOfInts.Serialize(list);
			Console.WriteLine(s);

			var devs = devserializer.Deserialize(s);

			Assert.AreEqual(10, devs[0].Age);
			Assert.AreEqual(175, devs[0].Height);
		}

		[Test]
		public void TestSerializeList()
		{
			string s;
			var list = new List<string>() { "a", "b" };

			var serializer = TextSerializer.GetSerializer<List<string>>();

			s = serializer.Serialize(list);
			Console.WriteLine(s);
			Assert.AreEqual("[a;b]", s);
			Assert.AreEqual(list[0], serializer.Deserialize(s)[0]);
			Assert.AreEqual(list[1], serializer.Deserialize(s)[1]);

			list = null;

			s = serializer.Serialize(list);
			Console.WriteLine(s);
			Assert.AreEqual("", s);

			list = new List<string>();

			s = serializer.Serialize(list);
			Console.WriteLine(s);
			Assert.AreEqual("[]", s);
		}

		[Test]
		public void TestSerializeListOfStructs()
		{
			string s;
			var list = new List<Address>();

			list.Add(new Address() { Number = 1, Street = "Loop" });
			list.Add(new Address() { Number = 5, Street = "Turnham Green" });

			var serializer = TextSerializer.GetSerializer<List<Address>>();

			s = serializer.Serialize(list);
			Console.WriteLine(s);
			Assert.AreEqual("[[Number=1;Street=Loop];[Number=5;Street=Turnham Green]]", s);
			Assert.AreEqual(list[0], serializer.Deserialize(s)[0]);
			Assert.AreEqual(list[1], serializer.Deserialize(s)[1]);

			list = null;

			s = serializer.Serialize(list);
			Console.WriteLine(s);
			Assert.AreEqual("", s);

			list = new List<Address>();

			s = serializer.Serialize(list);
			Console.WriteLine(s);
			Assert.AreEqual("[]", s);
		}

		[Test]
		public void TestSerializeDictionary()
		{
			string s;
			var dictionary = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } };

			var serializer = TextSerializer.GetSerializer<Dictionary<int, string>>();

			s = serializer.Serialize(dictionary);
			Console.WriteLine(s);
			Assert.AreEqual("[1=one;2=two]", s);
			Assert.AreEqual(dictionary[1], serializer.Deserialize(s)[1]);
			Assert.AreEqual(dictionary[2], serializer.Deserialize(s)[2]);

			dictionary = null;

			s = serializer.Serialize(dictionary);
			Console.WriteLine(s);
			Assert.AreEqual("", s);

			dictionary = new Dictionary<int, string>();

			s = serializer.Serialize(dictionary);
			Console.WriteLine(s);
			Assert.AreEqual("[]", s);
		}

		[Test]
		public void TestSerializeString()
		{
			string s;
			const string hello = "Hello World";

			var serializer = TextSerializer.GetSerializer<string>();

			s = serializer.Serialize(hello);
			Console.WriteLine(s);
			Assert.AreEqual(hello, s);
			Assert.AreEqual(hello, serializer.Deserialize(s));

			s = serializer.Serialize("");
			Console.WriteLine(s);
			Assert.AreEqual("[]", s);
			Assert.AreEqual("", serializer.Deserialize(s));

			s = serializer.Serialize(null);
			Console.WriteLine(s);
			Assert.AreEqual("", s);
			Assert.AreEqual(null, serializer.Deserialize(s));

			s = serializer.Serialize("|abc");
			Console.WriteLine(s);
			Assert.AreEqual("|abc", s);
			Assert.AreEqual("|abc", serializer.Deserialize(s));

			s = serializer.Serialize("||a|bc");
			Console.WriteLine(s);
			Assert.AreEqual("||a|bc", s);
			Assert.AreEqual("||a|bc", serializer.Deserialize(s));

			s = serializer.Serialize("||a|bc[]");
			Console.WriteLine(s);
			Assert.AreEqual("||a|bc[\\]", s);
			Assert.AreEqual("||a|bc[]", serializer.Deserialize(s));

			s = serializer.Serialize(@"\abc\\");
			Console.WriteLine(s);
			Assert.AreEqual(@"\\abc\\\\", s);
			s = serializer.Deserialize(s);
			Assert.AreEqual(@"\abc\\", s);
		}

		[Test]
		public void TestSerializeGuid()
		{
			var guid = Guid.NewGuid();

			var serializer = TextSerializer.GetSerializer<Guid>();

			var s = serializer.Serialize(guid);

			Console.WriteLine(s);
			Assert.AreEqual(guid.ToString("N"), serializer.Serialize(guid));
			Assert.AreEqual(guid, serializer.Deserialize(s));
		}

		[Test]
		public void TestDeserializeStructsAsStrings()
		{
			var list = new List<Address>()
			           	{
			           		new Address() {Number = 1, Street = "A"},
			           		new Address() {Number = 2, Street = "B"}
			           	};

			var serializer = TextSerializer.GetSerializer<List<Address>>();


			var s = serializer.Serialize(list);

			Console.WriteLine(s);

			var serializer2 = TextSerializer.GetSerializer<List<string>>();

			var list2 = serializer2.Deserialize(s);

			Console.WriteLine("0: {0}", list2[0]);
			Console.WriteLine("1: {0}", list2[1]);

			Console.WriteLine("[" + list2.JoinToString(";") + "]");
		}

		[Test]
		public void TestPerformance()
		{
			var cat = new Cat();

			cat.Name = "Mars";
			cat.Age = 5;
			cat.Biography = "ewtrwetertrwtwrtre";
			cat.Breed = Breed.Persian;
			cat.Salutation = "Ms";
			cat.AddressesAsArray = new Address[] { new Address() { Street = "test" } };

			var stopwatch = new Stopwatch();

			Console.WriteLine(TextSerializer.SerializeToString(cat));

			const int times = 100000;

			stopwatch.Start();
			for (var i = 0; i < times; i++)
			{
				TextSerializer.SerializeToString(cat);
			}
			stopwatch.Stop();

			Console.WriteLine(TimeSpan.FromSeconds(stopwatch.ElapsedTicks * 1d / Stopwatch.Frequency).TotalMilliseconds);
		}

		[Test]
		public void TestCat()
		{
			var cat = new Cat()
			{
				Breed = Breed.Tabby,
				Biography = "[SquareBrack%ets]|PIPE|",
				BiographyLiteral = "[SquareBrack%ets]|PIPE|",
				Birthdate = DateTime.Parse("2003-11-05"),
				Name = "Mars",
				Aliases = new List<string>() { "Possum", "Cat" },
				Addresses = new List<Address>()
			          		          	{
			          		          		new Address()
			          		          			{
			          		          				Number = 10,
			          		          				FlatNumber = null
			          		          			},
												new Address()
			          		          			{
			          		          				Number = 11,
			          		          				FlatNumber = 'C'
			          		          			}
			          		          	},
				AddressesAsArray = new List<Address>()
		          		          	{
		          		          		new Address()
		          		          			{
		          		          				Number = 10,
		          		          				FlatNumber = null
		          		          			},
											new Address()
		          		          			{
		          		          				Number = 11,
		          		          				FlatNumber = 'C'
		          		          			}
		          		          	}.ToArray()
			};

			var serializer = TextSerializer.GetSerializer<Cat>();

			var s = serializer.Serialize(cat);
			Console.WriteLine(s);

			var dcat = serializer.Deserialize(s);
			Assert.AreEqual(cat.Name, dcat.Name);
			Assert.AreEqual(cat.Biography, dcat.Biography);
			Assert.AreEqual(cat.BiographyLiteral, dcat.BiographyLiteral);
			Assert.AreEqual(cat.Aliases[0], dcat.Aliases[0]);
			Assert.AreEqual(cat.Aliases[1], dcat.Aliases[1]);
			Assert.AreEqual(cat.Addresses[0], dcat.Addresses[0]);
			Assert.AreEqual(cat.Addresses[0].Number, dcat.Addresses[0].Number);
			Assert.AreEqual(cat.Addresses[0].FlatNumber, dcat.Addresses[0].FlatNumber);
			Assert.AreEqual(cat.AddressesAsArray[0], dcat.Addresses[0]);
			Assert.AreEqual(cat.AddressesAsArray[0].Number, dcat.Addresses[0].Number);
			Assert.AreEqual(cat.AddressesAsArray[0].FlatNumber, dcat.Addresses[0].FlatNumber);
		}

		public static void Test(Cat cat)
		{
			Test(new Cat("bob"));

			cat.ToString();
		}
	}
}