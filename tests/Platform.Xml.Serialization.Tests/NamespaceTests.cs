using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{
    [TestFixture]
    public class NamespaceTests
    {
        [Test]
        public void NamespaceOnlyOnParent()
        {
            var test = new Parent()
            {
                Child = new Child()
                {
                    Property = "Test"
                }
            };

            var xml = XmlSerializer<Parent>.New().SerializeToString(test);
            Assert.IsFalse(xml.Contains("<Child xmlns=\"\">"));
            Assert.IsTrue(xml.Contains("<Parent xmlns=\"http://mynamespace.org/test\">"));
        }
    }

    [XmlElement(Namespace = "http://mynamespace.org/test")]
    public class Parent
    {
        [XmlElement]
        public Child Child { get; set; }
    }

    [XmlElement]
    public class Child
    {
        [XmlElement]
        public string Property { get; set; }
    }
}
