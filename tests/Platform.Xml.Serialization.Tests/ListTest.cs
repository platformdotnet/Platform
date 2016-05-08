using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{
    [TestFixture]
    public class ListTest
    {
        /// <summary>
        /// This test failed before this fix
        /// </summary>
        [Test]
        public void SerializeAndDeserializeLower()
        {
            var test = new ListHolderLower()
            {
                new ListItemLower()
            };
            var xml = XmlSerializer<ListHolderLower>.New().SerializeToString(test);
            var obj = XmlSerializer<ListHolderLower>.New().Deserialize(xml);
            Assert.IsTrue(obj.Count == 1, "List should contain one element");
        }

        [Test]
        public void SerializeAndDeserializeNormal()
        {
            var test = new ListHolder()
            {
                new ListItem()
            };
            var xml = XmlSerializer<ListHolder>.New().SerializeToString(test);
            var obj = XmlSerializer<ListHolder>.New().Deserialize(xml);
            Assert.IsTrue(obj.Count == 1, "List should contain one element");
        }

	    [Test]
	    public void Test_Serialize_List()
	    {
		    var foo = new Foo() { As = new [] { new B() } };

		    XmlSerializer<Foo>.New().SerializeToString(foo);
	    }
    }

	public class A
	{	
	}

	public class B : A
	{	
	}

	[XmlElement]
	public class Foo
	{
		[XmlElement]
		[XmlListElement("A", ItemType = typeof(A), SerializeAsValueNode = true, ValueNodeAttributeName = "Name")]
		public A[] As { get; set; }
	}

	[XmlElement(MakeNameLowercase=true)]
    [XmlListElement(typeof(ListItemLower),MakeNameLowercase=true)]
    public class ListHolderLower : List<ListItemLower>
    {
                
    }

    [XmlElement(MakeNameLowercase=true)]
    public class ListItemLower
    {
        [XmlAttribute]
        public int Property { get; set; }
    }


    [XmlElement]
    [XmlListElement(typeof(ListItem))]
    public class ListHolder : List<ListItem>
    {

    }

    [XmlElement]
    public class ListItem
    {
        [XmlAttribute]
        public int Property { get; set; }
    }
}
