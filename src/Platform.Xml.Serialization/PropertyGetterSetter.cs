using System.Reflection;

namespace Platform.Xml.Serialization
{
	public class PropertyGetterSetter
		: AbstractGetterSetter
	{
		public PropertyGetterSetter(MemberInfo memberInfo)
			: base(memberInfo)
		{
		}

		public override object GetValue(object obj)
		{
			if (((PropertyInfo)this.memberInfo).CanRead)
			{
				return ((PropertyInfo)this.memberInfo).GetValue(obj, null);
			}
			else
			{
				return null;
			}
		}

		public override void SetValue(object obj, object val)
		{
			if (((PropertyInfo)this.memberInfo).CanWrite)
			{
				((PropertyInfo)this.memberInfo).SetValue(obj, val, null);
			}
		}
	}
}