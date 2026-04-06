// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializableDictionary.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BugReporter.Serialization;

[XmlRoot("dictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable where TKey : notnull where TValue : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableDictionary{TKey,TValue}"/> class.
    /// This is the default constructor provided for XML serializer.
    /// </summary>
    public SerializableDictionary()
    {
    }

    public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
        {
            Add(pair.Key, pair.Value);
        }
    }

    public XmlSchema? GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        /*if (reader.IsEmptyElement)
        {
            return;
        }*/
        XmlReader inner = reader.ReadSubtree();

        XElement xElement = XElement.Load(inner);
        if (xElement.HasElements)
        {
            foreach (XElement element in xElement.Elements())
            {
                Add((TKey)Convert.ChangeType(element.Name.ToString(), typeof(TKey)), (TValue)Convert.ChangeType(element.Value, typeof(TValue)));
            }
        }

        inner.Close();

        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        foreach (TKey key in Keys)
        {
            writer.WriteStartElement(key.ToString().Replace(" ", ""));

            try
            {
                writer.WriteValue(this[key]);
            }
            catch (Exception)
            {
                writer.WriteValue(this[key].ToString());
            }

            writer.WriteEndElement();
        }
    }
}
