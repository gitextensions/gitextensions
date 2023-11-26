using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GitCommands
{
    [XmlRoot("dictionary")]
    [Serializable]
    public class XmlSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public XmlSerializableDictionary()
        {
        }

#pragma warning disable SYSLIB0051 // This ctor is obsolete: 'This API supports obsolete formatter-based serialization. It should not be called or extended by application code.'
        protected XmlSerializableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#pragma warning restore SYSLIB0051

        #region IXmlSerializable Members

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer keySerializer = new(typeof(TKey));
            XmlSerializer valueSerializer = new(typeof(TValue));
            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (reader.NodeType == XmlNodeType.XmlDeclaration)
            {
                reader.Read();
            }

            if (wasEmpty || reader.IsEmptyElement)
            {
                return;
            }

            if (reader.ReadToDescendant("item"))
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("item");

                    reader.ReadStartElement("key");
                    TKey key = (TKey)keySerializer.Deserialize(reader);
                    reader.ReadEndElement();

                    reader.ReadStartElement("value");
                    TValue value = (TValue)valueSerializer.Deserialize(reader);
                    reader.ReadEndElement();

                    reader.ReadEndElement();
                    reader.MoveToContent();

                    this[key] = value;
                }
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer keySerializer = new(typeof(TKey));
            XmlSerializer valueSerializer = new(typeof(TValue));

            foreach ((TKey key, TValue value) in this.OrderBy(pair => pair.Key))
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
