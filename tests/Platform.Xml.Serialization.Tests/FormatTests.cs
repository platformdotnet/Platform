using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{

    [TestFixture]
    public class FormatTests
    {
        [Test]
        public void CultureNotSpecified()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL"); //Uses , instead of .
            var xml = XmlSerializer<TestWithoutCulture>.New().SerializeToString(new TestWithoutCulture() {FloatProperty = 0.45F});
            Assert.IsTrue(xml.Contains("0,45"), "Number should contain a , instead of a .");
            var obj = XmlSerializer<TestWithoutCulture>.New().Deserialize(xml);
            Assert.IsTrue((decimal)obj.FloatProperty == 0.45M, "Number should be 0.45");

            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; //Uses . instead of ,
            var xml2 = XmlSerializer<TestWithoutCulture>.New().SerializeToString(new TestWithoutCulture() { FloatProperty = 0.45F });
            Assert.IsTrue(xml2.Contains("0.45"), "Number should contain a . instead of a ,");
            var obj2 = XmlSerializer<TestWithoutCulture>.New().Deserialize(xml2);
            Assert.IsTrue((decimal)obj2.FloatProperty == 0.45M, "Number should be 0.45");
        }

        [Test]
        public void CultureSpecified()
        {

            //Dutch culture set in format attribute
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; //Uses . instead of ,
            var xml = XmlSerializer<TestWithDutchCulture>.New().SerializeToString(new TestWithDutchCulture() { FloatProperty = 0.45F });
            Assert.IsTrue(xml.Contains("0,45"), "Number should contain a , instead of a .");
            var obj = XmlSerializer<TestWithDutchCulture>.New().Deserialize(xml);
            Assert.IsTrue((decimal)obj.FloatProperty == 0.45M, "Number should be 0.45");

            //Invariant culture set in format attribute
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL"); //Uses , instead of .
            var xml2 = XmlSerializer<TestWithInvariantCulture>.New().SerializeToString(new TestWithInvariantCulture() { FloatProperty = 0.45F });
            Assert.IsTrue(xml2.Contains("0.45"), "Number should contain a . instead of a ,");
            var obj2 = XmlSerializer<TestWithInvariantCulture>.New().Deserialize(xml2);
            Assert.IsTrue((decimal)obj2.FloatProperty == 0.45M, "Number should be 0.45");

        }

        [XmlElement]
        protected class TestWithDutchCulture
        {
            [XmlElement]
            [XmlFormat("F", Culture="nl-NL")]
            public float FloatProperty { get; set; }
        }

        [XmlElement]
        protected class TestWithInvariantCulture
        {
            [XmlElement]
            [XmlFormat("F", Culture="")]
            public float FloatProperty { get; set; }
        }

        [XmlElement]
        protected class TestWithoutCulture
        {
            [XmlElement]
            [XmlFormat("F")]
            public float FloatProperty { get; set; }
        }
    }
}
