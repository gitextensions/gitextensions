using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace GitUI
{
    [Serializable()]
    public class CommitTemplateItem : ISerializable
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        private static bool _useBinaryFormatter = true;

        public CommitTemplateItem(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public CommitTemplateItem()
        {
            Name = String.Empty;
            Text = String.Empty;
        }

        public CommitTemplateItem(SerializationInfo info, StreamingContext ctxt)
        {
            Name = (string)info.GetValue("Name", typeof(string));
            Text = (string)info.GetValue("Text", typeof(string));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Name", Name);
            info.AddValue("Text", Text);
        }

        public static string SerializeCommitTemplates(CommitTemplateItem[] items)
        {
            try
            {
                if (_useBinaryFormatter)
                {
                    // Serialize to a base 64 string
                    byte[] bytes;
                    long length = 0;
                    MemoryStream ws = new MemoryStream();
                    BinaryFormatter sf = new BinaryFormatter();
                    sf.Serialize(ws, items);
                    length = ws.Length;
                    bytes = ws.GetBuffer();
                    return bytes.Length.ToString() + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
                }
                else
                {
                    var sw = new StringWriter();
                    var serializer = new XmlSerializer(typeof(CommitTemplateItem[]));
                    serializer.Serialize(sw, items);
                    return sw.ToString();
                }
            }
            catch
            {
                return null;
            }
        }

        public static CommitTemplateItem[] DeserializeCommitTemplates(string serializedString)
        {
            if (string.IsNullOrEmpty(serializedString))
                return null;

            CommitTemplateItem[] commitTemplateItem = null;
            try
            {
                if (_useBinaryFormatter)
                {
                    int p = serializedString.IndexOf(':');
                    int length = Convert.ToInt32(serializedString.Substring(0, p));

                    byte[] memorydata = Convert.FromBase64String(serializedString.Substring(p + 1));
                    MemoryStream rs = new MemoryStream(memorydata, 0, length);
                    BinaryFormatter sf = new BinaryFormatter();
                    commitTemplateItem = (CommitTemplateItem[])sf.Deserialize(rs);
                }
                else
                {
                    var serializer = new XmlSerializer(typeof(CommitTemplateItem[]));
                    using (var stringReader = new StringReader(serializedString))
                    using (var xmlReader = new XmlTextReader(stringReader))
                    {
                        commitTemplateItem = serializer.Deserialize(xmlReader) as CommitTemplateItem[];
                    }
                }
            }
            catch (Exception /*e*/)
            {
                return null;
            }

            return commitTemplateItem;
        }
    
    }
}
