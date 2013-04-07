using System;
using System.Collections;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class XmlTreatAsNullIfEmptyAttribute
		: XmlSerializationAttribute
	{
	}

	public class XmlSerializeBaseAttribute
		: XmlSerializationAttribute
	{
		/// <summary>
		///
		/// </summary>
		public bool SerializeBase
		{
			get
			{
				return m_SerializeBase;
			}
			
			set
			{
				m_SerializeBase = value;
			}
		}
		/// <remarks>
		/// <see cref="SerializeBase"/>
		/// </remarks>
		private bool m_SerializeBase;

		public XmlSerializeBaseAttribute()
		{
		}

		public XmlSerializeBaseAttribute(bool serializeBase)
		{
			m_SerializeBase = serializeBase;
		}
	}

	public class XmlSerializationAttribute
		: Attribute
	{
		public XmlSerializationAttribute()
		{
		}
		
		/// <summary>
		///
		/// </summary>
		public string ConditionName
		{
			get
			{
				return m_ConditionName;
			}
			
			set
			{
				m_ConditionName = value;
			}
		}
		/// <remarks>
		/// <see cref="ConditionName"/>
		/// </remarks>
		private string m_ConditionName = "";

		/// <summary>
		///
		/// </summary>
		public object ConditionValue
		{
			get
			{
				return m_ConditionValue;
			}
			
			set
			{
				m_ConditionValue = value;
			}
		}
		/// <remarks>
		/// <see cref="ConditionValue"/>
		/// </remarks>
		private object m_ConditionValue;

		/// <summary>
		/// Tests if this attribute should be applied/considered when serializing.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public virtual bool Applies(SerializerOptions options)
		{
			if (m_ConditionName.Length > 0)
			{
				object o = options[m_ConditionName];

				if (o == null)
				{
					return false;
				}

				if (!o.Equals(m_ConditionValue))
				{
					return false;
				}				
			}

			return true;
		}
	}

	[
		AttributeUsage
		(
		AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Interface | AttributeTargets.Field |
		AttributeTargets.Property,
		Inherited = true, AllowMultiple = true
		)
	]
	public abstract class XmlApproachAttribute
		: XmlSerializationAttribute
	{
		/// <summary>
		/// The name to serialize the member as.
		/// </summary>
		public virtual string Name
		{
			get
			{
				return m_Name;
			}
			
			set
			{
				m_Name = value;
			}
		}
		/// <remarks>
		/// <see cref="Name"/>
		/// </remarks>
		protected string m_Name = "";

		/// <summary>
		/// The type of the member.  
		/// </summary>
		/// <remarks>
		/// If this property is null then the declared type of the member is used.
		/// </remarks>
		public virtual Type Type
		{
			get
			{
				return m_Type;
			}
			
			set
			{
				m_Type = value;
			}
		}
		/// <remarks>
		/// <see cref="Type"/>
		/// </remarks>
		protected Type m_Type;

		/// <summary>
		/// Property TypeSerializerType (Type)
		/// </summary>
		public virtual Type TypeSerializerType
		{
			get
			{
				return this.m_TypeSerializerType;
			}
			set
			{
				this.m_TypeSerializerType = value;
			}
		}
		/// <summary>
		/// <see cref="TypeSerializerType"/>
		/// </summary>
		protected Type m_TypeSerializerType;

		public XmlApproachAttribute()
		{
		}

		public XmlApproachAttribute(Type type)
			: this("", type)
		{
		}

		public XmlApproachAttribute(string name)
		{
			m_Name = name;
		}

		public XmlApproachAttribute(string name, Type type)
		{
			m_Type = type;
			m_Name = name;
		}
	}

	[
		AttributeUsage
		(
		AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Interface | AttributeTargets.Field |
		AttributeTargets.Property,
		Inherited = true, AllowMultiple = true
		)
	]
	public class XmlAttributeAttribute
		: XmlApproachAttribute
	{
		public XmlAttributeAttribute()
		{
		}

		public XmlAttributeAttribute(Type type)
			: base(type)
		{
		}

		public XmlAttributeAttribute(string name)
			: base(name)
		{			
		}

		public XmlAttributeAttribute(string name, Type type)
			: base(name, type)
		{
		}
	}

	[
		AttributeUsage
			(
				AttributeTargets.Class | AttributeTargets.Struct |
				AttributeTargets.Interface | AttributeTargets.Field |
				AttributeTargets.Property,
				Inherited = true, AllowMultiple = true
			)
	]
	public class XmlElementAttribute
		: XmlApproachAttribute
	{
		public XmlElementAttribute()
		{
		}

		public XmlElementAttribute(Type type)
			: base(type)
		{
		}

		public XmlElementAttribute(string name)
			: base(name)
		{
		}

		public XmlElementAttribute(string name, Type type)
			: base(name, type)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlCDataAttribute
		: XmlSerializationAttribute
	{
		/// <summary>
		///
		/// </summary>
		public bool Enabled
		{
			get
			{
				return m_Enabled;
			}
			
			set
			{
				m_Enabled = value;
			}
		}
		/// <remarks>
		/// <see cref="Enabled"/>
		/// </remarks>
		private bool m_Enabled;

		public XmlCDataAttribute()
		{
			m_Enabled = true;
		}

		public XmlCDataAttribute(bool enabled)
		{
			m_Enabled = enabled;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlTextAttribute
		: XmlSerializationAttribute
	{
	}
}
