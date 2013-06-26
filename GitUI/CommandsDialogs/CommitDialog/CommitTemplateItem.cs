using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace GitUI.CommandsDialogs.CommitDialog
{
    [Serializable]
    public sealed class CommitTemplateItem : ISerializable
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

        private const bool UseBinaryFormatter = true;

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

        private CommitTemplateItem(SerializationInfo info, StreamingContext ctxt)
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
                Func<bool> fncUseBinaryFormatter = () => UseBinaryFormatter;
                if (fncUseBinaryFormatter())
                {
                    // Serialize to a base 64 string
                    byte[] bytes;
                    using (MemoryStream ws = new MemoryStream())
                    {
                        BinaryFormatter sf = new BinaryFormatter();
                        sf.Serialize(ws, items);
                        bytes = ws.GetBuffer();
                    }
                    return bytes.Length.ToString() + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
                }
                else
                {
                    using (var sw = new StringWriter())
                    {
                        var serializer = new XmlSerializer(typeof(CommitTemplateItem[]));
                        serializer.Serialize(sw, items);
                        return sw.ToString();
                    }
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
                Func<bool> fncUseBinaryFormatter = () => UseBinaryFormatter;
                if (fncUseBinaryFormatter())
                {
                    int p = serializedString.IndexOf(':');
                    int length = Convert.ToInt32(serializedString.Substring(0, p));

                    byte[] memorydata = Convert.FromBase64String(serializedString.Substring(p + 1));
                    using (MemoryStream rs = new MemoryStream(memorydata, 0, length))
                    {
                        BinaryFormatter sf = new BinaryFormatter();
                        commitTemplateItem = (CommitTemplateItem[])sf.Deserialize(rs);
                    }
                }
                else
                {
                    var serializer = new XmlSerializer(typeof(CommitTemplateItem[]));
                    using (var stringReader = new StringReader(serializedString))
                    {
                        var xmlReader = new XmlTextReader(stringReader);
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
