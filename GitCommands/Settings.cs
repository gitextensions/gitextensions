using System.Diagnostics;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System;
using GitCommands.Config;
using GitCommands.Logging;
using GitCommands.Repository;
using System.Windows.Forms;
using System.Text;
using System.Linq;
using System.Collections;
namespace GitCommands.Properties {

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
    public sealed partial class Settings {
        

        public readonly string GitExtensionsVersionString;
        public readonly int GitExtensionsVersionInt;
        public readonly char PathSeparator = '\\';
        public readonly char PathSeparatorWrong = '/';
        public  string ApplicationDataPath { get; private set; }

        public CommandLogger GitLog { get; private set; }
        public  string GravatarCachePath
        {
            get { return ApplicationDataPath + "Images\\"; }
        }

        public Settings() {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //

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
                ApplicationDataPath = Assembly.GetCallingAssembly().Location;
            else
                ApplicationDataPath = Application.UserAppDataPath.Replace(Application.ProductVersion, string.Empty);


        }

       
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }
        public  string GetDictionaryDir()
        {
            return GetInstallDir() + "\\Dictionaries\\";
        }

        public  string GetInstallDir()
        {
            return GetValue("InstallDir", "");
        }

        public  void SetInstallDir(string dir)
        {
            if (VersionIndependentRegKey != null)
                SetValue("InstallDir", dir);
        }

        public  bool RunningOnWindows()
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

        public  bool RunningOnUnix()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    return true;
                default:
                    return false;
            }
        }

        public  bool RunningOnMacOSX()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    return true;
                default:
                    return false;
            }
        }

        public  readonly Dictionary<string, Encoding> AvailableEncodings = new Dictionary<string, Encoding>();
        private  readonly Dictionary<string, Encoding> EncodingSettings = new Dictionary<string, Encoding>();

        internal  bool GetEncoding(string settingName, out Encoding encoding)
        {
            lock (EncodingSettings)
            {
                return EncodingSettings.TryGetValue(settingName, out encoding);
            }
        }

        internal  void SetEncoding(string settingName, Encoding encoding)
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
