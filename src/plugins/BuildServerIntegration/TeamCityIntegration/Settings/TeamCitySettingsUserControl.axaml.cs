using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using MessageBoxes = GitExtensions.Extensibility.MessageBoxes;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace TeamCityIntegration.Settings;

[Export(typeof(IBuildServerSettingsUserControl))]
[BuildServerSettingsUserControlMetadata(TeamCityAdapter.PluginName)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class TeamCitySettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
{
    private string? _defaultProjectName;
    private readonly TeamCityAdapter _teamCityAdapter = new();
    private readonly TranslationString _failToLoadProjectMessage = new("Failed to load the projects and build list." + Environment.NewLine + "Please verify the server url.");
    private readonly TranslationString _failToLoadProjectCaption = new("Error when loading the projects and build list");
    private readonly TranslationString _failToExtractDataFromClipboardMessage = new("The clipboard doesn't contain a valid build url." + Environment.NewLine + Environment.NewLine +
            "Please copy in the clipboard the url of the build before retrying." + Environment.NewLine +
            "(Should contain at least the \"buildTypeId\" parameter)");
    private readonly TranslationString _failToExtractDataFromClipboardCaption = new("Build url not valid");

    [GeneratedRegex(@"(\?|\&)(?<buildtypeid>[^=]+)\=(?<buildtype>[^&]+)", RegexOptions.ExplicitCapture)]
    private static partial Regex TeamcityBuildUrl { get; }

    public TeamCitySettingsUserControl()
    {
        InitializeComponent();
        TeamCityServerUrl.PropertyChanged += TeamCityServerUrl_TextChanged;
        TeamCityBuildIdFilter.PropertyChanged += TeamCityBuildIdFilter_TextChanged;
        buttonProjectChooser.Click += buttonProjectChooser_Click;
        lnkExtractDataFromBuildUrlCopiedInTheClipboard.Click += lnkExtractDataFromBuildUrlCopiedInTheClipboard_LinkClicked;
        InitializeComplete();
    }

    private WinFormsShims.IWin32Window? Owner
        => TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;

    public void Initialize(string defaultProjectName, IEnumerable<string?> remotes)
    {
        _defaultProjectName = defaultProjectName;
        SetChooseBuildButtonState();
    }

    public void LoadSettings(SettingsSource buildServerConfig)
    {
        TeamCityServerUrl.Text = buildServerConfig.GetString("BuildServerUrl", null);
        TeamCityProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
        TeamCityBuildIdFilter.Text = buildServerConfig.GetString("BuildIdFilter", null);
        CheckBoxLogAsGuest.IsChecked = buildServerConfig.GetBool("LogAsGuest", false);
    }

    public void SaveSettings(SettingsSource buildServerConfig)
    {
        if (!BuildServerSettingsHelper.IsRegexValid(TeamCityBuildIdFilter.Text ?? string.Empty))
        {
            return;
        }

        // Empty string is handled as unset, not overriding lower priority levels
        buildServerConfig.SetString("BuildServerUrl", TeamCityServerUrl.Text.NullIfEmpty());
        buildServerConfig.SetString("ProjectName", TeamCityProjectName.Text.NullIfEmpty());
        buildServerConfig.SetString("BuildIdFilter", TeamCityBuildIdFilter.Text.NullIfEmpty());
        buildServerConfig.SetBool("LogAsGuest", CheckBoxLogAsGuest.IsChecked);
    }

    private void TeamCityBuildIdFilter_TextChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == AvaloniaTextBox.TextProperty)
        {
            labelRegexError.IsVisible = !BuildServerSettingsHelper.IsRegexValid(TeamCityBuildIdFilter.Text ?? string.Empty);
        }
    }

    private void buttonProjectChooser_Click(object? sender, EventArgs e)
    {
        try
        {
            using TeamCityBuildChooser teamCityBuildChooser = new(
                TeamCityServerUrl.Text ?? string.Empty,
                TeamCityProjectName.Text ?? string.Empty,
                TeamCityBuildIdFilter.Text ?? string.Empty);
            WinFormsShims.DialogResult result = teamCityBuildChooser.ShowDialog(Owner);

            if (result == WinFormsShims.DialogResult.OK)
            {
                TeamCityProjectName.Text = teamCityBuildChooser.TeamCityProjectName;
                TeamCityBuildIdFilter.Text = teamCityBuildChooser.TeamCityBuildIdFilter;
            }
        }
        catch
        {
            MessageBoxes.ShowError(Owner, _failToLoadProjectMessage.Text, _failToLoadProjectCaption.Text);
        }
    }

    private void TeamCityServerUrl_TextChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == AvaloniaTextBox.TextProperty)
        {
            SetChooseBuildButtonState();
        }
    }

    private void SetChooseBuildButtonState()
    {
        buttonProjectChooser.IsEnabled = !string.IsNullOrWhiteSpace(TeamCityServerUrl.Text);
    }

    private void lnkExtractDataFromBuildUrlCopiedInTheClipboard_LinkClicked(object? sender, EventArgs e)
    {
        if (Clipboard.ContainsText() && Clipboard.GetText().Contains("buildTypeId="))
        {
            Uri buildUri = new(Clipboard.GetText());
            string teamCityServerUrl = buildUri.Scheme + "://" + buildUri.Authority;
            TeamCityServerUrl.Text = teamCityServerUrl;
            _teamCityAdapter.InitializeHttpClient(teamCityServerUrl);

            MatchCollection paramResults = TeamcityBuildUrl.Matches(buildUri.Query);
            foreach (Match paramResult in paramResults)
            {
                if (paramResult.Success)
                {
                    if (paramResult.Groups["buildtypeid"].ValueSpan is "buildTypeId")
                    {
                        Build buildType = _teamCityAdapter.GetBuildType(paramResult.Groups["buildtype"].Value);
                        TeamCityProjectName.Text = buildType.ParentProject;
                        TeamCityBuildIdFilter.Text = buildType.Id;
                        return;
                    }
                }
            }
        }

        MessageBoxes.Show(
            Owner,
            _failToExtractDataFromClipboardMessage.Text,
            _failToExtractDataFromClipboardCaption.Text,
            WinFormsShims.MessageBoxButtons.OK,
            WinFormsShims.MessageBoxIcon.Warning);
    }
}
