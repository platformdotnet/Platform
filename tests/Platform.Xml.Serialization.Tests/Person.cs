using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Platform.Xml.Serialization.Tests
{
	[XmlElement]
	public class Person
	{
		[XmlAttribute("type")]
		public virtual string PersonType
		{
			get
			{
				return Regex.Replace(this.GetType().Name, "Person$", "");
			}
		}

		[XmlAttribute(MakeNameLowercase=true)]
		public virtual string Name { get; set; }

		[XmlAttribute(MakeNameLowercase = true)]
		public virtual DateTime Birthdate { get; set; }

		[XmlElement]
		public virtual DateTime? Deathdate { get; set; }

		public override bool Equals(object obj)
		{
			var person = obj as Person;

			if (person == null)
			{
				return false;
			}

			if (person.Name != this.Name)
			{
				return false;
			}

			if (!Object.Equals(person.Birthdate, this.Birthdate))
			{
				return false;
			}

			if (!Object.Equals(person.Deathdate, this.Deathdate))
			{
				return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}
	}
}
