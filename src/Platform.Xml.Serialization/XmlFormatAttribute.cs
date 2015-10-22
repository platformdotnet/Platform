using System;
using System.Globalization;

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

        private bool cultureSet = false;

        public bool CultureSet
        {
            get { return cultureSet; }
        }

        private string culture;

        public string Culture
        {
            get { return culture; }
            set { cultureSet = true;
                culture = value;
            }
        }

        public CultureInfo CultureInfo
        {
            get
            {
                return this.CultureSet ? new CultureInfo(this.Culture) : CultureInfo.CurrentCulture;
            }
        }

        public XmlFormatAttribute(string format)
        {
            this.Format = format;
        }

        public XmlFormatAttribute(string format, string culture) : this(format)
        {
            this.Culture = culture;
        }
    }
}