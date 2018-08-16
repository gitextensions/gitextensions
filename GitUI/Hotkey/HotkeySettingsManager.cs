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
using GitUI.Script;
using JetBrains.Annotations;
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

        private static readonly HashSet<Keys> _usedKeys = new HashSet<Keys>();

        /// <summary>
        /// Returns whether the hotkey is already assigned.
        /// </summary>
        public static bool IsUniqueKey(Keys keyData)
        {
            return _usedKeys.Contains(keyData);
        }

        public static HotkeyCommand[] LoadHotkeys(string name)
        {
            var settings = new HotkeySettings();
            var scriptKeys = new HotkeySettings();
            var allSettings = LoadSettings();

            UpdateUsedKeys(allSettings);

            foreach (var setting in allSettings)
            {
                if (setting.Name == name)
                {
                    settings = setting;
                }

                if (setting.Name == "Scripts")
                {
                    scriptKeys = setting;
                }
            }

            // append general hotkeys to every form
            var allKeys = new HotkeyCommand[settings.Commands.Length + scriptKeys.Commands.Length];
            settings.Commands.CopyTo(allKeys, 0);
            scriptKeys.Commands.CopyTo(allKeys, settings.Commands.Length);

            return allKeys;
        }

        public static HotkeySettings[] LoadSettings()
        {
            // Get the default settings
            var defaultSettings = CreateDefaultSettings();
            var loadedSettings = LoadSerializedSettings();

            MergeIntoDefaultSettings(defaultSettings, loadedSettings);

            return defaultSettings;
        }

        private static void UpdateUsedKeys(HotkeySettings[] settings)
        {
            _usedKeys.Clear();

            foreach (var setting in settings)
            {
                foreach (var command in setting.Commands)
                {
                    if (command != null)
                    {
                        _usedKeys.Add(command.KeyData);
                    }
                }
            }
        }

        /// <summary>Serializes and saves the supplied settings</summary>
        public static void SaveSettings(HotkeySettings[] settings)
        {
            try
            {
                UpdateUsedKeys(settings);

                var str = new StringBuilder();
                using (var writer = new StringWriter(str))
                {
                    Serializer.Serialize(writer, settings);
                    AppSettings.SerializedHotkeys = str.ToString();
                }
            }
            catch
            {
                // ignore
            }
        }

        internal static void MergeIntoDefaultSettings(HotkeySettings[] defaultSettings, HotkeySettings[] loadedSettings)
        {
            if (loadedSettings == null)
            {
                return;
            }

            var defaultCommands = new Dictionary<string, HotkeyCommand>();

            FillDictionaryWithCommands();
            AssignHotkeysFromLoaded();

            void AssignHotkeysFromLoaded()
            {
                foreach (var setting in loadedSettings)
                {
                    if (setting != null)
                    {
                        foreach (var command in setting.Commands)
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

            void FillDictionaryWithCommands()
            {
                foreach (var setting in defaultSettings)
                {
                    foreach (var command in setting.Commands)
                    {
                        if (command != null)
                        {
                            string dictKey = CalcDictionaryKey(setting.Name, command.CommandCode);
                            defaultCommands.Add(dictKey, command);
                        }
                    }
                }
            }

            string CalcDictionaryKey(string settingName, int commandCode) => settingName + ":" + commandCode;
        }

        [CanBeNull]
        private static HotkeySettings[] LoadSerializedSettings()
        {
            MigrateSettings();

            if (!string.IsNullOrWhiteSpace(AppSettings.SerializedHotkeys))
            {
                return LoadSerializedSettings(AppSettings.SerializedHotkeys);
            }

            return null;
        }

        [CanBeNull]
        private static HotkeySettings[] LoadSerializedSettings(string serializedHotkeys)
        {
            try
            {
                using (var reader = new StringReader(serializedHotkeys))
                {
                    return (HotkeySettings[])Serializer.Deserialize(reader);
                }
            }
            catch
            {
                return null;
            }
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

        public static HotkeySettings[] CreateDefaultSettings()
        {
            HotkeyCommand Hk(object en, Keys k) => new HotkeyCommand((int)en, en.ToString()) { KeyData = k };

            const Keys OpenWithDifftoolHotkey = Keys.F3;
            const Keys ShowHistoryHotkey = Keys.H;
            const Keys BlameHotkey = Keys.B;

            return new[]
            {
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
                    Hk(FormCommit.Commands.ShowHistory, ShowHistoryHotkey),
                    Hk(FormCommit.Commands.ToggleSelectionFilter, Keys.Control | Keys.F),
                    Hk(FormCommit.Commands.StageAll, Keys.Control | Keys.S),
                    Hk(FormCommit.Commands.OpenWithDifftool, OpenWithDifftoolHotkey)),
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
                    Hk(FormBrowse.Commands.OpenSettings, Keys.Control | Keys.Oemcomma),
                    Hk(FormBrowse.Commands.OpenWithDifftool, OpenWithDifftoolHotkey),
                    Hk(FormBrowse.Commands.ToggleBranchTreePanel, Keys.Control | Keys.Alt | Keys.C)),
                new HotkeySettings(
                    RevisionGridControl.HotkeySettingsName,
                    Hk(RevisionGridControl.Commands.RevisionFilter, Keys.Control | Keys.F),
                    Hk(RevisionGridControl.Commands.ToggleRevisionGraph, Keys.None),
                    Hk(RevisionGridControl.Commands.ToggleAuthorDateCommitDate, Keys.None),
                    Hk(RevisionGridControl.Commands.ToggleOrderRevisionsByDate, Keys.None),
                    Hk(RevisionGridControl.Commands.ToggleShowRelativeDate, Keys.None),
                    Hk(RevisionGridControl.Commands.ToggleDrawNonRelativesGray, Keys.None),
                    Hk(RevisionGridControl.Commands.ToggleShowGitNotes, Keys.None),
                    Hk(RevisionGridControl.Commands.ToggleShowMergeCommits, Keys.Control | Keys.Shift | Keys.M),
                    Hk(RevisionGridControl.Commands.ToggleShowTags, Keys.Control | Keys.Alt | Keys.T),
                    Hk(RevisionGridControl.Commands.ShowAllBranches, Keys.Control | Keys.Shift | Keys.A),
                    Hk(RevisionGridControl.Commands.ShowCurrentBranchOnly, Keys.Control | Keys.Shift | Keys.U),
                    Hk(RevisionGridControl.Commands.ShowFilteredBranches, Keys.Control | Keys.Shift | Keys.T),
                    Hk(RevisionGridControl.Commands.ShowRemoteBranches, Keys.Control | Keys.Shift | Keys.R),
                    Hk(RevisionGridControl.Commands.ShowFirstParent, Keys.Control | Keys.Shift | Keys.S),
                    Hk(RevisionGridControl.Commands.GoToParent, Keys.Control | Keys.P),
                    Hk(RevisionGridControl.Commands.GoToChild, Keys.Control | Keys.N),
                    Hk(RevisionGridControl.Commands.ToggleHighlightSelectedBranch, Keys.Control | Keys.Shift | Keys.B),
                    Hk(RevisionGridControl.Commands.NextQuickSearch, Keys.Alt | Keys.Down),
                    Hk(RevisionGridControl.Commands.PrevQuickSearch, Keys.Alt | Keys.Up),
                    Hk(RevisionGridControl.Commands.SelectCurrentRevision, Keys.Control | Keys.Shift | Keys.C),
                    Hk(RevisionGridControl.Commands.SelectAsBaseToCompare, Keys.Control | Keys.L),
                    Hk(RevisionGridControl.Commands.CompareToBase, Keys.Control | Keys.R),
                    Hk(RevisionGridControl.Commands.GoToCommit, Keys.Control | Keys.Shift | Keys.G),
                    Hk(RevisionGridControl.Commands.CreateFixupCommit, Keys.Control | Keys.X),
                    Hk(RevisionGridControl.Commands.CompareToWorkingDirectory, Keys.Control | Keys.D),
                    Hk(RevisionGridControl.Commands.CompareToCurrentBranch, Keys.None),
                    Hk(RevisionGridControl.Commands.CompareToBranch, Keys.None),
                    Hk(RevisionGridControl.Commands.CompareSelectedCommits, Keys.None)),
                new HotkeySettings(
                    FileViewer.HotkeySettingsName,
                    Hk(FileViewer.Commands.Find, Keys.Control | Keys.F),
                    Hk(FileViewer.Commands.FindNextOrOpenWithDifftool, OpenWithDifftoolHotkey),
                    Hk(FileViewer.Commands.FindPrevious, Keys.Shift | OpenWithDifftoolHotkey),
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
                    RevisionDiffControl.HotkeySettingsName,
                    Hk(RevisionDiffControl.Command.DeleteSelectedFiles, Keys.Delete),
                    Hk(RevisionDiffControl.Command.ShowHistory, ShowHistoryHotkey),
                    Hk(RevisionDiffControl.Command.Blame, BlameHotkey),
                    Hk(RevisionDiffControl.Command.OpenWithDifftool, OpenWithDifftoolHotkey)),
                new HotkeySettings(
                    RevisionFileTreeControl.HotkeySettingsName,
                    Hk(RevisionFileTreeControl.Command.ShowHistory, ShowHistoryHotkey),
                    Hk(RevisionFileTreeControl.Command.Blame, BlameHotkey),
                    Hk(RevisionFileTreeControl.Command.OpenWithDifftool, OpenWithDifftoolHotkey)),
                new HotkeySettings(
                    FormSettings.HotkeySettingsName,
                    LoadScriptHotkeys())
            };

            HotkeyCommand[] LoadScriptHotkeys()
            {
                /* define unusable int for identifying a shortcut for a custom script is pressed
                 * all integers above 9000 represent a script hotkey
                 * these integers are never matched in the 'switch' routine on a form and
                 * therefore execute the 'default' action
                 */
                return ScriptManager
                    .GetScripts()
                    .Where(s => !s.Name.IsNullOrEmpty())
                    .Select(s => new HotkeyCommand(s.HotkeyCommandIdentifier, s.Name) { KeyData = Keys.None })
                    .ToArray();
            }
        }
    }
}
