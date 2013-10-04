﻿using System;
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
            return JsonSerializer.Serialize(items);
        }

        public static CommitTemplateItem[] DeserializeCommitTemplates(string serializedString, out bool shouldBeUpdated)
        {
            shouldBeUpdated = false;
            if (string.IsNullOrEmpty(serializedString))
                return null;

            CommitTemplateItem[] commitTemplateItem = null;
            try
            {
                commitTemplateItem = JsonSerializer.Deserialize<CommitTemplateItem[]>(serializedString);
            }
            catch (Exception)
            {
            }

            if (commitTemplateItem == null)
            {
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
                            BinaryFormatter sf = new BinaryFormatter() {Binder = new MoveNamespaceDeserializationBinder()};
                            commitTemplateItem = (CommitTemplateItem[]) sf.Deserialize(rs);
                        }
                    }
                    else
                    {
                        var serializer = new XmlSerializer(typeof (CommitTemplateItem[]));
                        using (var stringReader = new StringReader(serializedString))
                        {
                            var xmlReader = new XmlTextReader(stringReader);
                            commitTemplateItem = serializer.Deserialize(xmlReader) as CommitTemplateItem[];
                        }
                    }
                    shouldBeUpdated = true;
                }
                catch (Exception /*e*/)
                {
                    return null;
                }
            }

            return commitTemplateItem;
        }
    
    }

    public sealed class MoveNamespaceDeserializationBinder : SerializationBinder
    {
        private const string OldNamespace = "GitUI";
        private const string NewNamespace = "GitUI.CommandsDialogs.CommitDialog";

        public override Type BindToType(string assemblyName, string typeName)
        {
            typeName = typeName.Replace(OldNamespace, NewNamespace);
            //assemblyName = assemblyName.Replace(OldNamespace, NewNamespace);
            var type = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            return type;
        }
    }

    public static class JsonSerializer
    {
        public static string Serialize<T>(T myObject)
        {
            var json = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
            var stream = new System.IO.MemoryStream();
            json.WriteObject(stream, myObject);
            return System.Text.Encoding.Default.GetString(stream.ToArray());
        }

        public static T Deserialize<T>(string myString)
        {
            var json = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
            var stream = new System.IO.MemoryStream(System.Text.Encoding.Unicode.GetBytes(myString));
            return (T)json.ReadObject(stream);
        }
    }
}
