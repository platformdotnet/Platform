using System;
using System.Globalization;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]	
	public class XmlDateTimeFormatAttribute
		: XmlSerializationAttribute
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
            set
            {
                cultureSet = true;
                culture = value;
            }
        }

        public bool UseUserOverride
        {
            get;
            set;
        }

        public CultureInfo CultureInfo
        {
            get
            {
                return this.CultureSet ? new CultureInfo(this.Culture, this.UseUserOverride) : CultureInfo.CurrentCulture;
            }
        }

        public XmlDateTimeFormatAttribute(string format)
        {
            this.Format = format;
        }

        public XmlDateTimeFormatAttribute(string format, string culture) : this(format)
        {
            this.Culture = culture;
            this.UseUserOverride = true;
        }
        public XmlDateTimeFormatAttribute(string format, bool useUserOverride) : this(format)
        {
            this.Culture = CultureInfo.CurrentCulture.Name;
            this.UseUserOverride = useUserOverride;
        }

        public XmlDateTimeFormatAttribute(string format, string culture, bool useUserOverride) : this(format)
        {
            this.Culture = culture;
            this.UseUserOverride = useUserOverride;
        }
    }
}