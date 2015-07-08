using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace Platform.Xml.Serialization
{
    public class StringWriterWithEncoding : StringWriter
    {
        private Encoding textEncoding { get; set; }
        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding) : base(sb)
        {
            this.textEncoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return textEncoding; }
        }
    }
}
