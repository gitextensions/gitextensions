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
}