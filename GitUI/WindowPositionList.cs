using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GitUI
{
    /// <summary>
    ///   Stores the state and position of a single window
    /// </summary>
    [DebuggerDisplay("Rect={Rect} State={State}")]
    public class WindowPosition
    {
        public WindowPosition(Rectangle rect, FormWindowState state)
        {
            Rect = rect;
            State = state;
        }

        public Rectangle Rect { get; private set; }
        public FormWindowState State { get; private set; }
    }

    /// <summary>
    ///   A Hashtable for storing WindowPosition objects with the ability to
    ///   serialize them to the user's settings.
    /// </summary>
    public class WindowPositionList : Hashtable, IXmlSerializable
    {
        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("window");
                var name = reader.ReadElementString("name");
                var state =
                    (FormWindowState) TypeDescriptor.GetConverter(typeof (FormWindowState))
                                          .ConvertFromString(reader.ReadElementString("state"));
                var rect =
                    (Rectangle) TypeDescriptor.GetConverter(typeof (Rectangle))
                                    .ConvertFromString(reader.ReadElementString("position"));
                reader.ReadEndElement();
                Add(name, new WindowPosition(rect, state));
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var key in Keys)
            {
                var position = (WindowPosition) this[key];
                writer.WriteStartElement("window");
                writer.WriteElementString("name", (String) key);
                writer.WriteElementString(
                    "state",
                    TypeDescriptor.GetConverter(position.State).ConvertToString(position.State));
                writer.WriteElementString(
                    "position",
                    TypeDescriptor.GetConverter(position.Rect).ConvertToString(position.Rect));
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}