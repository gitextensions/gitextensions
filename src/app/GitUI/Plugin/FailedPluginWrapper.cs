using System.Diagnostics;
using System.Text.RegularExpressions;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitUI.Properties;

namespace GitUI;

internal partial class FailedPluginWrapper : IGitPlugin
{
    [GeneratedRegex(@"""GitExtensions.([^""]+)""", RegexOptions.ExplicitCapture)]
    private static partial Regex PluginNameRegex();

    private readonly string _exception;
    private string _pluginName;

    public FailedPluginWrapper(Exception loadingException)
    {
        ArgumentNullException.ThrowIfNull(loadingException);

        _exception = loadingException.Demystify().ToString();
        try
        {
            // Try to extract plugin name from exception
            Match match = PluginNameRegex().Match(_exception);
            if (match.Success)
            {
                _pluginName = match.Value;
                Name = $"{TranslatedStrings.FailedToLoadPlugin}: {_pluginName.Substring(0, Math.Min(50, match.Value.Length))}";
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
    public Image? Icon { get; } = Resources.bug;
    public IGitPluginSettingsContainer? SettingsContainer { get; set; }
    public bool HasSettings { get; } = false;

    public bool Execute(GitUIEventArgs args)
    {
        DialogResult result = MessageBox.Show(string.Format(TranslatedStrings.FailedToLoadPluginPopupText, _exception),
            TranslatedStrings.FailedToLoadPlugin, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
        if (result == DialogResult.OK)
        {
            Clipboard.SetText(_exception);
        }

        return false;
    }

    public IEnumerable<ISetting> GetSettings() => Array.Empty<ISetting>();

    public void Register(IGitUICommands gitUiCommands)
    {
    }

    public void Unregister(IGitUICommands gitUiCommands)
    {
    }
}
