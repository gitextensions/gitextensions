using System.Xml;
using System.Xml.Serialization;
using GitCommands;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using GitUI.LeftPanel;
using GitUI.ScriptsEngine;
using Microsoft;
using ResourceManager;

namespace GitUI.Hotkey
{
    /// <summary>
    ///  Provides the ability to manage hotkeys settings.
    /// </summary>
    public interface IHotkeySettingsManager : IHotkeySettingsLoader
    {
        /// <summary>
        ///  Generates the list of preset hotkeys.
        /// </summary>
        /// <returns>The list of preset hotkeys.</returns>
        IReadOnlyList<HotkeySettings> CreateDefaultSettings();

        /// <summary>
        ///  Determines whether the hotkey is already assigned.
        /// </summary>
        /// <returns><see langword="true"/> if the hotkey is available; otherwise, <see langword="false"/>.</returns>
        bool IsUniqueKey(Keys keyData);

        /// <summary>
        ///  Loads all preset and user-configured hotkeys.
        /// </summary>
        /// <returns>The union of preset and user-configured hotkeys.</returns>
        IReadOnlyList<HotkeySettings> LoadSettings();

        /// <summary>
        ///  Saves the user-configured hotkeys.
        /// </summary>
        /// <param name="settings">The user-configured hotkeys.</param>
        void SaveSettings(IEnumerable<HotkeySettings> settings);
    }

    internal class HotkeySettingsManager : IHotkeySettingsManager
    {
        private static readonly XmlSerializer? _serializer = new(typeof(HotkeySettings[]), new[] { typeof(HotkeyCommand) });
        private readonly HashSet<Keys> _usedKeys = [];
        private readonly IScriptsManager _scriptsManager;

        public HotkeySettingsManager(IScriptsManager scriptsManager)
        {
            _scriptsManager = scriptsManager;
        }

        public bool IsUniqueKey(Keys keyData) => _usedKeys.Contains(keyData);

        public IReadOnlyList<HotkeySettings> LoadSettings()
        {
            // Get the default settings
            IReadOnlyList<HotkeySettings> defaultSettings = CreateDefaultSettings();
            HotkeySettings[] loadedSettings = LoadSerializedSettings();

            MergeIntoDefaultSettings(defaultSettings, loadedSettings);

            return defaultSettings;
        }

        private void UpdateUsedKeys(IEnumerable<HotkeySettings> settings)
        {
            _usedKeys.Clear();

            foreach (HotkeySettings setting in settings)
            {
                if (setting.Commands is not null)
                {
                    foreach (HotkeyCommand command in setting.Commands)
                    {
                        _usedKeys.Add(command.KeyData);
                    }
                }
            }
        }

        /// <summary>Serializes and saves the supplied settings.</summary>
        public void SaveSettings(IEnumerable<HotkeySettings> settings)
        {
            try
            {
                UpdateUsedKeys(settings);

                XmlWriterSettings xmlWriterSettings = new()
                {
                    Indent = true
                };
                using StringWriter sw = new();
                using XmlWriter xmlWriter = XmlWriter.Create(sw, xmlWriterSettings);

                _serializer.Serialize(xmlWriter, settings.ToArray());
                AppSettings.SerializedHotkeys = sw.ToString();
            }
            catch
            {
                // ignore
            }
        }

        internal static void MergeIntoDefaultSettings(IEnumerable<HotkeySettings> defaultSettings, IEnumerable<HotkeySettings>? loadedSettings)
        {
            if (loadedSettings is null)
            {
                return;
            }

            Dictionary<string, HotkeyCommand> defaultCommands = [];

            FillDictionaryWithCommands();
            AssignHotkeysFromLoaded();

            void AssignHotkeysFromLoaded()
            {
                foreach (HotkeySettings setting in loadedSettings)
                {
                    if (setting.Commands is not null && setting.Name is not null)
                    {
                        foreach (HotkeyCommand command in setting.Commands)
                        {
                            string dictKey = CalcDictionaryKey(setting.Name, command.CommandCode);
                            if (defaultCommands.TryGetValue(dictKey, out HotkeyCommand defaultCommand))
                            {
                                defaultCommand.KeyData = command.KeyData;
                            }
                        }
                    }
                }
            }

            void FillDictionaryWithCommands()
            {
                foreach (HotkeySettings setting in defaultSettings)
                {
                    if (setting.Commands is not null && setting.Name is not null)
                    {
                        foreach (HotkeyCommand command in setting.Commands)
                        {
                            string dictKey = CalcDictionaryKey(setting.Name, command.CommandCode);
                            defaultCommands.Add(dictKey, command);
                        }
                    }
                }
            }

            string CalcDictionaryKey(string settingName, int commandCode) => settingName + ":" + commandCode;
        }

        private static HotkeySettings[]? LoadSerializedSettings()
        {
            if (!string.IsNullOrWhiteSpace(AppSettings.SerializedHotkeys))
            {
                return LoadSerializedSettings(AppSettings.SerializedHotkeys);
            }

            return null;
        }

        private static HotkeySettings[]? LoadSerializedSettings(string serializedHotkeys)
        {
            try
            {
                using StringReader reader = new(serializedHotkeys);
                return (HotkeySettings[])_serializer.Deserialize(reader);
            }
            catch
            {
                return null;
            }
        }

        public IReadOnlyList<HotkeySettings> CreateDefaultSettings()
        {
            HotkeyCommand Hk(object en, Keys k) => new((int)en, en.ToString()) { KeyData = k };

            const Keys OpenWithDifftoolHotkey = Keys.F3;
            const Keys OpenWithDifftoolFirstToLocalHotkey = Keys.Alt | Keys.F3;
            const Keys OpenWithDifftoolSelectedToLocalHotkey = Keys.Shift | Keys.Alt | Keys.F3;
            const Keys OpenAsTempFileHotkey = Keys.Control | Keys.F3;
            const Keys OpenAsTempFileWithHotkey = Keys.Shift | Keys.Control | Keys.F3;
            const Keys EditFileHotkey = Keys.F4;
            const Keys OpenFileHotkey = Keys.Shift | Keys.F4;
            const Keys OpenFileWithHotkey = Keys.Shift | Keys.Control | Keys.F4;
            const Keys ShowHistoryHotkey = Keys.H;
            const Keys BlameHotkey = Keys.B;

            return new[]
            {
                new HotkeySettings(
                    FormCommit.HotkeySettingsName,
                    Hk(FormCommit.Command.AddSelectionToCommitMessage, Keys.C),
                    Hk(FormCommit.Command.AddToGitIgnore, Keys.None),
                    Hk(FormCommit.Command.ConventionalCommit_PrefixMessage, Keys.Control | Keys.T),
                    Hk(FormCommit.Command.ConventionalCommit_PrefixMessageWithScope, Keys.Control | Keys.Shift | Keys.T),
                    Hk(FormCommit.Command.CreateBranch, Keys.Control | Keys.B),
                    Hk(FormCommit.Command.DeleteSelectedFiles, Keys.Delete),
                    Hk(FormCommit.Command.EditFile, EditFileHotkey),
                    Hk(FormCommit.Command.FocusUnstagedFiles, Keys.Control | Keys.D1),
                    Hk(FormCommit.Command.FocusSelectedDiff, Keys.Control | Keys.D2),
                    Hk(FormCommit.Command.FocusStagedFiles, Keys.Control | Keys.D3),
                    Hk(FormCommit.Command.FocusCommitMessage, Keys.Control | Keys.D4),
                    Hk(FormCommit.Command.OpenFile, OpenFileHotkey),
                    Hk(FormCommit.Command.OpenFileWith, OpenFileWithHotkey),
                    Hk(FormCommit.Command.OpenWithDifftool, OpenWithDifftoolHotkey),
                    Hk(FormCommit.Command.Refresh, Keys.F5),
                    Hk(FormCommit.Command.ResetSelectedFiles, Keys.R),
                    Hk(FormCommit.Command.SelectNext, Keys.Control | Keys.N),
                    Hk(FormCommit.Command.SelectNext_AlternativeHotkey1, Keys.Alt | Keys.Down),
                    Hk(FormCommit.Command.SelectNext_AlternativeHotkey2, Keys.Alt | Keys.Right),
                    Hk(FormCommit.Command.SelectPrevious, Keys.Control | Keys.P),
                    Hk(FormCommit.Command.SelectPrevious_AlternativeHotkey1, Keys.Alt | Keys.Up),
                    Hk(FormCommit.Command.SelectPrevious_AlternativeHotkey2, Keys.Alt | Keys.Left),
                    Hk(FormCommit.Command.StageSelectedFile, Keys.S),
                    Hk(FormCommit.Command.UnStageSelectedFile, Keys.U),
                    Hk(FormCommit.Command.ShowHistory, ShowHistoryHotkey),
                    Hk(FormCommit.Command.StageAll, Keys.Control | Keys.S),
                    Hk(FormCommit.Command.ToggleSelectionFilter, Keys.Control | Keys.F)),
                new HotkeySettings(
                    FormBrowse.HotkeySettingsName,
                    Hk(FormBrowse.Command.AddNotes, Keys.Control | Keys.Shift | Keys.N),
                    Hk(FormBrowse.Command.CheckoutBranch, Keys.Control | Keys.OemPeriod),
                    Hk(FormBrowse.Command.CloseRepository, Keys.Control | Keys.W),
                    Hk(FormBrowse.Command.Commit, Keys.Control | Keys.Space),
                    Hk(FormBrowse.Command.CreateBranch, Keys.Control | Keys.B),
                    Hk(FormBrowse.Command.CreateTag, Keys.Control | Keys.T),
                    Hk(FormBrowse.Command.EditFile, EditFileHotkey),
                    Hk(FormBrowse.Command.FindFileInSelectedCommit, Keys.Control | Keys.Shift | Keys.F),
                    Hk(FormBrowse.Command.FocusLeftPanel, Keys.Control | Keys.D0),
                    Hk(FormBrowse.Command.FocusRevisionGrid, Keys.Control | Keys.D1),
                    Hk(FormBrowse.Command.FocusCommitInfo, Keys.Control | Keys.D2),
                    Hk(FormBrowse.Command.FocusDiff, Keys.Control | Keys.D3),
                    Hk(FormBrowse.Command.FocusFileTree, Keys.Control | Keys.D4),
                    Hk(FormBrowse.Command.FocusGpgInfo, Keys.Control | Keys.D5),
                    Hk(FormBrowse.Command.FocusGitConsole, Keys.Control | Keys.D6),
                    Hk(FormBrowse.Command.FocusBuildServerStatus, Keys.Control | Keys.D7),
                    Hk(FormBrowse.Command.FocusNextTab, Keys.Control | Keys.Tab),
                    Hk(FormBrowse.Command.FocusPrevTab, Keys.Control | Keys.Shift | Keys.Tab),
                    Hk(FormBrowse.Command.FocusFilter, Keys.Control | Keys.E),
                    Hk(FormBrowse.Command.GitBash, Keys.Control | Keys.G),
                    Hk(FormBrowse.Command.GitGui, Keys.None),
                    Hk(FormBrowse.Command.GitGitK, Keys.None),
                    Hk(FormBrowse.Command.GoToChild, Keys.Control | Keys.N),
                    Hk(FormBrowse.Command.GoToParent, Keys.Control | Keys.P),
                    Hk(FormBrowse.Command.GoToSubmodule, Keys.None),
                    Hk(FormBrowse.Command.GoToSuperproject, Keys.None),
                    Hk(FormBrowse.Command.MergeBranches, Keys.Control | Keys.M),
                    Hk(FormBrowse.Command.OpenAsTempFile, OpenAsTempFileHotkey),
                    Hk(FormBrowse.Command.OpenAsTempFileWith, OpenAsTempFileWithHotkey),
                    Hk(FormBrowse.Command.OpenCommitsWithDifftool, Keys.None),
                    Hk(FormBrowse.Command.OpenRepo, Keys.Control | Keys.O),
                    Hk(FormBrowse.Command.OpenSettings, Keys.Control | Keys.Oemcomma),
                    Hk(FormBrowse.Command.OpenWithDifftool, OpenWithDifftoolHotkey),
                    Hk(FormBrowse.Command.OpenWithDifftoolFirstToLocal, OpenWithDifftoolFirstToLocalHotkey),
                    Hk(FormBrowse.Command.OpenWithDifftoolSelectedToLocal, OpenWithDifftoolSelectedToLocalHotkey),
                    Hk(FormBrowse.Command.PullOrFetch, Keys.Control | Keys.Down),
                    Hk(FormBrowse.Command.Push, Keys.Control | Keys.Up),
                    Hk(FormBrowse.Command.QuickFetch, Keys.Control | Keys.Shift | Keys.Down),
                    Hk(FormBrowse.Command.QuickPull, Keys.Control | Keys.Shift | Keys.P),
                    Hk(FormBrowse.Command.QuickPullOrFetch, Keys.F8),
                    Hk(FormBrowse.Command.QuickPush, Keys.Control | Keys.Shift | Keys.Up),
                    Hk(FormBrowse.Command.Rebase, Keys.Control | Keys.Shift | Keys.E),
                    Hk(FormBrowse.Command.Stash, Keys.Control | Keys.Alt | Keys.Up),
                    Hk(FormBrowse.Command.StashPop, Keys.Control | Keys.Alt | Keys.Down),
                    Hk(FormBrowse.Command.StashStaged, Keys.Control | Keys.Shift | Keys.Alt | Keys.Up),
                    Hk(FormBrowse.Command.ToggleBetweenArtificialAndHeadCommits, Keys.Control | Keys.OemBackslash),
                    Hk(FormBrowse.Command.ToggleLeftPanel, Keys.Control | Keys.Alt | Keys.C)),
                new HotkeySettings(
                    RepoObjectsTree.HotkeySettingsName,
                    Hk(RepoObjectsTree.Command.Delete, Keys.Delete),
                    Hk(RepoObjectsTree.Command.MultiSelect, Keys.Control | Keys.Space),
                    Hk(RepoObjectsTree.Command.MultiSelectWithChildren, Keys.Control | Keys.Shift | Keys.Space),
                    Hk(RepoObjectsTree.Command.Rename, Keys.F2),
                    Hk(RepoObjectsTree.Command.Search, Keys.F3)),
                new HotkeySettings(
                    RevisionGridControl.HotkeySettingsName,
                    Hk(RevisionGridControl.Command.CompareSelectedCommits, Keys.None),
                    Hk(RevisionGridControl.Command.CompareToBase, Keys.Control | Keys.R),
                    Hk(RevisionGridControl.Command.CompareToBranch, Keys.None),
                    Hk(RevisionGridControl.Command.CompareToCurrentBranch, Keys.None),
                    Hk(RevisionGridControl.Command.CompareToWorkingDirectory, Keys.Control | Keys.D),
                    Hk(RevisionGridControl.Command.CreateAmendCommit, Keys.None),
                    Hk(RevisionGridControl.Command.CreateFixupCommit, Keys.Control | Keys.X),
                    Hk(RevisionGridControl.Command.CreateSquashCommit, Keys.Control | Keys.Shift | Keys.X),
                    Hk(RevisionGridControl.Command.DeleteRef, Keys.Delete),
                    Hk(RevisionGridControl.Command.GoToChild, Keys.Control | Keys.N),
                    Hk(RevisionGridControl.Command.GoToCommit, Keys.Control | Keys.Shift | Keys.G),
                    Hk(RevisionGridControl.Command.GoToMergeBase, Keys.Control | Keys.Shift | Keys.K),
                    Hk(RevisionGridControl.Command.GoToParent, Keys.Control | Keys.P),
                    Hk(RevisionGridControl.Command.NavigateBackward, Keys.Alt | Keys.Left),
                    Hk(RevisionGridControl.Command.NavigateBackward_AlternativeHotkey, Keys.BrowserBack),
                    Hk(RevisionGridControl.Command.NavigateForward, Keys.Alt | Keys.Right),
                    Hk(RevisionGridControl.Command.NavigateForward_AlternativeHotkey, Keys.BrowserForward),
                    Hk(RevisionGridControl.Command.NextQuickSearch, Keys.Alt | Keys.Down),
                    Hk(RevisionGridControl.Command.OpenCommitsWithDifftool, Keys.None),
                    Hk(RevisionGridControl.Command.PrevQuickSearch, Keys.Alt | Keys.Up),
                    Hk(RevisionGridControl.Command.RenameRef, Keys.F2),
                    Hk(RevisionGridControl.Command.ResetRevisionPathFilter, Keys.Control | Keys.Shift | Keys.H),
                    Hk(RevisionGridControl.Command.ResetRevisionFilter, Keys.Control | Keys.Shift | Keys.I),
                    Hk(RevisionGridControl.Command.RevisionFilter, Keys.Control | Keys.I),
                    Hk(RevisionGridControl.Command.SelectAsBaseToCompare, Keys.Control | Keys.L),
                    Hk(RevisionGridControl.Command.SelectCurrentRevision, Keys.Control | Keys.Shift | Keys.C),
                    Hk(RevisionGridControl.Command.SelectNextForkPointAsDiffBase, Keys.Control | Keys.K),
                    Hk(RevisionGridControl.Command.ShowAllBranches, Keys.Control | Keys.Shift | Keys.A),
                    Hk(RevisionGridControl.Command.ShowCurrentBranchOnly, Keys.Control | Keys.Shift | Keys.U),
                    Hk(RevisionGridControl.Command.ShowFilteredBranches, Keys.Control | Keys.Shift | Keys.T),
                    Hk(RevisionGridControl.Command.ShowFirstParent, Keys.Control | Keys.Shift | Keys.S),
                    Hk(RevisionGridControl.Command.ShowReflogReferences, Keys.Control | Keys.Shift | Keys.L),
                    Hk(RevisionGridControl.Command.ShowRemoteBranches, Keys.Control | Keys.Shift | Keys.R),
                    Hk(RevisionGridControl.Command.ToggleAuthorDateCommitDate, Keys.None),
                    Hk(RevisionGridControl.Command.ToggleBetweenArtificialAndHeadCommits, Keys.Control | Keys.OemBackslash),
                    Hk(RevisionGridControl.Command.ToggleDrawNonRelativesGray, Keys.None),
                    Hk(RevisionGridControl.Command.ToggleHighlightSelectedBranch, Keys.Control | Keys.Shift | Keys.B),
                    Hk(RevisionGridControl.Command.ToggleOrderRevisionsByDate, Keys.None),
                    Hk(RevisionGridControl.Command.ToggleRevisionGraph, Keys.None),
                    Hk(RevisionGridControl.Command.ToggleShowGitNotes, Keys.None),
                    Hk(RevisionGridControl.Command.ToggleHideMergeCommits, Keys.Control | Keys.Shift | Keys.M),
                    Hk(RevisionGridControl.Command.ToggleShowRelativeDate, Keys.None),
                    Hk(RevisionGridControl.Command.ToggleShowTags, Keys.Control | Keys.Alt | Keys.T)),
                new HotkeySettings(
                    FileViewer.HotkeySettingsName,
                    Hk(FileViewer.Command.Find, Keys.Control | Keys.F),
                    Hk(FileViewer.Command.Replace, Keys.Control | Keys.H),
                    Hk(FileViewer.Command.FindNextOrOpenWithDifftool, OpenWithDifftoolHotkey),
                    Hk(FileViewer.Command.FindPrevious, Keys.Shift | OpenWithDifftoolHotkey),
                    Hk(FileViewer.Command.GoToLine, Keys.Control | Keys.G),
                    Hk(FileViewer.Command.IncreaseNumberOfVisibleLines, Keys.Control | Keys.Oemplus),
                    Hk(FileViewer.Command.DecreaseNumberOfVisibleLines, Keys.Control | Keys.OemMinus),
                    Hk(FileViewer.Command.NextChange, Keys.Alt | Keys.Down),
                    Hk(FileViewer.Command.PreviousChange, Keys.Alt | Keys.Up),
                    Hk(FileViewer.Command.ShowEntireFile, Keys.Control | Keys.E),
                    Hk(FileViewer.Command.ShowSyntaxHighlighting, Keys.X),
                    Hk(FileViewer.Command.ShowGitWordColoring, Keys.Control | Keys.D),
                    Hk(FileViewer.Command.ShowDifftastic, Keys.Control | Keys.T),
                    Hk(FileViewer.Command.TreatFileAsText, Keys.None),
                    Hk(FileViewer.Command.NextOccurrence, Keys.Alt | Keys.Right),
                    Hk(FileViewer.Command.PreviousOccurrence, Keys.Alt | Keys.Left),
                    Hk(FileViewer.Command.StageLines, Keys.S),
                    Hk(FileViewer.Command.UnstageLines, Keys.U),
                    Hk(FileViewer.Command.ResetLines, Keys.R),
                    Hk(FileViewer.Command.IgnoreAllWhitespace, Keys.Control | Keys.Shift | Keys.W)),
                new HotkeySettings(
                    FormResolveConflicts.HotkeySettingsName,
                    Hk(FormResolveConflicts.Commands.ChooseBase, Keys.B),
                    Hk(FormResolveConflicts.Commands.ChooseLocal, Keys.L),
                    Hk(FormResolveConflicts.Commands.ChooseRemote, Keys.R),
                    Hk(FormResolveConflicts.Commands.Merge, Keys.M),
                    Hk(FormResolveConflicts.Commands.Rescan, Keys.F5)),
                new HotkeySettings(
                    RevisionDiffControl.HotkeySettingsName,
                    Hk(RevisionDiffControl.Command.Blame, BlameHotkey),
                    Hk(RevisionDiffControl.Command.DeleteSelectedFiles, Keys.Delete),
                    Hk(RevisionDiffControl.Command.EditFile, EditFileHotkey),
                    Hk(RevisionDiffControl.Command.FilterFileInGrid, Keys.F),
                    Hk(RevisionDiffControl.Command.FindFile, Keys.None),
                    Hk(RevisionDiffControl.Command.OpenAsTempFile, OpenAsTempFileHotkey),
                    Hk(RevisionDiffControl.Command.OpenAsTempFileWith, OpenAsTempFileWithHotkey),
                    Hk(RevisionDiffControl.Command.OpenWithDifftool, OpenWithDifftoolHotkey),
                    Hk(RevisionDiffControl.Command.OpenWithDifftoolFirstToLocal, OpenWithDifftoolFirstToLocalHotkey),
                    Hk(RevisionDiffControl.Command.OpenWithDifftoolSelectedToLocal, OpenWithDifftoolSelectedToLocalHotkey),
                    Hk(RevisionDiffControl.Command.OpenWorkingDirectoryFileWith, Keys.Control | Keys.O),
                    Hk(RevisionDiffControl.Command.ResetSelectedFiles, Keys.R),
                    Hk(RevisionDiffControl.Command.SearchCommit, Keys.Control | Keys.Shift | Keys.F),
                    Hk(RevisionDiffControl.Command.SelectFirstGroupChanges, Keys.Control | Keys.A),
                    Hk(RevisionDiffControl.Command.ShowFileTree, Keys.T),
                    Hk(RevisionDiffControl.Command.ShowHistory, ShowHistoryHotkey),
                    Hk(RevisionDiffControl.Command.StageSelectedFile, Keys.S),
                    Hk(RevisionDiffControl.Command.UnStageSelectedFile, Keys.U)),
                new HotkeySettings(
                    RevisionFileTreeControl.HotkeySettingsName,
                    Hk(RevisionFileTreeControl.Command.Blame, BlameHotkey),
                    Hk(RevisionFileTreeControl.Command.EditFile, EditFileHotkey),
                    Hk(RevisionFileTreeControl.Command.FilterFileInGrid, Keys.F),
                    Hk(RevisionFileTreeControl.Command.FindFile, Keys.Control | Keys.F),
                    Hk(RevisionFileTreeControl.Command.OpenAsTempFile, OpenAsTempFileHotkey),
                    Hk(RevisionFileTreeControl.Command.OpenAsTempFileWith, OpenAsTempFileWithHotkey),
                    Hk(RevisionFileTreeControl.Command.OpenWithDifftool, OpenWithDifftoolHotkey),
                    Hk(RevisionFileTreeControl.Command.OpenWorkingDirectoryFileWith, Keys.Control | Keys.O),
                    Hk(RevisionFileTreeControl.Command.ShowHistory, ShowHistoryHotkey)),
                new HotkeySettings(
                    FormStash.HotkeySettingsName,
                    Hk(FormStash.Command.NextStash, Keys.Control | Keys.N),
                    Hk(FormStash.Command.PreviousStash, Keys.Control | Keys.P),
                    Hk(FormStash.Command.Refresh, Keys.F5)),
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
                return _scriptsManager
                    .GetScripts()
                    .Where(s => !string.IsNullOrEmpty(s.Name))
                    .Select(s => new HotkeyCommand(s.HotkeyCommandIdentifier, s.Name!) { KeyData = Keys.None })
                    .ToArray();
            }
        }

        IReadOnlyList<HotkeyCommand> IHotkeySettingsLoader.LoadHotkeys(string hotkeySettingsName)
        {
            HotkeySettings settings = new();
            HotkeySettings scriptKeys = new();
            IEnumerable<HotkeySettings> allSettings = LoadSettings();

            UpdateUsedKeys(allSettings);

            foreach (HotkeySettings setting in allSettings)
            {
                if (setting.Name == hotkeySettingsName)
                {
                    settings = setting;
                }

                if (setting.Name == "Scripts")
                {
                    scriptKeys = setting;
                }
            }

            // append general hotkeys to every form
            Validates.NotNull(settings.Commands);
            Validates.NotNull(scriptKeys.Commands);
            HotkeyCommand[] allKeys = new HotkeyCommand[settings.Commands.Length + scriptKeys.Commands.Length];
            settings.Commands.CopyTo(allKeys, 0);
            scriptKeys.Commands.CopyTo(allKeys, settings.Commands.Length);

            return allKeys;
        }
    }
}
