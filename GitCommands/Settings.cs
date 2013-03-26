using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GitCommands.Config;
using GitCommands.Logging;
using GitCommands.Repository;
using System.ComponentModel;

namespace GitCommands.Properties
{

    public enum LocalChangesAction
    {
        DontChange,
        Merge,
        Reset,
        Stash
    }
    public enum PullAction
    {
        None,
        Merge,
        Rebase,
        Fetch,
        FetchAll
    }
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class Settings
    {



        public readonly string GitExtensionsVersionString;
        public readonly int GitExtensionsVersionInt;
        public readonly char PathSeparator = '\\';
        public readonly char PathSeparatorWrong = '/';
        public string ApplicationDataPath { get; private set; }

        public XmlSerializableDictionary<string, GitCommands.Properties.PullAction> LastPullAction { get; private set; }
        public XmlSerializableDictionary<string, string> PluginsSettings { get; private set; }
        public XmlSerializableDictionary<string, bool> HelpExpanded { get; private set; }

        public CommandLogger GitLog { get; private set; }
        public string GravatarCachePath
        {
            get { return ApplicationDataPath + "Images\\"; }
        }

        public Settings()
        {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            this.SettingsSaving += this.SettingsSavingEventHandler;
            //

            Action<Encoding> addEncoding = delegate(Encoding e) { AvailableEncodings[e.HeaderName] = e; };
            addEncoding(Encoding.Default);
            addEncoding(new ASCIIEncoding());
            addEncoding(new UnicodeEncoding());
            addEncoding(new UTF7Encoding());
            addEncoding(new UTF8Encoding());

            Version version = Assembly.GetCallingAssembly().GetName().Version;
            GitExtensionsVersionString = version.Major.ToString() + '.' + version.Minor.ToString();
            GitExtensionsVersionInt = version.Major * 100 + version.Minor;
            if (version.Build > 0)
            {
                GitExtensionsVersionString += '.' + version.Build.ToString();
                GitExtensionsVersionInt = GitExtensionsVersionInt * 100 + version.Build;
            }
            if (!RunningOnWindows())
            {
                PathSeparator = '/';
                PathSeparatorWrong = '\\';
            }

            GitLog = new CommandLogger();

          

            //Make applicationdatapath version dependent
            if (this.IsPortable)
            {
                ApplicationDataPath = System.IO.Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
                SetupPortableProvider();
            }
            else
            {
                ApplicationDataPath = Application.UserAppDataPath.Replace(Application.ProductVersion, string.Empty);

            }
            ReadLastPullAction();
            ReadPluginsSettings();
            ReadHelpExpanded();
        }
        private void SetupPortableProvider()
        {

            

            PortableSettingsProvider pv = new PortableSettingsProvider();
            this.Providers.Add(pv);

            System.Configuration.SettingsPropertyCollection props = new System.Configuration.SettingsPropertyCollection();
            Dictionary<SettingsProperty, SettingsProvider> dpv = new Dictionary<SettingsProperty, SettingsProvider>();
            foreach (SettingsProperty prop in Properties)
            {
                System.Configuration.UserScopedSettingAttribute at = (from DictionaryEntry a in prop.Attributes
                                                                      where a.Value.GetType() == typeof(System.Configuration.UserScopedSettingAttribute)
                                                                      select (System.Configuration.UserScopedSettingAttribute)a.Value).FirstOrDefault();



                if (at != null)
                {

                    props.Add(prop);
                    dpv.Add(prop, prop.Provider);
                }
            }

            System.Configuration.SettingsPropertyValueCollection pvals =
               (from p in dpv.Values.Distinct() where p.GetType() == typeof(LocalFileSettingsProvider) select p.GetPropertyValues(this.Context, props)).FirstOrDefault();

            foreach (SettingsProperty prop in props)
                prop.Provider = pv;
            this.Reload();
            

        }
        
        private void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            ////Auto Save functionality if needed.
            //string[] ignore = { "LastPullActionString", "PluginsSettingsString", "UpgradeSettings" };
            //bool IgnoreSetting = false;
            //foreach (string ig in ignore)
            //{

            //    if (e.PropertyName == ig)
            //    {
            //        IgnoreSetting = true;
            //        break;
            //    }
            //}
            //if (!IgnoreSetting)
            //    this.Save();
        }
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {

            // Add code to handle the SettingChangingEvent event here.


        }
        private System.Text.StringBuilder SaveXMLDicSettings<T>(XmlSerializableDictionary<string, T> Dic)
        {
            StringBuilder sb = new StringBuilder();
            using (System.IO.StringWriter sw = new System.IO.StringWriter(sb))
            {
                using (System.Xml.XmlTextWriter xtw = new System.Xml.XmlTextWriter(sw))
                {
                    xtw.WriteStartElement("dictionary");
                    
                    Dic.WriteXml(xtw);
                    xtw.WriteEndElement();
                }
            }
            return sb;
        }
        private XmlSerializableDictionary<string, T> ReadXMLDicSettings<T>(XmlSerializableDictionary<string, T> Dic, string StringSetting)
        {
            Dic = new XmlSerializableDictionary<string, T>();
            if (!string.IsNullOrWhiteSpace(StringSetting))
            { 
            using (System.IO.StringReader str = new System.IO.StringReader(StringSetting))
            {
                using (System.Xml.XmlTextReader xr = new System.Xml.XmlTextReader(str))
                {
                   Dic.ReadXml(xr);
                }
            }
            }
            return Dic;
        }

        private void ReadHelpExpanded()
        {
            HelpExpanded = ReadXMLDicSettings(HelpExpanded, this.HelpExpandedString);
        }
        private void ReadLastPullAction()
        {
            LastPullAction = ReadXMLDicSettings(LastPullAction, this.LastPullActionString);

        }
        private void ReadPluginsSettings()
        {
            PluginsSettings = ReadXMLDicSettings(PluginsSettings, this.PluginsSettingsString);

        }
        private void SaveHelpExpanded()
        {
            StringBuilder sb = SaveXMLDicSettings(HelpExpanded);
            this.HelpExpandedString = sb.ToString();
        }
        private void SaveLastPullAction()
        {
            StringBuilder sb = SaveXMLDicSettings(LastPullAction);
            this.LastPullActionString = sb.ToString();
        }
        private void SavePluginsSettings()
        {
            StringBuilder sb = SaveXMLDicSettings(PluginsSettings);
            this.PluginsSettingsString = sb.ToString();
        }
        public string getPluginSetting(string SettingName)
        {
            if (string.IsNullOrWhiteSpace(SettingName))
                return null;
            if (PluginsSettings.ContainsKey(SettingName))
                return PluginsSettings[SettingName];
            else
                return null;
        }
        public bool getHelpExpanded(string ID, bool DefaultValue)
        {
            if (HelpExpanded.ContainsKey(ID))
                return HelpExpanded[ID];
            else
                return DefaultValue;
        }

        public void setHelpExpanded(string ID, bool Value)
        {
            if (HelpExpanded.ContainsKey(ID))
                HelpExpanded[ID] = Value;
            else
                HelpExpanded.Add(ID, Value);
        }

        public void setPluginSetting(string SettingName, string Value)
        {
            if (string.IsNullOrWhiteSpace(SettingName))
                return;
            if (PluginsSettings.ContainsKey(SettingName))
                PluginsSettings[SettingName] = Value;
            else
                PluginsSettings.Add(SettingName, Value);
        }
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Add code to handle the SettingsSaving event here.
            SaveLastPullAction();
            SavePluginsSettings();
            SaveHelpExpanded();
        }

        public static string GetGitExtensionsFullPath()
        {
            return GetGitExtensionsDirectory() + "\\GitExtensions.exe";
        }

        public static string GetGitExtensionsDirectory()
        {
            string fileName = Assembly.GetAssembly(typeof(Settings)).Location;
            fileName = fileName.Substring(0, fileName.LastIndexOfAny(new[] { '\\', '/' }));
            return fileName;
        }

        public static string GetDictionaryDir()
        {
            return GetGitExtensionsDirectory() + "\\Dictionaries\\";
        }



        public bool RunningOnWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return true;
                default:
                    return false;
            }
        }

        public bool RunningOnUnix()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    return true;
                default:
                    return false;
            }
        }
        public bool IsMonoRuntime()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
        public bool RunningOnMacOSX()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    return true;
                default:
                    return false;
            }
        }
        public bool CommitInfoShowContainedInBranches
        {
            get
            {
                return CommitInfoShowContainedInBranchesLocal ||
                    CommitInfoShowContainedInBranchesRemote ||
                    CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            }
        }

        public readonly Dictionary<string, Encoding> AvailableEncodings = new Dictionary<string, Encoding>();
        private readonly Dictionary<string, Encoding> EncodingSettings = new Dictionary<string, Encoding>();

        internal bool GetEncoding(string settingName, out Encoding encoding)
        {
            lock (EncodingSettings)
            {
                return EncodingSettings.TryGetValue(settingName, out encoding);
            }
        }

        internal void SetEncoding(string settingName, Encoding encoding)
        {
            lock (EncodingSettings)
            {
                var items = EncodingSettings.Keys.Where(item => item.StartsWith(settingName)).ToList();
                foreach (var item in items)
                    EncodingSettings.Remove(item);
                EncodingSettings[settingName] = encoding;
            }
        }
    }
}
