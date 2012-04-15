using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GitUI
{
    public class CommitTemplateItem
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

        public static string SerializeCommitTemplatesIntoXml(CommitTemplateItem[] items)
        {
            try
            {
                var sw = new StringWriter();
                var serializer = new XmlSerializer(typeof(CommitTemplateItem[]));
                serializer.Serialize(sw, items);
                return sw.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static CommitTemplateItem[] DeserializeCommitTemplatesFromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return null;

            CommitTemplateItem[] commitTemplateItem = null;
            try
            {
                var serializer = new XmlSerializer(typeof(CommitTemplateItem[]));
                using (var stringReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    commitTemplateItem = serializer.Deserialize(xmlReader) as CommitTemplateItem[];
                }
            }
            catch
            {
                return null;
            }

            return commitTemplateItem;
        }
    
    }
}
