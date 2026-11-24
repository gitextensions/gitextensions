using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Translations;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
///  Offers host Git only inside Flatpak, using the existing Git command setting.
/// </summary>
public static class FlatpakHostGit
{
    private static Func<bool> _isFlatpak = FlatpakEnvironment.IsFlatpak;
    private static Func<string> _launcherPath = () => Path.Combine(AppContext.BaseDirectory, "gitextensions-host-git");
    private static Func<string> _bundledPath = () => Path.Combine(AppContext.BaseDirectory, "git");
    private static Func<ProcessStartInfo, Task<(int ExitCode, string Output, string Error)>> _query = FlatpakHost.QueryAsync;

    public static string LauncherPath => _launcherPath();

    public static void InitializeEnvironment()
    {
        if (!_isFlatpak())
        {
            return;
        }

        using HostGitMessages messages = HostGitMessages.GetTranslated();
        InitializeEnvironment(messages, Environment.SetEnvironmentVariable);
        SetSelection(IsLauncher(AppSettings.GitCommandValue));
    }

    internal static void InitializeEnvironment(HostGitMessages messages, Action<string, string?> setEnvironment)
    {
        setEnvironment("GITEXTENSIONS_HOST_GIT_MIN_VERSION", GitVersion.LastSupportedVersion.ToString());
        setEnvironment("GITEXTENSIONS_HOST_GIT_MISSING_MESSAGE", messages.Missing);
        setEnvironment("GITEXTENSIONS_HOST_GIT_OLD_MESSAGE", messages.Old);
        setEnvironment("GITEXTENSIONS_HOST_GIT_UNAVAILABLE_MESSAGE", messages.Unavailable);
    }

    public static Task NotifyIfFallingBackAsync()
        => NotifyIfFallingBackAsync(AppSettings.GitCommandValue);

    private static async Task NotifyIfFallingBackAsync(string? selectedCommand)
    {
        if (!_isFlatpak() || !IsLauncher(selectedCommand))
        {
            return;
        }

        using HostGitMessages messages = HostGitMessages.GetTranslated();
        string? message = await GetFallbackMessageAsync(_query, messages);
        if (message is not null)
        {
            MessageBoxes.Show(owner: null, message, TranslatedStrings.Warning,
                WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Warning);
        }
    }

    internal static async Task<string?> GetFallbackMessageAsync(
        Func<ProcessStartInfo, Task<(int ExitCode, string Output, string Error)>> query,
        HostGitMessages messages)
    {
        ProcessStartInfo request = new("/bin/sh");
        request.ArgumentList.Add("-c");
        request.ArgumentList.Add("command -v git >/dev/null 2>&1 || exit 44; exec git --version");
        FlatpakHost.ForwardToHost(request);
        try
        {
            (int exitCode, string output, _) = await query(request);
            return exitCode switch
            {
                44 => messages.Missing,
                0 => new GitVersion(output) < GitVersion.LastSupportedVersion ? messages.Old : null,
                _ => messages.Unavailable,
            };
        }
        catch (Exception exception) when (exception is Win32Exception or IOException or PlatformNotSupportedException)
        {
            Trace.TraceWarning(exception.ToString());
            return messages.Unavailable;
        }
    }

    internal static void Attach(Control owner, TextBox command)
    {
        if (Design.IsDesignMode || !_isFlatpak())
        {
            return;
        }

        Grid grid = command.Parent as Grid ?? throw new InvalidOperationException("The Git command must belong to its paths grid.");
        using HostGitMessages messages = HostGitMessages.GetTranslated();
        const string optionName = "_NO_TRANSLATE_chkUseHostGit";
        CheckBox option = new()
        {
            Name = optionName,
            Content = messages.UseHostGit,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(3, 2),
            IsChecked = IsLauncher(command.Text),
        };
        Grid.SetRow(option, grid.RowDefinitions.Count);
        Grid.SetColumnSpan(option, 3);
        int lastTabIndex = grid.GetLogicalDescendants().OfType<Control>().Where(control => control.Focusable)
            .Select(KeyboardNavigation.GetTabIndex).DefaultIfEmpty(0).Max();
        KeyboardNavigation.SetTabIndex(option, lastTabIndex == int.MaxValue ? lastTabIndex : lastTabIndex + 1);
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grid.Children.Add(option);
        InputAccessibility.Apply(option);
        bool synchronizing = false;
        option.IsCheckedChanged += (_, _) =>
        {
            if (synchronizing)
            {
                return;
            }

            if (option.IsChecked == true)
            {
                InitializeEnvironment();
                SetSelection(true);
                command.Text = LauncherPath;
                owner.InvokeAndForget(() => NotifyIfFallingBackAsync(command.Text));
            }
            else
            {
                SetSelection(false);
                if (IsLauncher(command.Text))
                {
                    command.Text = _bundledPath();
                }
            }
        };
        command.TextChanged += (_, _) =>
        {
            synchronizing = true;
            option.IsChecked = IsLauncher(command.Text);
            SetSelection(IsLauncher(AppSettings.GitCommandValue));
            synchronizing = false;
        };
    }

    private static bool IsLauncher(string? command)
        => string.Equals(command, LauncherPath, StringComparison.Ordinal);

    private static void SetSelection(bool selected)
        => Environment.SetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_SELECTED", selected ? "1" : "0");

    // parity-scaffolding: substitutes only package/platform and host-service answers for settings wiring tests.
    internal static class TestAccessor
    {
        internal static Func<bool> IsFlatpak
        {
            get => _isFlatpak;
            set => _isFlatpak = value;
        }

        internal static Func<string> LauncherPath
        {
            get => _launcherPath;
            set => _launcherPath = value;
        }

        internal static Func<string> BundledPath
        {
            get => _bundledPath;
            set => _bundledPath = value;
        }

        internal static Func<ProcessStartInfo, Task<(int ExitCode, string Output, string Error)>> Query
        {
            get => _query;
            set => _query = value;
        }
    }
}

internal sealed class HostGitMessages : ITranslate
{
    private readonly TranslationString _useHostGit = new("Use Git installed on the host");
    private readonly TranslationString _missing = new("Host Git was not found. Using the bundled Git.");
    private readonly TranslationString _old = new("Host Git is older than the required version ({0}). Using the bundled Git.");
    private readonly TranslationString _unavailable = new("Host Git is not available in this Flatpak installation. Using the bundled Git.");

    internal string UseHostGit => _useHostGit.Text;
    internal string Missing => _missing.Text;
    internal string Old => string.Format(_old.Text, GitVersion.LastSupportedVersion);
    internal string Unavailable => _unavailable.Text;

    internal static HostGitMessages GetTranslated()
    {
        HostGitMessages messages = new();
        Translator.Translate(messages, AppSettings.CurrentTranslation);
        return messages;
    }

    public void AddTranslationItems(ITranslation translation)
        => TranslationUtils.AddTranslationItemsFromFields(nameof(HostGitMessages), this, translation);

    public void TranslateItems(ITranslation translation)
        => TranslationUtils.TranslateItemsFromFields(nameof(HostGitMessages), this, translation);

    public void Dispose()
        => GC.SuppressFinalize(this);
}
