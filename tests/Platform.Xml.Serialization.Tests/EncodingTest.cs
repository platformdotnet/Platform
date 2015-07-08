using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{
     [TestFixture]
    public class EncodingTest
    {
        [Test]
        public void ShouldContainUTF8()
        {
            var result = XmlSerializer<Thing>.New().SerializeToString(new Thing() {Property = "Test"}, Encoding.UTF8);
            Assert.IsTrue(result.Contains("utf-8"));
        }

        [Test]
        public void ShouldContainUTF16()
        {
            var result = XmlSerializer<Thing>.New().SerializeToString(new Thing() { Property = "Test" });
            Assert.IsTrue(result.Contains("utf-16"));
        }
    }

    [XmlElement]
    public class Thing
    {
        [XmlElement]
        public string Property { get; set; }
    }
}
