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
            _name = name;
            _property = property;
            _source = source;
            _status = TranslationType.New;
        }

        public TranslationItem(string name, string property, string source, string oldSource, string value, TranslationType status)
        {
            _name = name;
            _property = property;
            _source = source;
            _oldSource = oldSource;
            _value = value;
            _status = status;
        }

        private string _name;
        [XmlAttribute]
        public string Name 
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private string _property;
        [XmlAttribute]
        public string Property
        {
            get
            {
                return _property;
            }
            set
            {
                _property = value;
            }
        }

        private TranslationType _status;
        [XmlAttribute("type"), DefaultValue(0)]
        public TranslationType Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }

        private string _source;
        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        private string _oldSource;
        public string OldSource
        {
            get
            {
                return _oldSource;
            }
            set
            {
                _oldSource = value;
            }
        }

        private string _value;
        public string Value 
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
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
            return new TranslationItem(_name, _property, _source, _oldSource, _value, _status);
        }
    }
}
