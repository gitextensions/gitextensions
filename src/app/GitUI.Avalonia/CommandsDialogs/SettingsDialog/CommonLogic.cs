using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog;

/// <summary>
/// Settings stores shared by the Avalonia pages. UI-specific pickers are added with the pages
/// that consume them; the storage boundary remains identical to WinForms.
/// </summary>
public sealed class CommonLogic : Translate
{
    internal const string PresetGitEditorEnvVariableName = "GIT_EDITOR";
    internal const string AmbientGitEditorEnvVariableName = "EDITOR";

    public readonly DistributedSettingsSet DistributedSettingsSet;
    public readonly GitConfigSettingsSet GitConfigSettingsSet;
    public readonly IGitModule Module;

    private CommonLogic()
    {
        Module = null!;
    }

    public CommonLogic(IGitModule module)
    {
        Validates.NotNull(module);
        Module = module;

        DistributedSettings distributedGlobalSettings = DistributedSettings.CreateGlobal(useSharedCache: false);
        DistributedSettings distributedPulledSettings = DistributedSettings.CreateDistributed(module, useSharedCache: false);
        DistributedSettings distributedLocalSettings = DistributedSettings.CreateLocal(module, useSharedCache: false);
        DistributedSettings distributedEffectiveSettings = new(
            new DistributedSettings(distributedGlobalSettings, distributedPulledSettings.SettingsCache, SettingLevel.Distributed),
            distributedLocalSettings.SettingsCache,
            SettingLevel.Effective);
        DistributedSettingsSet = new DistributedSettingsSet(
            distributedEffectiveSettings,
            distributedLocalSettings,
            distributedPulledSettings,
            distributedGlobalSettings);

        IExecutable gitExecutable = module.GitExecutable;
        GitConfigSettings systemGitConfigSettings = new(gitExecutable, GitSettingLevel.SystemWide);
        GitConfigSettings globalGitConfigSettings = new(gitExecutable, GitSettingLevel.Global);
        GitConfigSettings localGitConfigSettings = new(gitExecutable, GitSettingLevel.Local);
        EffectiveGitConfigSettings effectiveGitConfigSettings = new(gitExecutable);
        GitConfigSettingsSet = new GitConfigSettingsSet(
            new SettingsSource<IConfigValueStore>(effectiveGitConfigSettings),
            new SettingsSource<IPersistentConfigValueStore>(localGitConfigSettings),
            new SettingsSource<IPersistentConfigValueStore>(globalGitConfigSettings),
            new SettingsSource<IConfigValueStore>(systemGitConfigSettings));
    }

    public string? GetGlobalEditor()
    {
        return GetEditorOptions().FirstOrDefault(option => !string.IsNullOrEmpty(option));

        IEnumerable<string?> GetEditorOptions()
        {
            yield return Environment.GetEnvironmentVariable(PresetGitEditorEnvVariableName);
            yield return GitConfigSettingsSet.GlobalSettings.GetValue("core.editor");
            yield return Environment.GetEnvironmentVariable("VISUAL");
            yield return Environment.GetEnvironmentVariable(AmbientGitEditorEnvVariableName);
        }
    }

    public static void FillEncodings(ComboBox combo)
    {
        combo.ItemsSource = AppSettings.AvailableEncodings.Values.ToArray();
        combo.ItemTemplate = new FuncDataTemplate<Encoding>(
            (encoding, _) => new TextBlock { Text = encoding?.EncodingName },
            supportsRecycling: true);
    }
}
