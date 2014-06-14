using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace ResourceManager.Xliff
{
    [DebuggerDisplay("{name}")]
    public class TranslationCategory : IComparable<TranslationCategory>
    {
        public TranslationCategory()
        {
        }

        public TranslationCategory(string name, string source)
        {
            this._name = name;
            this._source = source;
        }

        private string _datatype = "plaintext";
        [XmlAttribute("datatype")]
        public string Datatype
        {
            get
            {
                return _datatype;
            }
            set
            {
                _datatype = value;
            }
        }

        private string _name;
        [XmlAttribute("original")]
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

        private string _source;
        [XmlAttribute("source-language")]
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

        private TranslationBody _body = new TranslationBody();
        [XmlElement(ElementName = "body")]
        public TranslationBody Body 
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value;
            }
        }

        public int CompareTo(TranslationCategory other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}
