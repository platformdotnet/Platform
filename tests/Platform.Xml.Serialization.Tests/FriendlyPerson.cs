using System;

namespace Platform.Xml.Serialization.Tests
{
	[XmlElement("Person")]
	public class FriendlyPerson
		: Person
	{
		[XmlElement, XmlPolymorphicType(typeof(DynamicPersonTypeProvider))]
		public virtual Person Friend { get; set; }

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var friendlyPersonFriend = obj as FriendlyPerson;

			if (friendlyPersonFriend == null)
			{
				return false;
			}

			if (!Object.Equals(this.Friend, friendlyPersonFriend.Friend))
			{
				return false;
			}

			return base.Equals(obj);
		}
	}
}
