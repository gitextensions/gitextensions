using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils;
using ResourceManager;

namespace GitExtensions.Plugins.ProxySwitcher;

public partial class ProxySwitcherForm : GitExtensionsFormBase
{
    private readonly ProxySwitcherPlugin? _plugin;
    private readonly SettingsSource? _settings;
    private readonly IGitModule? _gitCommands;

    #region Translation
    private readonly TranslationString _pluginDescription = new("Proxy Switcher");
    private readonly TranslationString _pleaseSetProxy = new("There is no proxy configured. Please set the proxy host in the plugin settings.");
    #endregion

    [GeneratedRegex(@":(.*)@", RegexOptions.ExplicitCapture)]
    private static partial Regex PasswordRegex { get; }

    /// <summary>
    /// Default constructor added to register all strings to be translated.
    /// Use the other constructor:
    /// ProxySwitcherForm(ProxySwitcherPlugin plugin, SettingsSource settings, GitUIEventArgs gitUiCommands).
    /// </summary>
    public ProxySwitcherForm()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public ProxySwitcherForm(ProxySwitcherPlugin plugin, SettingsSource settings, GitUIEventArgs gitUiCommands)
    {
        _plugin = plugin;
        _settings = settings;
        _gitCommands = gitUiCommands.GitModule;

        InitializeComponent();
        WireControls();
        InitializeComplete();

        Text = _pluginDescription.Text;
    }

    private void WireControls()
    {
        SetProxy_Button.Click += SetProxy_Button_Click;
        UnsetProxy_Button.Click += UnsetProxy_Button_Click;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        ProxySwitcherPlugin plugin = GetPlugin();
        SettingsSource settings = GetSettings();
        if (string.IsNullOrEmpty(plugin.HttpProxy.ValueOrDefault(settings)))
        {
            MessageBoxes.ShowError(this, _pleaseSetProxy.Text, Text);
            Close();
        }
        else
        {
            RefreshProxy();
        }
    }

    private void RefreshProxy()
    {
        IGitModule gitCommands = GetGitCommands();
        LocalHttpProxy_TextBox.Text = HidePassword(gitCommands.GetEffectiveSetting("http.proxy"));
        GlobalHttpProxy_TextBox.Text = HidePassword(
            new GitConfigSettings(gitCommands.GitExecutable, GitSettingLevel.Global).GetValue("http.proxy") ?? string.Empty);
        ApplyGlobally_CheckBox.IsChecked = string.Equals(LocalHttpProxy_TextBox.Text, GlobalHttpProxy_TextBox.Text);
    }

    private static string HidePassword(string httpProxy)
    {
        return PasswordRegex.Replace(httpProxy, ":****@");
    }

    private string BuildHttpProxy()
    {
        ProxySwitcherPlugin plugin = GetPlugin();
        SettingsSource settings = GetSettings();
        StringBuilder sb = new();
        sb.Append('"');
        string username = plugin.Username.ValueOrDefault(settings);
        if (!string.IsNullOrEmpty(username))
        {
            string password = plugin.Password.ValueOrDefault(settings);
            sb.Append(username);
            if (!string.IsNullOrEmpty(password))
            {
                sb.Append(':');
                sb.Append(password);
            }

            sb.Append('@');
        }

        sb.Append(plugin.HttpProxy.ValueOrDefault(settings));
        string port = plugin.HttpProxyPort.ValueOrDefault(settings);
        if (!string.IsNullOrEmpty(port))
        {
            sb.Append(':');
            sb.Append(port);
        }

        sb.Append('"');
        return sb.ToString();
    }

    private void SetProxy_Button_Click(object? sender, EventArgs e)
    {
        string httpProxy = BuildHttpProxy();

        GitArgumentBuilder args = new("config")
        {
            { ApplyGlobally_CheckBox.IsChecked == true, "--global" },
            "http.proxy",
            httpProxy
        };
        IGitModule gitCommands = GetGitCommands();
        gitCommands.GitExecutable.GetOutput(args);
        gitCommands.InvalidateGitSettings();

        RefreshProxy();
    }

    private void UnsetProxy_Button_Click(object? sender, EventArgs e)
    {
        string arguments = ApplyGlobally_CheckBox.IsChecked == true
            ? "config --global --unset http.proxy"
            : "config --unset http.proxy";

        IGitModule gitCommands = GetGitCommands();
        gitCommands.GitExecutable.GetOutput(arguments);
        gitCommands.InvalidateGitSettings();

        RefreshProxy();
    }

    private ProxySwitcherPlugin GetPlugin()
        => _plugin ?? throw new InvalidOperationException($"{nameof(ProxySwitcherForm)} was constructed incorrectly.");

    private SettingsSource GetSettings()
        => _settings ?? throw new InvalidOperationException($"{nameof(ProxySwitcherForm)} was constructed incorrectly.");

    private IGitModule GetGitCommands()
        => _gitCommands ?? throw new InvalidOperationException($"{nameof(ProxySwitcherForm)} was constructed incorrectly.");

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ProxySwitcherForm form)
    {
        public Avalonia.Controls.CheckBox ApplyGlobally => form.ApplyGlobally_CheckBox;
        public string GlobalProxy => form.GlobalHttpProxy_TextBox.Text ?? string.Empty;
        public string LocalProxy => form.LocalHttpProxy_TextBox.Text ?? string.Empty;

        public string BuildHttpProxy() => form.BuildHttpProxy();
        public void RefreshProxy() => form.RefreshProxy();
        public void SetProxy() => form.SetProxy_Button_Click(form.SetProxy_Button, EventArgs.Empty);
        public void UnsetProxy() => form.UnsetProxy_Button_Click(form.UnsetProxy_Button, EventArgs.Empty);
    }
}
