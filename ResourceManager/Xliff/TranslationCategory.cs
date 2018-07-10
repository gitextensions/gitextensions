using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace ResourceManager.Xliff
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class TranslationCategory : IComparable<TranslationCategory>
    {
        public TranslationCategory()
        {
        }

        public TranslationCategory(string name, string sourceLanguage, string targetLanguage = null)
        {
            Name = name;
            SourceLanguage = sourceLanguage;
            TargetLanguage = targetLanguage;
        }

        [XmlAttribute("datatype")]
        public string DataType { get; set; } = "plaintext";

        [XmlAttribute("original")]
        public string Name { get; set; }

        [XmlAttribute("source-language")]
        public string SourceLanguage { get; set; }

        [XmlAttribute("target-language")]
        public string TargetLanguage { get; set; }

        [XmlElement(ElementName = "body")]
        public TranslationBody Body { get; set; } = new TranslationBody();

        public int CompareTo(TranslationCategory other) => StringComparer.InvariantCulture.Compare(Name, other.Name);
    }
}
