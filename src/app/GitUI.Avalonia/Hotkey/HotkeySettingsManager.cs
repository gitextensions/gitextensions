using System.Xml.Serialization;
using GitCommands;
using GitUI.CommandsDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Hotkey;

// Reduced twin: loads the persisted settings for commands currently implemented by the
// Avalonia Browse form. More targets join the default set as their command handlers are ported.
internal sealed class HotkeySettingsManager : IHotkeySettingsLoader
{
    private static readonly XmlSerializer _serializer = new(typeof(HotkeySettings[]), [typeof(HotkeyCommand)]);

    public IReadOnlyList<HotkeyCommand> LoadHotkeys(string hotkeySettingsName)
    {
        HotkeySettings? defaults = CreateDefaultSettings()
            .FirstOrDefault(settings => settings.Name == hotkeySettingsName);
        if (defaults?.Commands is null)
        {
            return [];
        }

        HotkeyCommand[] commands = [.. defaults.Commands.Select(command => new HotkeyCommand(command.CommandCode, command.Name!) { KeyData = command.KeyData })];
        HotkeySettings? loaded = LoadSerializedSettings()
            ?.FirstOrDefault(settings => settings.Name == hotkeySettingsName);
        if (loaded?.Commands is not null)
        {
            Dictionary<int, HotkeyCommand> commandsByCode = commands.ToDictionary(command => command.CommandCode);
            foreach (HotkeyCommand loadedCommand in loaded.Commands)
            {
                if (commandsByCode.TryGetValue(loadedCommand.CommandCode, out HotkeyCommand? command))
                {
                    command.KeyData = loadedCommand.KeyData;
                }
            }
        }

        return commands;
    }

    internal static IReadOnlyList<HotkeySettings> CreateDefaultSettings()
    {
        HotkeyCommand Hk<TCommand>(TCommand command, WinFormsShims.Keys key)
            where TCommand : struct, Enum
            => new(Convert.ToInt32(command), command.ToString()) { KeyData = key };

        return
        [
            new HotkeySettings(
                FormBrowse.HotkeySettingsName,
                Hk(FormBrowse.Command.GitBash, WinFormsShims.Keys.Control | WinFormsShims.Keys.G),
                Hk(FormBrowse.Command.Refresh, WinFormsShims.Keys.F5),
                Hk(FormBrowse.Command.Commit, WinFormsShims.Keys.Control | WinFormsShims.Keys.Space),
                Hk(FormBrowse.Command.CheckoutBranch, WinFormsShims.Keys.Control | WinFormsShims.Keys.OemPeriod),
                Hk(FormBrowse.Command.PullOrFetch, WinFormsShims.Keys.Control | WinFormsShims.Keys.Down),
                Hk(FormBrowse.Command.Push, WinFormsShims.Keys.Control | WinFormsShims.Keys.Up),
                Hk(FormBrowse.Command.CreateBranch, WinFormsShims.Keys.Control | WinFormsShims.Keys.B),
                Hk(FormBrowse.Command.MergeBranches, WinFormsShims.Keys.Control | WinFormsShims.Keys.M),
                Hk(FormBrowse.Command.CreateTag, WinFormsShims.Keys.Control | WinFormsShims.Keys.T),
                Hk(FormBrowse.Command.Rebase, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.E)),
            new HotkeySettings(
                RevisionGridControl.HotkeySettingsName,
                Hk(RevisionGridControl.Command.CompareSelectedCommits, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.CompareToBase, WinFormsShims.Keys.Control | WinFormsShims.Keys.R),
                Hk(RevisionGridControl.Command.CompareToBranch, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.CompareToCurrentBranch, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.CompareToWorkingDirectory, WinFormsShims.Keys.Control | WinFormsShims.Keys.D),
                Hk(RevisionGridControl.Command.CreateAmendCommit, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.CreateFixupCommit, WinFormsShims.Keys.Control | WinFormsShims.Keys.X),
                Hk(RevisionGridControl.Command.CreateSquashCommit, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.X),
                Hk(RevisionGridControl.Command.DeleteRef, WinFormsShims.Keys.Delete),
                Hk(RevisionGridControl.Command.GoToChild, WinFormsShims.Keys.Control | WinFormsShims.Keys.N),
                Hk(RevisionGridControl.Command.GoToCommit, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.G),
                Hk(RevisionGridControl.Command.GoToFirstParent, WinFormsShims.Keys.Control | WinFormsShims.Keys.Left),
                Hk(RevisionGridControl.Command.GoToLastParent, WinFormsShims.Keys.Control | WinFormsShims.Keys.Right),
                Hk(RevisionGridControl.Command.GoToMergeBase, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.K),
                Hk(RevisionGridControl.Command.GoToParent, WinFormsShims.Keys.Control | WinFormsShims.Keys.P),
                Hk(RevisionGridControl.Command.NavigateBackward, WinFormsShims.Keys.Alt | WinFormsShims.Keys.Left),
                Hk(RevisionGridControl.Command.NavigateBackward_AlternativeHotkey, WinFormsShims.Keys.BrowserBack),
                Hk(RevisionGridControl.Command.NavigateForward, WinFormsShims.Keys.Alt | WinFormsShims.Keys.Right),
                Hk(RevisionGridControl.Command.NavigateForward_AlternativeHotkey, WinFormsShims.Keys.BrowserForward),
                Hk(RevisionGridControl.Command.NextQuickSearch, WinFormsShims.Keys.Alt | WinFormsShims.Keys.Down),
                Hk(RevisionGridControl.Command.OpenCommitsWithDifftool, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.PrevQuickSearch, WinFormsShims.Keys.Alt | WinFormsShims.Keys.Up),
                Hk(RevisionGridControl.Command.RenameRef, WinFormsShims.Keys.F2),
                Hk(RevisionGridControl.Command.ResetRevisionPathFilter, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.H),
                Hk(RevisionGridControl.Command.ResetRevisionFilter, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.I),
                Hk(RevisionGridControl.Command.RevisionFilter, WinFormsShims.Keys.Control | WinFormsShims.Keys.I),
                Hk(RevisionGridControl.Command.SelectAsBaseToCompare, WinFormsShims.Keys.Control | WinFormsShims.Keys.L),
                Hk(RevisionGridControl.Command.SelectCurrentRevision, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.C),
                Hk(RevisionGridControl.Command.SelectNextForkPointAsDiffBase, WinFormsShims.Keys.Control | WinFormsShims.Keys.K),
                Hk(RevisionGridControl.Command.ShowAllBranches, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.A),
                Hk(RevisionGridControl.Command.ShowCurrentBranchOnly, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.U),
                Hk(RevisionGridControl.Command.ShowFilteredBranches, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.T),
                Hk(RevisionGridControl.Command.ShowFirstParent, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.S),
                Hk(RevisionGridControl.Command.ShowReflogReferences, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.L),
                Hk(RevisionGridControl.Command.ShowRemoteBranches, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.R),
                Hk(RevisionGridControl.Command.ToggleAuthorDateCommitDate, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.ToggleBetweenArtificialAndHeadCommits, WinFormsShims.Keys.Control | WinFormsShims.Keys.OemBackslash),
                Hk(RevisionGridControl.Command.ToggleDrawNonRelativesGray, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.ToggleHighlightSelectedBranch, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.B),
                Hk(RevisionGridControl.Command.ToggleOrderRevisionsByDate, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.ToggleRevisionGraph, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.ToggleShowGitNotes, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.ToggleShowGitNotesColumn, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.ToggleHideMergeCommits, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.M),
                Hk(RevisionGridControl.Command.ToggleShowRelativeDate, WinFormsShims.Keys.None),
                Hk(RevisionGridControl.Command.ToggleShowTags, WinFormsShims.Keys.Control | WinFormsShims.Keys.Alt | WinFormsShims.Keys.T)),
            new HotkeySettings(
                FormResolveConflicts.HotkeySettingsName,
                Hk(FormResolveConflicts.Commands.ChooseBase, WinFormsShims.Keys.B),
                Hk(FormResolveConflicts.Commands.ChooseLocal, WinFormsShims.Keys.L),
                Hk(FormResolveConflicts.Commands.ChooseRemote, WinFormsShims.Keys.R),
                Hk(FormResolveConflicts.Commands.Merge, WinFormsShims.Keys.M),
                Hk(FormResolveConflicts.Commands.Rescan, WinFormsShims.Keys.F5)),
            new HotkeySettings(
                FormStash.HotkeySettingsName,
                Hk(FormStash.Command.NextStash, WinFormsShims.Keys.Control | WinFormsShims.Keys.N),
                Hk(FormStash.Command.PreviousStash, WinFormsShims.Keys.Control | WinFormsShims.Keys.P),
                Hk(FormStash.Command.Refresh, WinFormsShims.Keys.F5)),
        ];
    }

    private static HotkeySettings[]? LoadSerializedSettings()
    {
        if (string.IsNullOrWhiteSpace(AppSettings.SerializedHotkeys))
        {
            return null;
        }

        try
        {
            using StringReader reader = new(AppSettings.SerializedHotkeys);
            return (HotkeySettings[]?)_serializer.Deserialize(reader);
        }
        catch
        {
            return null;
        }
    }
}
