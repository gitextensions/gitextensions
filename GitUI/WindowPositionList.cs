using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace GitUI
{
    /// <summary>
    /// Stores the state and position of a single window
    /// </summary>
    public class WindowPosition
    {
        public FormWindowState state;
        public Rectangle rect;
    }

    /// <summary>
    /// A Hashtable for storing WindowPosition objects with the ability to
    /// serialize them to the user's settings.
    /// </summary>
    public class WindowPositionList : Hashtable, IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.Read();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                WindowPosition p = new WindowPosition();
                reader.ReadStartElement("window");
                string name = reader.ReadElementString("name");
                p.state =
                    (FormWindowState)TypeDescriptor.GetConverter(typeof(FormWindowState))
                    .ConvertFromString(reader.ReadElementString("state"));
                p.rect =
                    (Rectangle)TypeDescriptor.GetConverter(typeof(Rectangle))
                    .ConvertFromString(reader.ReadElementString("position"));
                reader.ReadEndElement();
                this.Add(name, p);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (object name in this.Keys)
            {
                WindowPosition p = (WindowPosition)this[name];
                writer.WriteStartElement("window");
                writer.WriteElementString("name", (String)name);
                writer.WriteElementString("state",
                    TypeDescriptor.GetConverter(p.state).ConvertToString(p.state));
                writer.WriteElementString("position",
                    TypeDescriptor.GetConverter(p.rect).ConvertToString(p.rect));
                writer.WriteEndElement();
            }
        }
    }
}
