using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using Microsoft.VisualStudio.Threading;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class SettingsDialogTests
{
    [AvaloniaTest]
    public void FormBrowse_should_expose_the_live_settings_toolbar_action()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        FormBrowse form = new();

        Button settings = form.FindControl<Button>("EditSettings")
            ?? throw new InvalidOperationException("Settings toolbar button was not created.");
        settings.Content.Should().Be("Settings");
        settings.IsVisible.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormSettings_should_host_the_placeholder_and_preserve_dialog_chrome()
    {
        FormSettings form = new();
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            FormSettings.TestAccessor accessor = form.GetTestAccessor();
            accessor.CurrentPage.Should().BeOfType<SettingsPlaceholderPage>();
            accessor.OkButton.IsDefault.Should().BeTrue();
            accessor.CancelButton.Content.Should().Be("Cancel");
            form.Title.Should().Be($"Settings - {GitCommands.AppSettings.ApplicationName}");
            form.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormSettings_should_register_and_navigate_to_the_general_page()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();

        GeneralSettingsPage page = accessor.SettingsTreeView.SettingsPages
            .OfType<GeneralSettingsPage>()
            .Single();
        form.GotoPage(GeneralSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        SettingsPageHeader.TestAccessor headerAccessor = header.GetTestAccessor();
        headerAccessor.Page.Should().BeSameAs(page);
        headerAccessor.Global.IsChecked.Should().BeTrue();
        headerAccessor.Effective.IsVisible.Should().BeFalse();
        page.GetTitle().Should().Be("General");
    }

    [AvaloniaTest]
    public void General_settings_should_preserve_special_load_and_save_mappings()
    {
        int originalMaxCommits = AppSettings.MaxRevisionGraphCommits;
        bool originalToolbarStatus = AppSettings.ShowGitStatusInBrowseToolbar;
        bool originalArtificialStatus = AppSettings.ShowGitStatusForArtificialCommits;
        bool originalSubmoduleStatus = AppSettings.ShowSubmoduleStatus;
        bool? originalUpdateModules = AppSettings.UpdateSubmodulesOnCheckout;
        GitPullAction originalPullAction = AppSettings.DefaultPullAction;
        string originalCloneDestination = AppSettings.DefaultCloneDestinationPath;
        try
        {
            AppSettings.MaxRevisionGraphCommits = 42000;
            AppSettings.ShowGitStatusInBrowseToolbar = false;
            AppSettings.ShowGitStatusForArtificialCommits = false;
            AppSettings.ShowSubmoduleStatus = true;
            AppSettings.UpdateSubmodulesOnCheckout = null;
            AppSettings.DefaultPullAction = GitPullAction.FetchAll;
            AppSettings.DefaultCloneDestinationPath = @"D:\Repositories";

            FormSettings form = new();
            FormSettings.TestAccessor formAccessor = form.GetTestAccessor();
            formAccessor.InitializePages();
            GeneralSettingsPage page = formAccessor.SettingsTreeView.SettingsPages
                .OfType<GeneralSettingsPage>()
                .Single();
            page.LoadSettings();

            GeneralSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.LimitCommits.IsChecked.Should().BeTrue();
            accessor.MaxCommits.Value.Should().Be(42000);
            accessor.UpdateModules.IsChecked.Should().BeNull();
            accessor.ShowSubmoduleStatusInBrowse.IsEnabled.Should().BeFalse();
            accessor.ShowSubmoduleStatusInBrowse.IsChecked.Should().BeFalse();
            accessor.DefaultPullAction.SelectedItem!.ToString().Should().Be("Fetch all");
            accessor.DefaultCloneDestination.Text.Should().Be(@"D:\Repositories");

            accessor.ShowGitStatusInToolbar.IsChecked = true;
            accessor.ShowSubmoduleStatusInBrowse.IsChecked = true;
            accessor.LimitCommits.IsChecked = false;
            accessor.UpdateModules.IsChecked = false;
            accessor.DefaultPullAction.SelectedItem = accessor.DefaultPullAction.Items
                .Cast<object>()
                .Single(item => item.ToString() == "Pull - rebase");
            accessor.DefaultCloneDestination.Text = @"C:\Source";
            page.SaveSettings();

            AppSettings.ShowSubmoduleStatus.Should().BeTrue();
            AppSettings.MaxRevisionGraphCommits.Should().Be(0);
            AppSettings.UpdateSubmodulesOnCheckout.Should().BeFalse();
            AppSettings.DefaultPullAction.Should().Be(GitPullAction.Rebase);
            AppSettings.DefaultCloneDestinationPath.Should().Be(@"C:\Source");
        }
        finally
        {
            AppSettings.MaxRevisionGraphCommits = originalMaxCommits;
            AppSettings.ShowGitStatusInBrowseToolbar = originalToolbarStatus;
            AppSettings.ShowGitStatusForArtificialCommits = originalArtificialStatus;
            AppSettings.ShowSubmoduleStatus = originalSubmoduleStatus;
            AppSettings.UpdateSubmodulesOnCheckout = originalUpdateModules;
            AppSettings.DefaultPullAction = originalPullAction;
            AppSettings.DefaultCloneDestinationPath = originalCloneDestination;
        }
    }

    [AvaloniaTest]
    public void Settings_tree_should_preserve_root_replacement_navigation_and_search()
    {
        SettingsTreeViewUserControl tree = new();
        TestPage rootPage = new("Root settings", "root keyword");
        TestPage childPage = new("Child page", "searchable option");
        SettingsPageReference rootReference = new SettingsPageReferenceByType(typeof(RootGroup));

        tree.AddSettingsPage(new RootGroup(), parentPageReference: null, icon: null);
        tree.AddSettingsPage(rootPage, rootReference, icon: null, asRoot: true);
        tree.AddSettingsPage(childPage, rootReference, icon: null);

        List<SettingsPageSelectedEventArgs> selections = [];
        tree.SettingsPageSelected += (_, e) => selections.Add(e);

        Window window = new() { Content = tree, Width = 320, Height = 300 };
        window.Show();
        try
        {
            tree.GotoPage(childPage.PageReference);
            selections.Should().ContainSingle();
            selections[0].SettingsPage.Should().BeSameAs(childPage);
            selections[0].IsTriggeredByGoto.Should().BeTrue();

            TextBox search = tree.FindControl<TextBox>("textBoxFind")
                ?? throw new InvalidOperationException("Settings search box was not created.");
            search.Text = "searchable";
            Dispatcher.UIThread.RunJobs();

            TreeViewItem childNode = (tree.FindControl<TreeView>("treeView1")
                    ?? throw new InvalidOperationException("Settings tree was not created."))
                .Items.OfType<TreeViewItem>().Single()
                .Items.OfType<TreeViewItem>().Single();
            childNode.Classes.Should().Contain("settings-search-match");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void SettingsPageBase_should_run_shared_bindings_and_track_loading_state()
    {
        TestSettingsPage page = new();
        TestBinding binding = new();
        page.AddControlBinding(binding);
        page.AttachHost(new TestHost());

        page.LoadSettings();
        binding.LoadCount.Should().Be(1);
        page.WasLoadingDuringLoad.Should().BeTrue();
        page.SettingsLoaded.Should().BeTrue();

        page.SaveSettings();
        binding.SaveCount.Should().Be(1);
    }

    private sealed class RootGroup : GroupSettingsPage
    {
        public RootGroup()
            : base("Root")
        {
        }
    }

    private sealed class TestPage(string title, params string[] keywords) : ISettingsPage
    {
        public Control GuiControl { get; } = new Border();

        public bool IsInstantSavePage => false;

        public SettingsPageReference PageReference { get; } = new TestPageReference(Guid.NewGuid());

        public string GetTitle() => title;

        public IEnumerable<string> GetSearchKeywords() => keywords;

        public void OnPageShown()
        {
        }

        public void LoadSettings()
        {
        }

        public void SaveSettings()
        {
        }
    }

    private sealed class TestPageReference(Guid id) : SettingsPageReference
    {
        public override bool Equals(object? obj) => obj is TestPageReference other && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        private Guid Id { get; } = id;
    }

    private sealed class TestSettingsPage : SettingsPageBase
    {
        private readonly TestSettingsSource _settings = new();

        public TestSettingsPage()
            : base(EmptyServiceProvider.Instance)
        {
            Text = "Test";
        }

        public bool WasLoadingDuringLoad { get; private set; }

        public bool SettingsLoaded => IsSettingsLoaded;

        public void AttachHost(ISettingsPageHost host) => Init(host);

        protected override SettingsSource GetCurrentSettings()
        {
            WasLoadingDuringLoad = IsLoadingSettings;
            return _settings;
        }
    }

    private sealed class TestBinding : ISettingControlBinding
    {
        public int LoadCount { get; private set; }

        public int SaveCount { get; private set; }

        public string Caption() => string.Empty;

        public WinFormsShims.Control GetControl() => new();

        public GitExtensions.Extensibility.Settings.ISetting GetSetting()
            => throw new NotSupportedException();

        public void LoadSetting(SettingsSource settings) => LoadCount++;

        public void SaveSetting(SettingsSource settings) => SaveCount++;
    }

    private sealed class TestSettingsSource : SettingsSource
    {
        public override string? GetValue(string name) => null;

        public override void SetValue(string name, string? value)
        {
        }
    }

    private sealed class TestHost : ISettingsPageHost
    {
        public CheckSettingsLogic CheckSettingsLogic => throw new NotSupportedException();

        public void GotoPage(SettingsPageReference settingsPageReference)
        {
        }

        public void LoadAll()
        {
        }

        public void SaveAll()
        {
        }
    }
}
