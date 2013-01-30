using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace ResourceManager.Translation
{
    public enum TranslationType
    {
        [XmlEnum(Name = "translated")]
        Translated,
        [XmlEnum(Name = "unfinished")]
        Unfinished,
        [XmlEnum(Name = "obsolete")]
        Obsolete,
        [XmlEnum(Name = "new")]
        New
    }

    [DebuggerDisplay("{name}.{property}={value}")]
    public class TranslationItem : IComparable<TranslationItem>, ICloneable
    {
        public TranslationItem()
        {

        }

        public TranslationItem(string name, string property, string source)
        {
            this.name = name;
            this.property = property;
            this.source = source;
            this.status = TranslationType.New;
        }

        public TranslationItem(string name, string property, string source, string value, TranslationType status)
        {
            this.name = name;
            this.property = property;
            this.source = source;
            this.value = value;
            this.status = status;
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

        private TranslationType status;
        [XmlAttribute("type"), DefaultValue(0)]
        public TranslationType Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }

        private string source;
        public string Source
        {
            get
            {
                return source;
            }
            set
            {
                this.source = value;
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
            int val = String.Compare(Name, other.Name, StringComparison.Ordinal);
            if (val == 0) val = String.Compare(Property, other.Property, StringComparison.Ordinal);
            return val;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public TranslationItem Clone()
        {
            return new TranslationItem(name, property, source, value, status);
        }
    }
}
