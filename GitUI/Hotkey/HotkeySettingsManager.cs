using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using GitCommands;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using ResourceManager;

namespace GitUI.Hotkey
{
    internal static class HotkeySettingsManager
    {
        #region Serializer
        private static XmlSerializer _serializer;

        /// <summary>Lazy-loaded Serializer for HotkeySettings[]</summary>
        private static XmlSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = new XmlSerializer(typeof(HotkeySettings[]), new[] { typeof(HotkeyCommand) });
                }

                return _serializer;
            }
        }
        #endregion

        private static readonly List<Keys> UsedKeys = new List<Keys>();

        /// <summary>
        /// Returns whether the hotkey is already assigned.
        /// </summary>
        public static bool IsUniqueKey(Keys keyData)
        {
            return UsedKeys.Contains(keyData);
        }

        public static HotkeyCommand[] LoadHotkeys(string name)
        {
            ////var settings = LoadSettings().FirstOrDefault(s => s.Name == name);
            HotkeySettings[] allSettings;
            HotkeySettings settings = new HotkeySettings();
            HotkeySettings scriptkeys = new HotkeySettings();
            allSettings = LoadSettings();

            GetUsedHotkeys(allSettings);

            foreach (HotkeySettings hs in allSettings)
            {
                if (hs.Name == name)
                {
                    settings = hs;
                }

                if (hs.Name == "Scripts")
                {
                    scriptkeys = hs;
                }
            }

            ////HotkeyCommand[] scriptkeys = LoadSettings().FirstOrDefault(s => s.Name == name);

            if (settings != null)
            {
                // append general hotkeys to every form
                ////HotkeyCommand[] scriptkeys = LoadScriptHotkeys();
                HotkeyCommand[] allkeys = new HotkeyCommand[settings.Commands.Length + scriptkeys.Commands.Length];
                settings.Commands.CopyTo(allkeys, 0);
                scriptkeys.Commands.CopyTo(allkeys, settings.Commands.Length);

                return allkeys;
            }

            ////return settings != null ? settings.Commands : null;
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
                    {
                        UsedKeys.Add(hotkeyCommand.KeyData);
                    }
                }
            }
            ////MessageBox.Show(UsedKeys.Count.ToString());
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
            catch
            {
            }
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
                            if (defaultCommands.TryGetValue(dictKey, out var defaultCommand))
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
            foreach (HotkeySettings setting in settings)
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

        private static string CalcDictionaryKey(string settingName, int commandCode)
        {
            return settingName + ":" + commandCode;
        }

        private static HotkeySettings[] LoadSerializedSettings()
        {
            HotkeySettings[] settings = null;

            MigrateSettings();

            if (!string.IsNullOrWhiteSpace(AppSettings.SerializedHotkeys))
            {
                settings = LoadSerializedSettings(AppSettings.SerializedHotkeys);
            }

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
            catch
            {
            }

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
                        AppSettings.SerializedHotkeys = " "; // mark settings as migrated
                    }
                    else
                    {
                        SaveSettings(settings);
                    }
                }
                else
                {
                    AppSettings.SerializedHotkeys = " "; // mark settings as migrated
                }
            }
        }

        /// <summary>Asks the IHotkeyables to create their default hotkey settings</summary>
        public static HotkeySettings[] CreateDefaultSettings()
        {
            HotkeyCommand Hk(object en, Keys k) => new HotkeyCommand((int)en, en.ToString()) { KeyData = k };

            HotkeyCommand[] scriptsHotkeys = LoadScriptHotkeys();

            return new[]
              {
                // FormCommit
                new HotkeySettings(
                    FormCommit.HotkeySettingsName,
                    Hk(FormCommit.Commands.AddToGitIgnore, Keys.None),
                    Hk(FormCommit.Commands.DeleteSelectedFiles, Keys.Delete),
                    Hk(FormCommit.Commands.FocusUnstagedFiles, Keys.Control | Keys.D1),
                    Hk(FormCommit.Commands.FocusSelectedDiff, Keys.Control | Keys.D2),
                    Hk(FormCommit.Commands.FocusStagedFiles, Keys.Control | Keys.D3),
                    Hk(FormCommit.Commands.FocusCommitMessage, Keys.Control | Keys.D4),
                    Hk(FormCommit.Commands.ResetSelectedFiles, Keys.R),
                    Hk(FormCommit.Commands.StageSelectedFile, Keys.S),
                    Hk(FormCommit.Commands.UnStageSelectedFile, Keys.U),
                    Hk(FormCommit.Commands.ShowHistory, Keys.H),
                    Hk(FormCommit.Commands.ToggleSelectionFilter, Keys.Control | Keys.F),
                    Hk(FormCommit.Commands.StageAll, Keys.Control | Keys.S)),
                new HotkeySettings(
                    FormBrowse.HotkeySettingsName,
                    Hk(FormBrowse.Commands.GitBash, Keys.Control | Keys.G),
                    Hk(FormBrowse.Commands.GitGui, Keys.None),
                    Hk(FormBrowse.Commands.GitGitK, Keys.None),
                    Hk(FormBrowse.Commands.FocusRevisionGrid, Keys.Control | Keys.D1),
                    Hk(FormBrowse.Commands.FocusCommitInfo, Keys.Control | Keys.D2),
                    Hk(FormBrowse.Commands.FocusFileTree, Keys.Control | Keys.D3),
                    Hk(FormBrowse.Commands.FocusDiff, Keys.Control | Keys.D4),
                    Hk(FormBrowse.Commands.FocusFilter, Keys.Control | Keys.E),
                    Hk(FormBrowse.Commands.Commit, Keys.Control | Keys.Space),
                    Hk(FormBrowse.Commands.AddNotes, Keys.Control | Keys.Shift | Keys.N),
                    Hk(FormBrowse.Commands.FindFileInSelectedCommit, Keys.Control | Keys.Shift | Keys.F),
                    Hk(FormBrowse.Commands.CheckoutBranch, Keys.Control | Keys.Decimal),
                    Hk(FormBrowse.Commands.QuickFetch, Keys.Control | Keys.Shift | Keys.Down),
                    Hk(FormBrowse.Commands.QuickPull, Keys.Control | Keys.Shift | Keys.P),
                    Hk(FormBrowse.Commands.QuickPush, Keys.Control | Keys.Shift | Keys.Up),
                    Hk(FormBrowse.Commands.Stash, Keys.Control | Keys.Alt | Keys.Up),
                    Hk(FormBrowse.Commands.StashPop, Keys.Control | Keys.Alt | Keys.Down),
                    Hk(FormBrowse.Commands.CloseRepository, Keys.Control | Keys.W),
                    Hk(FormBrowse.Commands.RotateApplicationIcon, Keys.Control | Keys.Shift | Keys.I)),
                new HotkeySettings(
                    RevisionGrid.HotkeySettingsName,
                    Hk(RevisionGrid.Commands.RevisionFilter, Keys.Control | Keys.F),
                    Hk(RevisionGrid.Commands.ToggleRevisionGraph, Keys.None),
                    Hk(RevisionGrid.Commands.ToggleAuthorDateCommitDate, Keys.None),
                    Hk(RevisionGrid.Commands.ToggleOrderRevisionsByDate, Keys.None),
                    Hk(RevisionGrid.Commands.ToggleShowRelativeDate, Keys.None),
                    Hk(RevisionGrid.Commands.ToggleDrawNonRelativesGray, Keys.None),
                    Hk(RevisionGrid.Commands.ToggleShowGitNotes, Keys.None),
                    Hk(RevisionGrid.Commands.ToggleRevisionCardLayout, Keys.Control | Keys.Shift | Keys.L),
                    Hk(RevisionGrid.Commands.ToggleShowMergeCommits, Keys.Control | Keys.Shift | Keys.M),
                    Hk(RevisionGrid.Commands.ToggleShowTags, Keys.Control | Keys.Alt | Keys.T),
                    Hk(RevisionGrid.Commands.ShowAllBranches, Keys.Control | Keys.Shift | Keys.A),
                    Hk(RevisionGrid.Commands.ShowCurrentBranchOnly, Keys.Control | Keys.Shift | Keys.U),
                    Hk(RevisionGrid.Commands.ShowFilteredBranches, Keys.Control | Keys.Shift | Keys.T),
                    Hk(RevisionGrid.Commands.ShowRemoteBranches, Keys.Control | Keys.Shift | Keys.R),
                    Hk(RevisionGrid.Commands.ShowFirstParent, Keys.Control | Keys.Shift | Keys.S),
                    Hk(RevisionGrid.Commands.GoToParent, Keys.Control | Keys.P),
                    Hk(RevisionGrid.Commands.GoToChild, Keys.Control | Keys.N),
                    Hk(RevisionGrid.Commands.ToggleHighlightSelectedBranch, Keys.Control | Keys.Shift | Keys.B),
                    Hk(RevisionGrid.Commands.NextQuickSearch, Keys.Alt | Keys.Down),
                    Hk(RevisionGrid.Commands.PrevQuickSearch, Keys.Alt | Keys.Up),
                    Hk(RevisionGrid.Commands.SelectCurrentRevision, Keys.Control | Keys.Shift | Keys.C),
                    Hk(RevisionGrid.Commands.SelectAsBaseToCompare, Keys.Control | Keys.L),
                    Hk(RevisionGrid.Commands.CompareToBase, Keys.Control | Keys.R),
                    Hk(RevisionGrid.Commands.GoToCommit, Keys.Control | Keys.Shift | Keys.G),
                    Hk(RevisionGrid.Commands.CreateFixupCommit, Keys.Control | Keys.X),
                    Hk(RevisionGrid.Commands.ToggleBranchTreePanel, Keys.Control | Keys.Alt | Keys.C)),
                new HotkeySettings(
                    FileViewer.HotkeySettingsName,
                    Hk(FileViewer.Commands.Find, Keys.Control | Keys.F),
                    Hk(FileViewer.Commands.GoToLine, Keys.Control | Keys.G),
                    Hk(FileViewer.Commands.IncreaseNumberOfVisibleLines, Keys.None),
                    Hk(FileViewer.Commands.DecreaseNumberOfVisibleLines, Keys.None),
                    Hk(FileViewer.Commands.ShowEntireFile, Keys.None),
                    Hk(FileViewer.Commands.TreatFileAsText, Keys.None),
                    Hk(FileViewer.Commands.NextChange, Keys.Alt | Keys.Down),
                    Hk(FileViewer.Commands.PreviousChange, Keys.Alt | Keys.Up)),
                new HotkeySettings(
                    FormResolveConflicts.HotkeySettingsName,
                    Hk(FormResolveConflicts.Commands.ChooseBase, Keys.B),
                    Hk(FormResolveConflicts.Commands.ChooseLocal, Keys.L),
                    Hk(FormResolveConflicts.Commands.ChooseRemote, Keys.R),
                    Hk(FormResolveConflicts.Commands.Merge, Keys.M),
                    Hk(FormResolveConflicts.Commands.Rescan, Keys.F5)),
                new HotkeySettings(
                    RevisionDiff.HotkeySettingsName,
                    Hk(RevisionDiff.Commands.DeleteSelectedFiles, Keys.Delete)),
                new HotkeySettings(
                    FormSettings.HotkeySettingsName,
                    scriptsHotkeys)
              };
        }

        public static HotkeyCommand[] LoadScriptHotkeys()
        {
            var curScripts = Script.ScriptManager.GetScripts();

            /* define unusable int for identifying a shortcut for a custom script is pressed
             * all integers above 9000 represent a scripthotkey
             * these integers are never matched in the 'switch' routine on a form and
             * therefore execute the 'default' action
             */

            return curScripts.
                Where(s => !s.Name.IsNullOrEmpty()).
                Select(s => new HotkeyCommand(s.HotkeyCommandIdentifier, s.Name) { KeyData = Keys.None })
            .ToArray();
        }
    }
}
