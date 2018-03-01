using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using GitCommands;
using System.Collections.Generic;

namespace GitUI.Script
{
    public static class ScriptManager
    {
        private static BindingList<ScriptInfo> Scripts { get; set; }

        public static BindingList<ScriptInfo> GetScripts()
        {
            if (Scripts == null)
            {
                DeserializeFromXml(AppSettings.ownScripts);
                if (Scripts != null)
                    FixAmbiguousHotkeyCommandIdentifiers(Scripts);
            }

            return Scripts;
        }

        private static void FixAmbiguousHotkeyCommandIdentifiers(BindingList<ScriptInfo> scripts)
        {
            ISet<int> ids = new HashSet<int>();
            foreach (ScriptInfo si in scripts)
            {
                if (ids.Contains(si.HotkeyCommandIdentifier))
                {
                    si.HotkeyCommandIdentifier = NextHotkeyCommandIdentifier();
                }
                ids.Add(si.HotkeyCommandIdentifier);
            }
        }

        public static ScriptInfo GetScript(string key)
        {
            foreach (ScriptInfo script in GetScripts())
                if (script.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    return script;

            return null;
        }

        public static void RunEventScripts(GitModuleForm form, ScriptEvent scriptEvent)
        {
            foreach (var scriptInfo in GetScripts().Where(scriptInfo => scriptInfo.Enabled && scriptInfo.OnEvent == scriptEvent))
            {
                ScriptRunner.RunScript(form, form.Module, scriptInfo.Name, null);
            }
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
            // When there is nothing to deserialize, add default scripts
            if (string.IsNullOrEmpty(xml))
            {
                Scripts = new BindingList<ScriptInfo>();
                AddDefaultScripts();
                return;
            }

            try
            {
                var serializer = new XmlSerializer(typeof(BindingList<ScriptInfo>));
                using (var stringReader = new StringReader(xml))
                {
                    var xmlReader = new XmlTextReader(stringReader);
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

        private static void AddDefaultScripts()
        {
            ScriptInfo fetchAfterCommitScript = new ScriptInfo();
            fetchAfterCommitScript.HotkeyCommandIdentifier = 9000;
            fetchAfterCommitScript.Name = "Fetch changes after commit";
            fetchAfterCommitScript.Command = "git";
            fetchAfterCommitScript.Arguments = "fetch";
            fetchAfterCommitScript.RunInBackground = false;
            fetchAfterCommitScript.AskConfirmation = true;
            fetchAfterCommitScript.OnEvent = ScriptEvent.AfterCommit;
            fetchAfterCommitScript.AddToRevisionGridContextMenu = false;
            fetchAfterCommitScript.Enabled = false;
            Scripts.Add(fetchAfterCommitScript);

            ScriptInfo updateSubmodulesAfterPullScript = new ScriptInfo();
            updateSubmodulesAfterPullScript.HotkeyCommandIdentifier = 9001;
            updateSubmodulesAfterPullScript.Name = "Update submodules after pull";
            updateSubmodulesAfterPullScript.Command = "git";
            updateSubmodulesAfterPullScript.Arguments = "submodule update --init --recursive";
            updateSubmodulesAfterPullScript.RunInBackground = false;
            updateSubmodulesAfterPullScript.AskConfirmation = true;
            updateSubmodulesAfterPullScript.OnEvent = ScriptEvent.AfterPull;
            updateSubmodulesAfterPullScript.AddToRevisionGridContextMenu = false;
            updateSubmodulesAfterPullScript.Enabled = false;
            Scripts.Add(updateSubmodulesAfterPullScript);

            ScriptInfo userMenuScript = new ScriptInfo();
            userMenuScript.HotkeyCommandIdentifier = 9002;
            userMenuScript.Name = "Example";
            userMenuScript.Command = "c:\\windows\\system32\\calc.exe";
            userMenuScript.Arguments = "";
            userMenuScript.RunInBackground = false;
            userMenuScript.AskConfirmation = false;
            userMenuScript.OnEvent = ScriptEvent.ShowInUserMenuBar;
            userMenuScript.AddToRevisionGridContextMenu = false;
            userMenuScript.Enabled = false;
            Scripts.Add(userMenuScript);

            ScriptInfo openHashOnGitHub = new ScriptInfo();
            openHashOnGitHub.HotkeyCommandIdentifier = 9003;
            openHashOnGitHub.Name = "Open on GitHub";
            openHashOnGitHub.Command = "{openurl}";
            openHashOnGitHub.Arguments = "https://github.com{cDefaultRemotePathFromUrl}/commit/{sHash}";
            openHashOnGitHub.RunInBackground = false;
            openHashOnGitHub.AskConfirmation = false;
            openHashOnGitHub.OnEvent = 0;
            openHashOnGitHub.AddToRevisionGridContextMenu = true;
            openHashOnGitHub.Enabled = false;
            Scripts.Add(openHashOnGitHub);

            ScriptInfo FetchAll = new ScriptInfo();
            FetchAll.HotkeyCommandIdentifier = 9004;
            FetchAll.Name = "Fetch All Submodules";
            FetchAll.Command = "git";
            FetchAll.Arguments = "submodule foreach --recursive git fetch --all";
            FetchAll.RunInBackground = false;
            FetchAll.AskConfirmation = false;
            FetchAll.OnEvent = 0;
            FetchAll.AddToRevisionGridContextMenu = true;
            FetchAll.Enabled = false;
            Scripts.Add(FetchAll);

        }

        internal static int NextHotkeyCommandIdentifier()
        {
            return GetScripts().Select(s => s.HotkeyCommandIdentifier).Max() + 1;
        }

        private static void DeserializeFromOldFormat(string inputString)
        {
            const string paramSeparator = "<_PARAM_SEPARATOR_>";
            const string scriptSeparator = "<_SCRIPT_SEPARATOR_>";

            if (inputString.Contains(paramSeparator) || inputString.Contains(scriptSeparator))
            {
                Scripts = new BindingList<ScriptInfo>();

                string[] scripts = inputString.Split(new[] { scriptSeparator }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < scripts.Length; i++)
                {
                    string[] parameters = scripts[i].Split(new[] { paramSeparator }, StringSplitOptions.None);

                    ScriptInfo scriptInfo = new ScriptInfo();
                    scriptInfo.Name = parameters[0];
                    scriptInfo.Command = parameters[1];
                    scriptInfo.Arguments = parameters[2];
                    scriptInfo.AddToRevisionGridContextMenu = parameters[3].Equals("yes");
                    scriptInfo.Enabled = true;
                    scriptInfo.RunInBackground = false;

                    Scripts.Add(scriptInfo);
                }
            }
        }
    }
}
