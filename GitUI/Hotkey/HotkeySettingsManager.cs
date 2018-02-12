using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using ResourceManager;
using GitCommands;

namespace GitUI.Hotkey
{
    class HotkeySettingsManager
    {
        #region Serializer
        private static XmlSerializer _Serializer;
        /// <summary>Lazy-loaded Serializer for HotkeySettings[]</summary>
        private static XmlSerializer Serializer
        {
            get
            {
                if (_Serializer == null)
                    _Serializer = new XmlSerializer(typeof(HotkeySettings[]), new[] { typeof(HotkeyCommand) });
                return _Serializer;
            }
        }
        #endregion

        private static List<Keys> UsedKeys = new List<Keys>();

        /// <summary>
        /// Returns whether the hotkey is already assigned.
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        public static bool IsUniqueKey(Keys keyData)
        {
            return UsedKeys.Contains(keyData);
        }

        public static HotkeyCommand[] LoadHotkeys(string name)
        {
            //var settings = LoadSettings().FirstOrDefault(s => s.Name == name);
            HotkeySettings[] allSettings;
            HotkeySettings settings = new HotkeySettings();
            HotkeySettings scriptkeys = new HotkeySettings();
            allSettings = LoadSettings();

            GetUsedHotkeys(allSettings);

            foreach(HotkeySettings hs in allSettings)
            {
                if(hs.Name == name)
                    settings = hs;
                if(hs.Name == "Scripts")
                    scriptkeys = hs;
            }

            //HotkeyCommand[] scriptkeys = LoadSettings().FirstOrDefault(s => s.Name == name);

            if(settings != null) {
                //append general hotkeys to every form
                //HotkeyCommand[] scriptkeys = LoadScriptHotkeys();
                HotkeyCommand[] allkeys = new HotkeyCommand[settings.Commands.Length + scriptkeys.Commands.Length];
                settings.Commands.CopyTo(allkeys,0);
                scriptkeys.Commands.CopyTo(allkeys,settings.Commands.Length);

                return allkeys;
            }

            //return settings != null ? settings.Commands : null;
            return null;
        }

        public static HotkeySettings[] LoadSettings()
        {
            // Get the default settings
            var defaultSettings = CreateDefaultSettings();
            var loadedSettings = LoadSerializedSettings();

            MergeIntoDefaultSettings(defaultSettings, loadedSettings);

            return defaultSettings;
        }

        private static void GetUsedHotkeys(HotkeySettings[] settings)
        {
            UsedKeys.Clear();
            foreach (HotkeySettings hs in settings)
            {
                for (int i = 0; i < hs.Commands.Length; i++)
                {
                    HotkeyCommand hotkeyCommand = hs.Commands[i];

                    if (hotkeyCommand != null && !UsedKeys.Contains(hotkeyCommand.KeyData))
                        UsedKeys.Add(hotkeyCommand.KeyData);
                }
            }
            //MessageBox.Show(UsedKeys.Count.ToString());
        }

        /// <summary>Serializes and saves the supplied settings</summary>
        public static void SaveSettings(HotkeySettings[] settings)
        {
            try
            {
                GetUsedHotkeys(settings);

                StringBuilder strBuilder = new StringBuilder();
                using (StringWriter writer = new StringWriter(strBuilder))
                {
                    Serializer.Serialize(writer, settings);
                    AppSettings.SerializedHotkeys = strBuilder.ToString();
                }
            }
            catch { }
        }

        internal static void MergeIntoDefaultSettings(HotkeySettings[] defaultSettings, HotkeySettings[] loadedSettings)
        {
            if (loadedSettings == null)
            {
                return;
            }

            Dictionary<string, HotkeyCommand> defaultCommands = new Dictionary<string, HotkeyCommand>();
            FillDictionaryWithCommands(defaultCommands, defaultSettings);
            AssignHotkeysFromLoaded(defaultCommands, loadedSettings);
        }

        private static void AssignHotkeysFromLoaded(Dictionary<string, HotkeyCommand> defaultCommands, HotkeySettings[] loadedSettings)
        {
            foreach (HotkeySettings setting in loadedSettings)
            {
                if (setting != null)
                {
                    foreach (HotkeyCommand command in setting.Commands)
                    {
                        if (command != null)
                        {
                            string dictKey = CalcDictionaryKey(setting.Name, command.CommandCode);
                            HotkeyCommand defaultCommand;
                            if (defaultCommands.TryGetValue(dictKey, out defaultCommand))
                            {
                                defaultCommand.KeyData = command.KeyData;
                            }
                        }
                    }
                }
            }
        }

        private static void FillDictionaryWithCommands(Dictionary<string, HotkeyCommand> dict, HotkeySettings[] settings)
        {
            foreach(HotkeySettings setting in settings)
            {
                foreach (HotkeyCommand command in setting.Commands)
                {
                    if (command != null)
                    {
                        string dictKey = CalcDictionaryKey(setting.Name, command.CommandCode);
                        dict.Add(dictKey, command);
                    }
                }
            }
        }

        private static string CalcDictionaryKey(String settingName, int commandCode)
        {
            return settingName + ":" + commandCode;
        }

        private static HotkeySettings[] LoadSerializedSettings()
        {
            HotkeySettings[] settings = null;

            MigrateSettings();

            if (!string.IsNullOrWhiteSpace(AppSettings.SerializedHotkeys))
                settings = LoadSerializedSettings(AppSettings.SerializedHotkeys);

            return settings;
        }

        private static HotkeySettings[] LoadSerializedSettings(string serializedHotkeys)
        {
            HotkeySettings[] settings = null;

            try
            {
                using (StringReader reader = new StringReader(serializedHotkeys))
                {
                    settings = Serializer.Deserialize(reader) as HotkeySettings[];
                }
            }
            catch { }

            return settings;
        }

        private static void MigrateSettings()
        {
            if (AppSettings.SerializedHotkeys == null)
            {
                Properties.Settings.Default.Upgrade();
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.Hotkeys))
                {
                    HotkeySettings[] settings = LoadSerializedSettings(Properties.Settings.Default.Hotkeys);
                    if (settings == null)
                    {
                        AppSettings.SerializedHotkeys = " ";//mark settings as migrated
                    }
                    else
                    {
                        SaveSettings(settings);
                    }
                }
                else
                {
                    AppSettings.SerializedHotkeys = " ";//mark settings as migrated
                }
            }
        }

        /// <summary>Asks the IHotkeyables to create their default hotkey settings</summary>
        public static HotkeySettings[] CreateDefaultSettings()
        {
            Func<object, Keys, HotkeyCommand> hk = (en, k) => new HotkeyCommand((int)en, en.ToString()) { KeyData = k };

            HotkeyCommand[] scriptsHotkeys = LoadScriptHotkeys();


            return new[]
              {

                // FormCommit
                new HotkeySettings(FormCommit.HotkeySettingsName,
                    hk(FormCommit.Commands.AddToGitIgnore, Keys.None),
                    hk(FormCommit.Commands.DeleteSelectedFiles, Keys.Delete),
                    hk(FormCommit.Commands.FocusUnstagedFiles, Keys.Control | Keys.D1),
                    hk(FormCommit.Commands.FocusSelectedDiff, Keys.Control | Keys.D2),
                    hk(FormCommit.Commands.FocusStagedFiles, Keys.Control | Keys.D3),
                    hk(FormCommit.Commands.FocusCommitMessage, Keys.Control | Keys.D4),
                    hk(FormCommit.Commands.ResetSelectedFiles, Keys.R),
                    hk(FormCommit.Commands.StageSelectedFile, Keys.S),
                    hk(FormCommit.Commands.UnStageSelectedFile, Keys.U),
                    hk(FormCommit.Commands.ShowHistory, Keys.H),
                    hk(FormCommit.Commands.ToggleSelectionFilter, Keys.Control | Keys.F),
                    hk(FormCommit.Commands.StageAll, Keys.Control | Keys.S)),
                new HotkeySettings(FormBrowse.HotkeySettingsName,
                    hk(FormBrowse.Commands.GitBash, Keys.Control | Keys.G),
                    hk(FormBrowse.Commands.GitGui, Keys.None),
                    hk(FormBrowse.Commands.GitGitK, Keys.None),
                    hk(FormBrowse.Commands.FocusRevisionGrid, Keys.Control | Keys.D1),
                    hk(FormBrowse.Commands.FocusCommitInfo, Keys.Control | Keys.D2),
                    hk(FormBrowse.Commands.FocusFileTree, Keys.Control | Keys.D3),
                    hk(FormBrowse.Commands.FocusDiff, Keys.Control | Keys.D4),
                    hk(FormBrowse.Commands.Commit, Keys.Control | Keys.Space),
                    hk(FormBrowse.Commands.AddNotes, Keys.Control | Keys.Shift | Keys.N),
                    hk(FormBrowse.Commands.FindFileInSelectedCommit, Keys.Control | Keys.Shift | Keys.F),
                    hk(FormBrowse.Commands.CheckoutBranch, Keys.Control | Keys.Decimal),
                    hk(FormBrowse.Commands.QuickFetch, Keys.Control | Keys.Shift | Keys.Down),
                    hk(FormBrowse.Commands.QuickPull, Keys.Control | Keys.Shift | Keys.P),
                    hk(FormBrowse.Commands.QuickPush, Keys.Control | Keys.Shift | Keys.Up),
                    hk(FormBrowse.Commands.Stash, Keys.Control | Keys.Alt | Keys.Up),
                    hk(FormBrowse.Commands.StashPop, Keys.Control | Keys.Alt | Keys.Down),
                    hk(FormBrowse.Commands.CloseRepository, Keys.Control | Keys.W),
                    hk(FormBrowse.Commands.RotateApplicationIcon, Keys.Control | Keys.Shift | Keys.I)),
                new HotkeySettings(RevisionGrid.HotkeySettingsName,
                    hk(RevisionGrid.Commands.RevisionFilter, Keys.Control | Keys.F),
                    hk(RevisionGrid.Commands.ToggleRevisionGraph, Keys.None),
                    hk(RevisionGrid.Commands.ToggleAuthorDateCommitDate, Keys.None),
                    hk(RevisionGrid.Commands.ToggleOrderRevisionsByDate, Keys.None),
                    hk(RevisionGrid.Commands.ToggleShowRelativeDate, Keys.None),
                    hk(RevisionGrid.Commands.ToggleDrawNonRelativesGray, Keys.None),
                    hk(RevisionGrid.Commands.ToggleShowGitNotes, Keys.None),
                    hk(RevisionGrid.Commands.ToggleRevisionCardLayout, Keys.Control | Keys.Shift | Keys.L),
                    hk(RevisionGrid.Commands.ToggleShowMergeCommits, Keys.Control | Keys.Shift | Keys.M),
                    hk(RevisionGrid.Commands.ShowAllBranches, Keys.Control | Keys.Shift | Keys.A),
                    hk(RevisionGrid.Commands.ShowCurrentBranchOnly, Keys.Control | Keys.Shift | Keys.U),
                    hk(RevisionGrid.Commands.ShowFilteredBranches, Keys.Control | Keys.Shift | Keys.T),
                    hk(RevisionGrid.Commands.ShowRemoteBranches, Keys.Control | Keys.Shift | Keys.R),
                    hk(RevisionGrid.Commands.ShowFirstParent, Keys.Control | Keys.Shift | Keys.S),
                    hk(RevisionGrid.Commands.GoToParent, Keys.Control | Keys.P),
                    hk(RevisionGrid.Commands.GoToChild, Keys.Control | Keys.N),
                    hk(RevisionGrid.Commands.ToggleHighlightSelectedBranch, Keys.Control | Keys.Shift | Keys.B),
                    hk(RevisionGrid.Commands.NextQuickSearch, Keys.Alt | Keys.Down),
                    hk(RevisionGrid.Commands.PrevQuickSearch, Keys.Alt | Keys.Up),
                    hk(RevisionGrid.Commands.SelectCurrentRevision, Keys.Control | Keys.Shift | Keys.C),
                    hk(RevisionGrid.Commands.SelectAsBaseToCompare, Keys.Control | Keys.L),
                    hk(RevisionGrid.Commands.CompareToBase, Keys.Control | Keys.R),
                    hk(RevisionGrid.Commands.GoToCommit, Keys.Control | Keys.Shift | Keys.G),
                    hk(RevisionGrid.Commands.CreateFixupCommit, Keys.Control | Keys.X)),
                new HotkeySettings(FileViewer.HotkeySettingsName,
                    hk(FileViewer.Commands.Find, Keys.Control | Keys.F),
                    hk(FileViewer.Commands.GoToLine, Keys.Control | Keys.G),
                    hk(FileViewer.Commands.IncreaseNumberOfVisibleLines, Keys.None),
                    hk(FileViewer.Commands.DecreaseNumberOfVisibleLines, Keys.None),
                    hk(FileViewer.Commands.ShowEntireFile, Keys.None),
                    hk(FileViewer.Commands.TreatFileAsText, Keys.None),
                    hk(FileViewer.Commands.NextChange, Keys.Alt | Keys.Down),
                    hk(FileViewer.Commands.PreviousChange, Keys.Alt | Keys.Up)),
                new HotkeySettings(FormResolveConflicts.HotkeySettingsName,
                    hk(FormResolveConflicts.Commands.ChooseBase, Keys.B),
                    hk(FormResolveConflicts.Commands.ChooseLocal, Keys.L),
                    hk(FormResolveConflicts.Commands.ChooseRemote, Keys.R),
                    hk(FormResolveConflicts.Commands.Merge, Keys.M),
                    hk(FormResolveConflicts.Commands.Rescan, Keys.F5)),
                new HotkeySettings(RevisionDiff.HotkeySettingsName,
                    hk(RevisionDiff.Commands.DeleteSelectedFiles, Keys.Delete)),
                new HotkeySettings(FormSettings.HotkeySettingsName,
                    scriptsHotkeys)
              };
        }

        public static HotkeyCommand[] LoadScriptHotkeys()
        {
            var curScripts = GitUI.Script.ScriptManager.GetScripts();

            HotkeyCommand[] scriptKeys = new HotkeyCommand[curScripts.Count];
            /* define unusable int for identifying a shortcut for a custom script is pressed
             * all integers above 9000 represent a scripthotkey
             * these integers are never matched in the 'switch' routine on a form and
             * therefore execute the 'default' action
             */

            return curScripts.
                Where(s => !s.Name.IsNullOrEmpty()).
                Select(s => new HotkeyCommand((int)s.HotkeyCommandIdentifier, s.Name) { KeyData = (Keys.None) }
            ).ToArray();
        }

    }
}
