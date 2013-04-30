namespace Platform.Xml.Serialization
{
	public interface IGetterSetter
	{
		object GetValue(object obj);
		void SetValue(object obj, object val);
	}
}