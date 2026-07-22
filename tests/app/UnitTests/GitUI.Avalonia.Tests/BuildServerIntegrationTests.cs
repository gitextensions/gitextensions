using AppVeyorIntegration;
using AppVeyorIntegration.Settings;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.HelperDialogs;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class BuildServerIntegrationTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        ManagedExtensibility.Initialise([typeof(AppVeyorIntegrationMetadata).Assembly]);
    }

    [SetUp]
    public void SetUp()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public async Task Build_server_settings_page_should_be_registered_translated_and_populate_the_selector()
    {
        SettingsSource settings = AppSettings.SettingsContainer;
        bool? originalEnabled = BuildServerSettings.IntegrationEnabled[settings];
        bool? originalShowResult = BuildServerSettings.ShowBuildResultPage[settings];
        string? originalServerName = BuildServerSettings.ServerName[settings];
        BuildServerSettings.IntegrationEnabled[settings] = null;
        BuildServerSettings.ShowBuildResultPage[settings] = true;
        BuildServerSettings.ServerName[settings] = null;
        FormSettings form = new();
        try
        {
            FormSettings.TestAccessor formAccessor = form.GetTestAccessor();
            formAccessor.InitializePages();
            BuildServerIntegrationSettingsPage page = formAccessor.SettingsTreeView.SettingsPages
                .OfType<BuildServerIntegrationSettingsPage>()
                .Single();

            form.GotoPage(page.PageReference);
            SettingsPageHeader header = formAccessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
            header.GetTestAccessor().Page.Should().BeSameAs(page);
            page.GetTitle().Should().Be("Build server integration");

            ITranslation translation = Substitute.For<ITranslation>();
            page.AddTranslationItems(translation);
            page.TranslateItems(translation);
            translation.Received(1).AddTranslationItem(
                nameof(BuildServerIntegrationSettingsPage), "$this", "Text", "Build server integration");
            translation.Received(1).AddTranslationItem(
                nameof(BuildServerIntegrationSettingsPage), "_noneItem", "Text", "None");
            translation.Received(1).AddTranslationItem(
                nameof(BuildServerIntegrationSettingsPage), "checkBoxEnableBuildServerIntegration", "Text", "Enable build server integration");
            translation.Received(1).AddTranslationItem(
                nameof(BuildServerIntegrationSettingsPage), "checkBoxShowBuildResultPage", "Text", "Show build result page");
            translation.Received(1).AddTranslationItem(
                nameof(BuildServerIntegrationSettingsPage), "labelBuildServerSettingsInfo", "Text", "Git Extensions can integrate with build servers to supply per-commit Continuous Integration information.");
            translation.Received(1).AddTranslationItem(
                nameof(BuildServerIntegrationSettingsPage), "labelBuildServerType", "Text", "Build server type");

            page.LoadSettings();
            BuildServerIntegrationSettingsPage.TestAccessor pageAccessor = page.GetTestAccessor();
            await pageAccessor.PopulateBuildServerTypeTask!.JoinAsync();
            Dispatcher.UIThread.RunJobs();

            pageAccessor.checkBoxEnableBuildServerIntegration.IsEnabled.Should().BeTrue();
            pageAccessor.checkBoxEnableBuildServerIntegration.IsThreeState.Should().BeTrue();
            pageAccessor.checkBoxShowBuildResultPage.IsEnabled.Should().BeTrue();
            pageAccessor.checkBoxShowBuildResultPage.IsThreeState.Should().BeTrue();
            pageAccessor.BuildServerType.IsEnabled.Should().BeTrue();
            pageAccessor.checkBoxEnableBuildServerIntegration.IsChecked.Should().BeNull();
            pageAccessor.checkBoxShowBuildResultPage.IsChecked.Should().BeTrue();
            pageAccessor.BuildServerType.SelectedIndex.Should().Be(0);
            string[] buildServerTypes = [.. pageAccessor.BuildServerType.Items.Cast<string>()];
            buildServerTypes.Should().Equal("None", "AppVeyor");
            pageAccessor.buildServerSettingsPanel.Content.Should().BeNull(
                "the parameterless settings form intentionally has no repository module");

            pageAccessor.checkBoxEnableBuildServerIntegration.IsChecked = true;
            pageAccessor.checkBoxShowBuildResultPage.IsChecked = false;
            page.SaveSettings();
            BuildServerSettings.IntegrationEnabled[settings].Should().BeTrue();
            BuildServerSettings.ShowBuildResultPage[settings].Should().BeFalse();
            BuildServerSettings.ServerName[settings].Should().BeNull();

            form.Show();
            Dispatcher.UIThread.RunJobs();
            form.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            BuildServerSettings.IntegrationEnabled[settings] = originalEnabled;
            BuildServerSettings.ShowBuildResultPage[settings] = originalShowResult;
            BuildServerSettings.ServerName[settings] = originalServerName;
            form.GotoPage(new SettingsPageReferenceByType(typeof(SettingsPlaceholderPage)));
            form.Close();
        }
    }

    [AvaloniaTest]
    public void AppVeyor_settings_control_should_be_a_native_export_and_round_trip_settings()
    {
        Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata> export = ManagedExtensibility
            .GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>()
            .Single(item => item.Metadata.BuildServerType == "AppVeyor");
        AppVeyorSettingsUserControl control = export.Value.Should()
            .BeAssignableTo<Control>()
            .Which.Should()
            .BeOfType<AppVeyorSettingsUserControl>()
            .Subject;
        TextBox projectName = control.FindControl<TextBox>("AppVeyorProjectName")!;
        TextBox accountName = control.FindControl<TextBox>("AppVeyorAccountName")!;
        TextBox accountToken = control.FindControl<TextBox>("AppVeyorAccountToken")!;
        CheckBox loadTestResults = control.FindControl<CheckBox>("cbLoadTestResults")!;

        ITranslation translation = Substitute.For<ITranslation>();
        control.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(
            nameof(AppVeyorSettingsUserControl), "cbLoadTestResults", "Text",
            "Display test results in build status summary for each build result (network intensive!)");

        TestSettingsSource settings = new();
        control.Initialize("default-project", []);
        control.LoadSettings(settings);
        projectName.Text.Should().Be("default-project");
        accountName.Text.Should().BeNullOrEmpty();
        accountToken.Text.Should().BeNullOrEmpty();
        loadTestResults.IsThreeState.Should().BeTrue();
        loadTestResults.IsChecked.Should().BeNull();

        settings.SetString("AppVeyorProjectName", "project-one|project-two");
        settings.SetString("AppVeyorAccountName", "account");
        settings.SetString("AppVeyorAccountToken", "token");
        settings.SetBool("AppVeyorLoadTestsResults", true);
        control.LoadSettings(settings);
        projectName.Text.Should().Be("project-one|project-two");
        accountName.Text.Should().Be("account");
        accountToken.Text.Should().Be("token");
        loadTestResults.IsChecked.Should().BeTrue();

        projectName.Text = "saved-project";
        accountName.Text = string.Empty;
        accountToken.Text = "saved-token";
        loadTestResults.IsChecked = null;
        control.SaveSettings(settings);
        settings.GetString("AppVeyorProjectName", null).Should().Be("saved-project");
        settings.GetString("AppVeyorAccountName", null).Should().BeNull();
        settings.GetString("AppVeyorAccountToken", null).Should().Be("saved-token");
        settings.GetBool("AppVeyorLoadTestsResults").Should().BeNull();
    }

    [AvaloniaTest]
    public void Credentials_dialog_should_retain_the_original_authentication_controls()
    {
        using FormBuildServerCredentials form = new("CI server");

        form.FindControl<TextBlock>("labelHeader")!.Text.Should().Contain("CI server");
        form.FindControl<RadioButton>("radioButtonGuestAccess").Should().NotBeNull();
        form.FindControl<RadioButton>("radioButtonAuthenticatedUser").Should().NotBeNull();
        form.FindControl<RadioButton>("radioButtonBearerToken").Should().NotBeNull();
        form.FindControl<TextBox>("textBoxUserName").Should().NotBeNull();
        form.FindControl<TextBox>("textBoxPassword").Should().NotBeNull();
        form.FindControl<TextBox>("textBoxBearerToken").Should().NotBeNull();
        form.FindControl<TextBlock>("label2").Should().NotBeNull();
        form.FindControl<TextBlock>("label3").Should().NotBeNull();
        form.FindControl<TextBlock>("label4").Should().NotBeNull();
        form.FindControl<Button>("buttonOK").Should().NotBeNull();
        form.FindControl<Button>("buttonCancel").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void Build_status_column_should_apply_user_display_settings_and_status_content()
    {
        bool originalShowIcon = AppSettings.ShowBuildStatusIconColumn;
        bool originalShowText = AppSettings.ShowBuildStatusTextColumn;
        bool originalShowTooltips = AppSettings.ShowRevisionGridTooltips.Value;
        try
        {
            AppSettings.ShowBuildStatusIconColumn = true;
            AppSettings.ShowBuildStatusTextColumn = true;
            AppSettings.ShowRevisionGridTooltips.Value = true;
            GitRevision revision = new(ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
            {
                BuildStatus = new BuildInfo
                {
                    Status = BuildStatus.Failure,
                    Description = "Failed",
                    Tooltip = "Build failed",
                    Url = "https://example.test/build/1",
                },
            };
            BuildStatusColumnProvider provider = new(_ => { });
            provider.Column.IsAvailable = true;

            provider.ApplySettings();
            TextBlock cell = (TextBlock)provider.CreateCell();
            cell.DataContext = revision;
            provider.UpdateCell(cell, revision);

            provider.Column.IsVisible.Should().BeTrue();
            provider.Column.Resizable.Should().BeTrue();
            provider.Column.Width.Should().Be(new GridLength(150));
            cell.Text.Should().Be("❌Failed");
            cell.Foreground.Should().BeSameAs(Brushes.DarkRed);
            ToolTip.GetTip(cell).Should().Be("Build failed");

            AppSettings.ShowBuildStatusTextColumn = false;
            provider.ApplySettings();
            provider.Column.Width.Should().Be(new GridLength(16));
            provider.Column.Resizable.Should().BeFalse();
        }
        finally
        {
            AppSettings.ShowBuildStatusIconColumn = originalShowIcon;
            AppSettings.ShowBuildStatusTextColumn = originalShowText;
            AppSettings.ShowRevisionGridTooltips.Value = originalShowTooltips;
        }
    }

    [AvaloniaTest]
    public void Build_report_tab_should_follow_selected_revision_status()
    {
        SettingsSource settings = Substitute.For<SettingsSource>();
        settings.GetValue(BuildServerSettings.ShowBuildResultPage.Name).Returns("true");
        IGitModule module = Substitute.For<IGitModule>();
        module.GetEffectiveSettings().Returns(settings);
        TabControl tabs = new();
        tabs.Items.Add(new TabItem { Header = "Diff" });
        BuildReportTabPageExtension extension = new(() => module, tabs, "Build Report");
        GitRevision revision = new(ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"));

        extension.FillBuildReport(revision);
        tabs.Items.Should().HaveCount(1);

        revision.BuildStatus = new BuildInfo { Url = "https://example.test/build/1" };
        Dispatcher.UIThread.RunJobs();
        tabs.Items.Should().HaveCount(2);
        TabItem buildTab = tabs.Items.OfType<TabItem>().Single(item => Equals(item.Header, "Build Report"));
        buildTab.Content.Should().Be(extension.Control).And.BeOfType<HyperlinkButton>();

        revision.BuildStatus = null;
        Dispatcher.UIThread.RunJobs();
        tabs.Items.Should().HaveCount(1);
    }

    private sealed class TestSettingsSource : SettingsSource
    {
        private readonly Dictionary<string, string?> _values = [];

        public override string? GetValue(string name)
            => _values.GetValueOrDefault(name);

        public override void SetValue(string name, string? value)
            => _values[name] = value;
    }
}
