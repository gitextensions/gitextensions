using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace ResourceManager.Translation
{
    [DebuggerDisplay("{name}.{property}={value}")]
    public class TranslationItem : IComparable<TranslationItem>
    {
        public TranslationItem()
        {

        }
        public TranslationItem(string name, string property, string value)
        {
            this.name = name;
            this.property = property;
            this.value = value;
        }

        private string name;
        [XmlAttribute]
        public string Name 
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string property;
        [XmlAttribute]
        public string Property 
        {
            get
            {
                return property;
            }
            set
            {
                property = value;
            }
        }

        private string value;
        public string Value 
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public int CompareTo(TranslationItem other)
        {
            int val = Name.CompareTo(other.Name);
            if (val == 0) val = Property.CompareTo(other.Property);
            return val;
        }
    }
}
