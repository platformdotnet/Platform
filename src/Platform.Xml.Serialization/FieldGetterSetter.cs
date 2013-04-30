using System.Reflection;

namespace Platform.Xml.Serialization
{
	public class FieldGetterSetter
		: AbstractGetterSetter
	{
		public FieldGetterSetter(MemberInfo memberInfo)
			: base(memberInfo)
		{
		}

		public override object GetValue(object obj)
		{
			return ((FieldInfo)this.memberInfo).GetValue(obj);
		}

		public override void SetValue(object obj, object val)
		{
			if (!((FieldInfo)this.memberInfo).IsInitOnly)
			{
				((FieldInfo)this.memberInfo).SetValue(obj, val);
			}
		}
	}
}