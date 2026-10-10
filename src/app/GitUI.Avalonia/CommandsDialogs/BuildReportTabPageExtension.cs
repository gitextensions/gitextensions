using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Layout;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

public class BuildReportTabPageExtension
{
    private readonly TabControl _tabControl;
    private readonly string _caption;
    private readonly Func<IGitModule> _getModule;
    private TabItem? _buildReportTabPage;
    private GitRevision? _selectedGitRevision;
    private string? _url;
    private readonly HyperlinkButton _openReportLink;

    public Control? Control { get; private set; }

    public BuildReportTabPageExtension(Func<IGitModule> getModule, TabControl tabControl, string caption)
    {
        _getModule = getModule;
        _tabControl = tabControl;
        _caption = caption;
        _openReportLink = new HyperlinkButton
        {
            Content = TranslatedStrings.OpenReport,
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _openReportLink.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(_url);
    }

    public void FillBuildReport(GitRevision? revision)
    {
        SetSelectedRevision(revision);
        bool showReport = revision is not null
            && BuildServerSettings.ShowBuildResultPage.ValueOrDefault(GetModule().GetEffectiveSettings())
            && !string.IsNullOrWhiteSpace(revision.BuildStatus?.Url);

        if (!showReport)
        {
            RemoveBuildReportTab();
            return;
        }

        _buildReportTabPage ??= new TabItem
        {
            Header = _caption,
            Icon = Properties.Images.Integration,
            Classes = { "gitextensions-workspace-tab" },
        };
        _url = revision!.BuildStatus!.Url;
        _buildReportTabPage.Content = _openReportLink;
        Control = _openReportLink;
        if (!_tabControl.Items.Contains(_buildReportTabPage))
        {
            _tabControl.Items.Add(_buildReportTabPage);
        }
    }

    private void RemoveBuildReportTab()
    {
        _url = null;
        Control = null;
        if (_buildReportTabPage is not null)
        {
            _tabControl.Items.Remove(_buildReportTabPage);
        }
    }

    private void SetSelectedRevision(GitRevision? revision)
    {
        _selectedGitRevision?.PropertyChanged -= RevisionPropertyChanged;
        _selectedGitRevision = revision;
        _selectedGitRevision?.PropertyChanged += RevisionPropertyChanged;
    }

    private void RevisionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GitRevision.BuildStatus))
        {
            // Refresh the selected Git revision
            FillBuildReport(_selectedGitRevision);
        }
    }

    private IGitModule GetModule()
        => _getModule() ?? throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
}
