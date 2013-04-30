using System.Reflection;

namespace Platform.Xml.Serialization
{
	public abstract class AbstractGetterSetter
		: IGetterSetter
	{
		protected MemberInfo memberInfo;

		protected AbstractGetterSetter(MemberInfo memberInfo)
		{
			this.memberInfo = memberInfo;
		}

		public abstract object GetValue(object obj);
		public abstract void SetValue(object obj, object val);
	}
}