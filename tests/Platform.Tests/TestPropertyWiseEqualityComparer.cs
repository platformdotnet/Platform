using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Platform.Tests
{
	[TestFixture]
	public class TestPropertyWiseEqualityComparer
	{
		public class User
		{
			public int Age { get; set; }
			public string Name { get; set; }
		}

		[Test]
		public void Test()
		{
			var user1 = new User { Name = "Harrison", Age = 1 };
			var user2 = new User { Name = "Mars", Age = 12 };

			Assert.IsTrue(PropertyWiseEqualityComparer<User>.Default.Equals(user1, user1));
			Assert.IsFalse(PropertyWiseEqualityComparer<User>.Default.Equals(user1, user2));
		}
	}
}
