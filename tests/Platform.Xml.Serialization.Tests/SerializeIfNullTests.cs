using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{
    [TestFixture]
    public class SerializeIfNullTests
    {

        [Test]
        public void ShouldSerializeNullProperty()
        {
            var item = new ItemWithSerializableNullProperty();
            var xml = XmlSerializer<ItemWithSerializableNullProperty>.New().SerializeToString(item).Replace("\n","").Replace("\r","").Replace("\t","");
            Assert.True(xml.Equals("<?xml version=\"1.0\" encoding=\"utf-16\"?><ItemWithSerializableNullProperty>  <Test /></ItemWithSerializableNullProperty>"));
        }

        [Test]
        public void ShouldNotSerializeNullProperty()
        {
            var item = new ItemWithNullProperty();
            var xml = XmlSerializer<ItemWithNullProperty>.New().SerializeToString(item).Replace("\n", "").Replace("\r", "").Replace("\t", "");
            Assert.True(xml.Equals("<?xml version=\"1.0\" encoding=\"utf-16\"?><ItemWithNullProperty />"));
        }

        [XmlElement]
        public class ItemWithSerializableNullProperty
        {
            [XmlElement(SerializeIfNull = true)]
            public string Test { get; set; }
        }

        [XmlElement]
        public class ItemWithNullProperty
        {
            [XmlElement]
            public string Test { get; set; }
        }
    }
}
