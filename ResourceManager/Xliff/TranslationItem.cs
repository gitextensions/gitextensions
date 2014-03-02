using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace ResourceManager.Xliff
{
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
        }

        public TranslationItem(string name, string property, string source, string value)
        {
            _name = name;
            _property = property;
            _source = source;
            _value = value;
        }

        private string _name;
        [XmlIgnore]
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
        [XmlIgnore]
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

        [XmlAttribute("id")]
        public string Id
        {
            get
            {
                return _name + "." + _property;
            }
            set
            {
                var vals = value.Split(new[] {'.'}, 2);
                _name = vals[0];
                _property = vals.Length > 1 ? vals[1] : "";
            }
        }

        private string _source;
        [XmlElement("source")]
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

        private string _value;
        [XmlElement("target")]
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
            return new TranslationItem(_name, _property, _source, _value);
        }
    }
}
