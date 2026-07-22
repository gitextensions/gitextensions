using System.Diagnostics;
using System.Text.RegularExpressions;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitUI.Properties;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

// Avalonia twin of GitUI/Plugin/FailedPluginWrapper.cs. Only icon materialization differs:
// the public plugin contract still carries the headless image wrapper on net10.0.
internal partial class FailedPluginWrapper : IGitPlugin
{
    [GeneratedRegex(@"""GitExtensions.([^""]+)""", RegexOptions.ExplicitCapture)]
    private static partial Regex PluginNameRegex { get; }

    private readonly string _exception;
    private string _pluginName = null!;

    public FailedPluginWrapper(Exception loadingException)
    {
        ArgumentNullException.ThrowIfNull(loadingException);

        _exception = loadingException.Demystify().ToString();
        try
        {
            Match match = PluginNameRegex.Match(_exception);
            if (match.Success)
            {
                _pluginName = match.Value;
                Name = $"{TranslatedStrings.FailedToLoadPlugin}: {_pluginName[..Math.Min(50, match.ValueSpan.Length)]}";
            }
        }
        catch (Exception)
        {
            // no-op: best effort
        }
    }

    public Guid Id { get; } = Guid.NewGuid();
    public string? Name { get; init; } = TranslatedStrings.FailedToLoadPlugin;
    public string? Description { get; } = TranslatedStrings.FailedToLoadPlugin;
    public WinFormsShims.Image? Icon { get; } = new() { PlatformImage = Images.Bug };
    public IGitPluginSettingsContainer? SettingsContainer { get; set; }
    public bool HasSettings { get; } = false;

    public bool Execute(GitUIEventArgs args)
    {
        DialogResult result = MessageBoxes.Show(
            string.Format(TranslatedStrings.FailedToLoadPluginPopupText, _exception),
            TranslatedStrings.FailedToLoadPlugin,
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Error);
        if (result == DialogResult.OK)
        {
            WinFormsShims.Clipboard.SetText(_exception);
        }

        return false;
    }

    public IEnumerable<ISetting> GetSettings() => [];

    public void Register(IGitUICommands gitUiCommands)
    {
    }

    public void Unregister(IGitUICommands gitUiCommands)
    {
    }
}
