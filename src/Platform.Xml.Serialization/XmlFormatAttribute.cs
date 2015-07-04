using System;

namespace Platform.Xml.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class XmlFormatAttribute : XmlSerializationAttribute
    {
        public string Format
        {
            get;
            set;
        }

        public XmlFormatAttribute(string format)
        {
            this.Format = format;
        }
    }
}