using System;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Describes the types of the items in a list to be serialized.
	/// </summary>
	/// <remarks>
	/// <p>
	/// You need to mark any IList field or property to be serialized with this attribute
	/// at least once.  The attribute is used to map an element name to the type
	/// of object contained in the list.
	/// </p>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlListElementAttribute
		: XmlElementAttribute
	{
		public virtual string Alias
		{
			get
			{
			    if (this.MakeNameLowercase)
			        return this.Name.ToLowerInvariant();
				return this.Name;
			}
			
			set
			{
				this.Name = value;
			}
		}

		public virtual Type ItemType { get { return this.Type; } set { this.Type = value; } }

		/// <summary>
		/// Specifies a list item's type.
		/// </summary>
		/// <remarks>
		/// The type's name will be used as the alias for all elements with the type.
		/// If the type has been attributed with an XmlElement attribute then the alias
		/// specified in that attribute will be used.
		/// </remarks>
		/// <param name="itemType">The type of element the list can contain.</param>
		public XmlListElementAttribute(Type itemType)
			: this(itemType, itemType.Name)
		{			
		}

		/// <summary>
		/// Specifies a list item's type.
		/// </summary>
		/// <remarks>
		/// The supplied alias will be used to map the actual element <c>Type</c> with
		/// an XML element.
		/// </remarks>
		/// <param name="itemType"></param>
		/// <param name="alias"></param>
		public XmlListElementAttribute(Type itemType, string alias)
			: base(alias, itemType)
		{
		}

		public XmlListElementAttribute(string alias)
			: base(alias, null)
		{
		}
	}
}