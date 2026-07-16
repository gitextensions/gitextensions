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
                Hk(FormBrowse.Command.Rebase, WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.E)),
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
