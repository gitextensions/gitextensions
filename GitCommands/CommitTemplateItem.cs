using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using GitCommands.Utils;

namespace GitCommands
{
    [Serializable]
    public sealed class CommitTemplateItem : ISerializable
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public Image? Icon { get; set; }

        public CommitTemplateItem(string name, string text, Image? icon)
        {
            Name = name;
            Text = text;
            Icon = icon;
        }

        public CommitTemplateItem()
        {
            Name = string.Empty;
            Text = string.Empty;
            Icon = null;
        }

        private CommitTemplateItem(SerializationInfo info, StreamingContext context)
        {
            Name = (string)info.GetValue("Name", typeof(string));
            Text = (string)info.GetValue("Text", typeof(string));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Text", Text);
        }

        public static void SaveToSettings(CommitTemplateItem[]? items)
        {
            string strVal = SerializeCommitTemplates(items);
            AppSettings.CommitTemplates = strVal;
        }

        public static CommitTemplateItem[]? LoadFromSettings()
        {
            string serializedString = AppSettings.CommitTemplates;
            var templates = DeserializeCommitTemplates(serializedString, out var shouldBeUpdated);
            if (shouldBeUpdated)
            {
                SaveToSettings(templates!);
            }

            return templates;
        }

        private static string SerializeCommitTemplates(CommitTemplateItem[]? items)
        {
            return JsonSerializer.Serialize(items);
        }

        private static CommitTemplateItem[]? DeserializeCommitTemplates(string serializedString, out bool shouldBeUpdated)
        {
            shouldBeUpdated = false;
            if (string.IsNullOrEmpty(serializedString))
            {
                return null;
            }

            CommitTemplateItem[]? commitTemplateItem = null;
            try
            {
                commitTemplateItem = JsonSerializer.Deserialize<CommitTemplateItem[]>(serializedString);
            }
            catch
            {
                // do nothing
            }

            if (commitTemplateItem is null)
            {
                try
                {
                    int p = serializedString.IndexOf(':');
                    int length = Convert.ToInt32(serializedString.Substring(0, p));

                    byte[] memoryData = Convert.FromBase64String(serializedString.Substring(p + 1));
                    using var rs = new MemoryStream(memoryData, 0, length);
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    var sf = new BinaryFormatter { Binder = new MoveNamespaceDeserializationBinder() };
                    commitTemplateItem = (CommitTemplateItem[])sf.Deserialize(rs);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

                    shouldBeUpdated = true;
                }
                catch
                {
                    return null;
                }
            }

            return commitTemplateItem;
        }
    }

    public sealed class MoveNamespaceDeserializationBinder : SerializationBinder
    {
        private const string OldNamespace = "GitUI.CommandsDialogs.CommitDialog";
        private const string NewNamespace = "GitCommands";

        public override Type? BindToType(string assemblyName, string typeName)
        {
            typeName = typeName.Replace(OldNamespace, NewNamespace);

            return Type.GetType($"{typeName}, {assemblyName}");
        }
    }
}
