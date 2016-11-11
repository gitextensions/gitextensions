﻿using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace ResourceManager.Xliff
{
    [DebuggerDisplay("{_name}")]
    public class TranslationCategory : IComparable<TranslationCategory>
    {
        public TranslationCategory()
        {
        }

        public TranslationCategory(string name, string sourceLanguage, string targetLanguage = null)
        {
            _name = name;
            _sourceLanguage = sourceLanguage;
            _targetLanguage = targetLanguage;
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

        private string _sourceLanguage;
        [XmlAttribute("source-language")]
        public string SourceLanguage
        {
            get { return _sourceLanguage; }
            set { _sourceLanguage = value; }
        }

        private string _targetLanguage;
        [XmlAttribute("target-language")]
        public string TargetLanguage
        {
            get { return _targetLanguage; }
            set { _targetLanguage = value; }
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
