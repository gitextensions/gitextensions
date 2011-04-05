using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics;
using GitCommands;
using System.ComponentModel;

namespace GitUI.Script
{
    public static class ScriptManager
    {
        private static BindingList<ScriptInfo> Scripts { get; set; }

        public static BindingList<ScriptInfo> GetScripts()
        {
            if (Scripts == null)
            {
                DeserializeFromXml(Settings.ownScripts);
            }

            return Scripts;
        }

        public static ScriptInfo GetScript(string key)
        {
            foreach (ScriptInfo script in GetScripts())
                if (script.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    return script;

            return null;
        }

        public static string SerializeIntoXml()
        {
            try
            {
                var sw = new StringWriter();
                var serializer = new XmlSerializer(typeof(BindingList<ScriptInfo>));
                serializer.Serialize(sw, Scripts);
                return sw.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static void DeserializeFromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return;

            try
            {
                var serializer = new XmlSerializer(typeof(BindingList<ScriptInfo>));
                using (var stringReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    Scripts = serializer.Deserialize(xmlReader) as BindingList<ScriptInfo>;
                }
            }
            catch (Exception ex)
            {
                Scripts = new BindingList<ScriptInfo>();
                DeserializeFromOldFormat(xml);
                
                Trace.WriteLine(ex.Message);                
            }
        }

        private static void DeserializeFromOldFormat(string inputString)
        {
            string PARAM_SEPARATOR = "<_PARAM_SEPARATOR_>";
            string SCRIPT_SEPARATOR = "<_SCRIPT_SEPARATOR_>";

            if (inputString.Contains(PARAM_SEPARATOR) || inputString.Contains(SCRIPT_SEPARATOR))
            {
                Scripts = new BindingList<ScriptInfo>();

                string[] scripts = inputString.Split(new string[] { SCRIPT_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < scripts.Length; i++)
                {
                    string[] parameters = scripts[i].Split(new string[] { PARAM_SEPARATOR }, StringSplitOptions.None);

                    ScriptInfo scriptInfo = new ScriptInfo();
                    scriptInfo.Name = parameters[0];
                    scriptInfo.Command = parameters[1];
                    scriptInfo.Arguments = parameters[2];
                    scriptInfo.AddToRevisionGridContextMenu = parameters[3].Equals("yes");
                    scriptInfo.Enabled = true;

                    Scripts.Add(scriptInfo);
                }
            }
        }
    }
}
