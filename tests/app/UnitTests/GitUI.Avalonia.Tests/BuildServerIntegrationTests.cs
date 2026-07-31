using System.Net;
using System.Net.Sockets;
using System.Text;
using AppVeyorIntegration;
using AppVeyorIntegration.Settings;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AzureDevOpsIntegration;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.GitHubActionsIntegration;
using GitExtensions.Plugins.GitHubActionsIntegration.Settings;
using GitExtensions.Plugins.GitlabIntegration;
using GitExtensions.Plugins.GitlabIntegration.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.HelperDialogs;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using JenkinsIntegration;
using JenkinsIntegration.Settings;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using TeamCityIntegration;
using TeamCityIntegration.Settings;
using AzureDevOpsSettingsUserControl = AzureDevOpsIntegration.Settings.SettingsUserControl;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class BuildServerIntegrationTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        ManagedExtensibility.Initialise(
        [
            typeof(AppVeyorIntegrationMetadata).Assembly,
            typeof(AzureDevOpsIntegrationMetadata).Assembly,
            typeof(GitHubActionsIntegrationMetadataAttribute).Assembly,
            typeof(GitlabIntegrationMetadataAttribute).Assembly,
            typeof(JenkinsIntegrationMetadata).Assembly,
            typeof(TeamCityIntegrationMetadataAttribute).Assembly,
        ]);
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
            buildServerTypes.Should().StartWith("None");
            buildServerTypes[1..].Should().BeEquivalentTo(
                "AppVeyor",
                "Azure DevOps and Team Foundation Server (since TFS2015)",
                "GitHub Actions",
                "Gitlab",
                "Jenkins",
                "TeamCity");
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
    public async Task Azure_DevOps_settings_control_should_be_a_native_export_and_round_trip_settings()
    {
        const string category = nameof(AzureDevOpsIntegration.Settings.SettingsUserControl);
        const string pluginName = "Azure DevOps and Team Foundation Server (since TFS2015)";
        Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata> export = ManagedExtensibility
            .GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>()
            .Single(item => item.Metadata.BuildServerType == pluginName);
        AzureDevOpsSettingsUserControl control = export.Value.Should()
            .BeAssignableTo<Control>()
            .Which.Should()
            .BeOfType<AzureDevOpsSettingsUserControl>()
            .Subject;
        TextBox projectUrl = control.FindControl<TextBox>("TfsServer")!;
        TextBox definitionFilter = control.FindControl<TextBox>("TfsBuildDefinitionNameFilter")!;
        TextBox apiToken = control.FindControl<TextBox>("RestApiToken")!;
        HyperlinkButton extract = control.FindControl<HyperlinkButton>("ExtractLink")!;
        HyperlinkButton tokenManagement = control.FindControl<HyperlinkButton>("TokenManagementLink")!;
        TextBlock regexError = control.FindControl<TextBlock>("labelRegexError")!;

        ITranslation translation = Substitute.For<ITranslation>();
        control.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(
            category, "ExtractLink", "Text",
            "Extract data from a build result url copied in the clipboard");
        translation.Received(1).AddTranslationItem(
            category, "TokenManagementLink", "Text",
            "Go to token management page");
        translation.Received(1).AddTranslationItem(
            category, "label1", "Text",
            "Examples:\n - https://dev.azure.com/yourorganization/projectname/\n - https://yourhost:8080/tfs/collectionname/projectname/\n - https://yourorganization.visualstudio.com/projectname/");
        translation.Received(1).AddTranslationItem(
            category, "label4", "Text",
            "You need to create a token with the following scopes:\n - 'Build (read)'\n - 'Project and team (read)'");
        translation.Received(1).AddTranslationItem(
            category, "labelRegexError", "Text",
            "The 'Build definition name' regular expression is not valid and won't be saved!");
        translation.Received(1).AddTranslationItem(
            category, "_failToExtractDataFromClipboardCaption", "Text",
            "Could not extract data");
        translation.Received(1).AddTranslationItem(
            category, "_failToExtractDataFromClipboardMessage", "Text",
            "The clipboard doesn't contain a valid build url." + Environment.NewLine + Environment.NewLine +
            "Please copy the url of the build into the clipboard before retrying." + Environment.NewLine +
            "(Should contain at least the \"buildId\" parameter)");
        translation.Received(1).AddTranslationItem(
            category, "_failToLoadBuildDefinitionInfoMessage", "Text",
            "Error while trying to retrieve build definition information from url." + Environment.NewLine + Environment.NewLine +
            "Please ensure that the url is valid and that the API token has access to build and project information.");
        translation.Received(1).AddTranslationItem(
            category, "_infoNoApiTokenMessage", "Text",
            "Unable to retrieve build definition information without API token. Field will be left blank.");
        translation.ReceivedCalls().Should().HaveCount(9);

        TestSettingsSource settings = new();
        control.Initialize("unused", ["git@ssh.dev.azure.com:v3/example/project/repository"]);
        control.LoadSettings(settings);
        projectUrl.Text.Should().Be("https://dev.azure.com/example/project");
        definitionFilter.Text.Should().BeEmpty();
        apiToken.Text.Should().BeEmpty();
        apiToken.PasswordChar.Should().Be(default);
        tokenManagement.IsEnabled.Should().BeTrue();
        regexError.IsVisible.Should().BeFalse();

        definitionFilter.Text = "[";
        regexError.IsVisible.Should().BeTrue();
        control.SaveSettings(settings);
        settings.GetString("ProjectUrl", null).Should().BeNull();

        projectUrl.Text = "https://dev.azure.com/saved/project";
        definitionFilter.Text = "build-.*";
        apiToken.Text = "configured-token";
        control.SaveSettings(settings);
        settings.GetString("ProjectUrl", null).Should().Be("https://dev.azure.com/saved/project");
        settings.GetString("BuildDefinitionNameFilter", null).Should().Be("build-.*");
        settings.GetString("RestApiToken", null).Should().Be("configured-token");

        IProcess process = Substitute.For<IProcess>();
        IExecutable executable = Substitute.For<IExecutable>();
        executable.Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<System.Text.Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(process);
        OsShellUtil.TestAccessor.MockExecutable = executable;
        try
        {
            tokenManagement.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            executable.Received(1).Start(
                Arg.Any<ArgumentString>(),
                createWindow: false,
                redirectInput: false,
                redirectOutput: false,
                outputEncoding: null,
                useShellExecute: true,
                throwOnErrorExit: false,
                Arg.Any<CancellationToken>());
        }
        finally
        {
            OsShellUtil.TestAccessor.MockExecutable = null;
            process.Dispose();
        }

        StubClipboard clipboard = new("https://dev.azure.com/copied/project/_build/results?buildId=42&view=results");
        StubMessageBoxHost messageBoxes = new();
        WinFormsShims.IClipboard? originalClipboard = TryGetClipboard();
        WinFormsShims.IMessageBoxHost? originalMessageBoxHost = TryGetMessageBoxHost();
        WinFormsShims.ShimHost.Clipboard = clipboard;
        WinFormsShims.ShimHost.MessageBoxHost = messageBoxes;
        apiToken.Text = string.Empty;
        try
        {
            extract.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await WaitUntilAsync(() => projectUrl.Text == "https://dev.azure.com/copied/project");
            definitionFilter.Text.Should().BeEmpty();
            messageBoxes.Messages.Should().ContainSingle()
                .Which.Should().Be("Unable to retrieve build definition information without API token. Field will be left blank.");
        }
        finally
        {
            WinFormsShims.ShimHost.Clipboard = originalClipboard ?? new StubClipboard(string.Empty);
            WinFormsShims.ShimHost.MessageBoxHost = originalMessageBoxHost ?? new StubMessageBoxHost();
        }

        return;

        static WinFormsShims.IClipboard? TryGetClipboard()
        {
            try
            {
                return WinFormsShims.ShimHost.Clipboard;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        static WinFormsShims.IMessageBoxHost? TryGetMessageBoxHost()
        {
            try
            {
                return WinFormsShims.ShimHost.MessageBoxHost;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        static async Task WaitUntilAsync(Func<bool> condition)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                Dispatcher.UIThread.RunJobs();
                if (condition())
                {
                    return;
                }

                await Task.Delay(10);
            }

            condition().Should().BeTrue("the clipboard extraction should complete before the timeout");
        }
    }

    [AvaloniaTest]
    public async Task TeamCity_settings_control_should_be_a_native_export_and_round_trip_settings()
    {
        const string category = nameof(TeamCitySettingsUserControl);
        Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata> export = ManagedExtensibility
            .GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>()
            .Single(item => item.Metadata.BuildServerType == "TeamCity");
        TeamCitySettingsUserControl control = export.Value.Should()
            .BeAssignableTo<Control>()
            .Which.Should()
            .BeOfType<TeamCitySettingsUserControl>()
            .Subject;
        TextBox serverUrl = control.FindControl<TextBox>("TeamCityServerUrl")!;
        TextBox projectName = control.FindControl<TextBox>("TeamCityProjectName")!;
        TextBox buildIdFilter = control.FindControl<TextBox>("TeamCityBuildIdFilter")!;
        CheckBox logAsGuest = control.FindControl<CheckBox>("CheckBoxLogAsGuest")!;
        Button projectChooser = control.FindControl<Button>("buttonProjectChooser")!;
        HyperlinkButton extract = control.FindControl<HyperlinkButton>("lnkExtractDataFromBuildUrlCopiedInTheClipboard")!;
        TextBlock regexError = control.FindControl<TextBlock>("labelRegexError")!;

        ITranslation translation = Substitute.For<ITranslation>();
        control.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(
            category, "CheckBoxLogAsGuest", "Text",
            "Log as guest to display the build report");
        translation.Received(1).AddTranslationItem(
            category, "_failToExtractDataFromClipboardCaption", "Text",
            "Build url not valid");
        translation.Received(1).AddTranslationItem(
            category, "_failToExtractDataFromClipboardMessage", "Text",
            "The clipboard doesn't contain a valid build url." + Environment.NewLine + Environment.NewLine +
            "Please copy in the clipboard the url of the build before retrying." + Environment.NewLine +
            "(Should contain at least the \"buildTypeId\" parameter)");
        translation.Received(1).AddTranslationItem(
            category, "_failToLoadProjectCaption", "Text",
            "Error when loading the projects and build list");
        translation.Received(1).AddTranslationItem(
            category, "_failToLoadProjectMessage", "Text",
            "Failed to load the projects and build list." + Environment.NewLine +
            "Please verify the server url.");
        translation.Received(1).AddTranslationItem(
            category, "labelRegexError", "Text",
            "The \"Build Id Filter\" regular expression is not valid and won't be saved!");
        translation.Received(1).AddTranslationItem(
            category, "lnkExtractDataFromBuildUrlCopiedInTheClipboard", "Text",
            "Extract the data from the build url copied in the clipboard");
        translation.ReceivedCalls().Should().HaveCount(7);

        TestSettingsSource settings = new();
        control.Initialize("default-project", []);
        control.LoadSettings(settings);
        serverUrl.Text.Should().BeNullOrEmpty();
        projectName.Text.Should().Be("default-project");
        buildIdFilter.Text.Should().BeNullOrEmpty();
        logAsGuest.IsChecked.Should().BeFalse();
        projectChooser.IsEnabled.Should().BeFalse();
        regexError.IsVisible.Should().BeFalse();

        serverUrl.Text = "https://teamcity.example.test";
        buildIdFilter.Text = "[";
        regexError.IsVisible.Should().BeTrue();
        projectChooser.IsEnabled.Should().BeTrue();
        control.SaveSettings(settings);
        settings.GetString("BuildServerUrl", null).Should().BeNull();

        projectName.Text = "ProjectA|ProjectB";
        buildIdFilter.Text = "Build.*";
        logAsGuest.IsChecked = null;
        control.SaveSettings(settings);
        settings.GetString("BuildServerUrl", null).Should().Be("https://teamcity.example.test");
        settings.GetString("ProjectName", null).Should().Be("ProjectA|ProjectB");
        settings.GetString("BuildIdFilter", null).Should().Be("Build.*");
        settings.GetBool("LogAsGuest").Should().BeNull();

        await using LoopbackHttpServer httpServer = new();
        httpServer.AddResponse(
            "/guestAuth/app/rest/buildTypes/id:BuildA",
            """<buildType id="BuildA" name="Build A" projectId="ProjectA" />""");

        WinFormsShims.IClipboard? originalClipboard = TryGetClipboard();
        WinFormsShims.IMessageBoxHost? originalMessageBoxHost = TryGetMessageBoxHost();
        StubMessageBoxHost messageBoxes = new();
        WinFormsShims.ShimHost.MessageBoxHost = messageBoxes;
        try
        {
            WinFormsShims.ShimHost.Clipboard = new StubClipboard("not a TeamCity build URL");
            extract.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            messageBoxes.Messages.Should().ContainSingle()
                .Which.Should().StartWith("The clipboard doesn't contain a valid build url.");

            WinFormsShims.ShimHost.Clipboard = new StubClipboard(
                $"{httpServer.BaseUrl}/viewLog.html?buildTypeId=BuildA&buildId=42");
            extract.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            serverUrl.Text.Should().Be(httpServer.BaseUrl);
            projectName.Text.Should().Be("ProjectA");
            buildIdFilter.Text.Should().Be("BuildA");
            messageBoxes.Messages.Should().HaveCount(1);
        }
        finally
        {
            WinFormsShims.ShimHost.Clipboard = originalClipboard ?? new StubClipboard(string.Empty);
            WinFormsShims.ShimHost.MessageBoxHost = originalMessageBoxHost ?? new StubMessageBoxHost();
        }

        return;

        static WinFormsShims.IClipboard? TryGetClipboard()
        {
            try
            {
                return WinFormsShims.ShimHost.Clipboard;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        static WinFormsShims.IMessageBoxHost? TryGetMessageBoxHost()
        {
            try
            {
                return WinFormsShims.ShimHost.MessageBoxHost;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }

    [AvaloniaTest]
    public async Task TeamCity_build_chooser_should_load_projects_lazily_and_select_a_build()
    {
        await using LoopbackHttpServer httpServer = new();
        httpServer.AddResponse(
            "/guestAuth/app/rest/projects",
            """
            <projects>
              <project id="_Root" name="Root" />
              <project id="ProjectA" name="Project A" parentProjectId="_Root" />
            </projects>
            """);
        httpServer.AddResponse(
            "/guestAuth/app/rest/projects/_Root",
            """<project id="_Root"><buildTypes /></project>""");
        httpServer.AddResponse(
            "/guestAuth/app/rest/projects/ProjectA",
            """
            <project id="ProjectA">
              <buildTypes>
                <buildType id="BuildA" name="Build A" projectId="ProjectA" />
              </buildTypes>
            </project>
            """);

        using TeamCityBuildChooser chooser = new(httpServer.BaseUrl, "ProjectA", "BuildA");
        TreeView tree = chooser.FindControl<TreeView>("treeViewTeamCityProjects")!;
        Button buttonOK = chooser.FindControl<Button>("buttonOK")!;
        Button buttonCancel = chooser.FindControl<Button>("buttonCancel")!;
        TreeViewItem root = tree.Items.OfType<TreeViewItem>().Should().ContainSingle().Subject;
        TreeViewItem project = root.Items.OfType<TreeViewItem>()
            .Single(item => item.Tag is TeamCityIntegration.Project { Id: "ProjectA" });
        project.Items.Should().ContainSingle("leaf projects start with the original loading placeholder");
        buttonOK.IsEnabled.Should().BeFalse();

        chooser.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            TreeViewItem build = project.Items.OfType<TreeViewItem>()
                .Single(item => item.Tag is TeamCityIntegration.Build { Id: "BuildA" });
            ((TextBlock)build.Header!).Text.Should().Be("Build A (BuildA)");
            tree.SelectedItem.Should().BeSameAs(build);
            buttonOK.IsEnabled.Should().BeTrue();

            buttonOK.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            chooser.TeamCityProjectName.Should().Be("ProjectA");
            chooser.TeamCityBuildIdFilter.Should().Be("BuildA");
            chooser.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);

            buttonCancel.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            chooser.DialogResult.Should().Be(WinFormsShims.DialogResult.Cancel);
        }
        finally
        {
            chooser.Close();
        }
    }

    [AvaloniaTest]
    public void GitLab_settings_control_should_be_a_native_export_and_round_trip_settings()
    {
        Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata> export = ManagedExtensibility
            .GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>()
            .Single(item => item.Metadata.BuildServerType == "Gitlab");
        GitlabSettingsUserControl control = export.Value.Should()
            .BeAssignableTo<Control>()
            .Which.Should()
            .BeOfType<GitlabSettingsUserControl>()
            .Subject;
        TextBox instanceUrl = control.FindControl<TextBox>("InstanceUrlTextBox")!;
        TextBox projectId = control.FindControl<TextBox>("ProjectIdTextBox")!;
        TextBox apiToken = control.FindControl<TextBox>("ApiTokenTextBox")!;
        HyperlinkButton getProjectId = control.FindControl<HyperlinkButton>("GetProjectIdLink")!;
        HyperlinkButton tokenManagement = control.FindControl<HyperlinkButton>("TokenManagementLink")!;
        TextBlock getProjectIdStatus = control.FindControl<TextBlock>("GetProjectIdStatusText")!;

        ITranslation translation = Substitute.For<ITranslation>();
        control.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(
            nameof(GitlabSettingsUserControl), "label1", "Text", "Instance URL");
        translation.Received(1).AddTranslationItem(
            nameof(GitlabSettingsUserControl), "label2", "Text", "Api Token");
        translation.Received(1).AddTranslationItem(
            nameof(GitlabSettingsUserControl), "label3", "Text", "Project ID");
        translation.Received(1).AddTranslationItem(
            nameof(GitlabSettingsUserControl), "GetProjectIdLink", "Text", "Get Project ID from server");
        translation.Received(1).AddTranslationItem(
            nameof(GitlabSettingsUserControl), "TokenManagementLink", "Text", "Go to token management page");
        translation.Received(1).AddTranslationItem(
            nameof(GitlabSettingsUserControl), "GetProjectIdStatusText", "Text",
            "Failed to obtain project from server. Try to specify valid API token or check instance URL");

        TestSettingsSource settings = new();
        settings.SetInt("ProjectId", 123);
        settings.SetString("ApiToken", "configured-token");
        control.Initialize("unused", ["git@gitlab.example.test:team/repository.git"]);
        control.LoadSettings(settings);
        instanceUrl.Text.Should().Be("https://gitlab.example.test");
        projectId.Text.Should().Be("123");
        apiToken.Text.Should().Be("configured-token");
        apiToken.PasswordChar.Should().Be(default);
        getProjectId.IsEnabled.Should().BeTrue();
        tokenManagement.IsEnabled.Should().BeTrue();
        getProjectIdStatus.IsVisible.Should().BeFalse();

        settings.SetString("InstanceUrl", "https://configured.gitlab.test");
        control.LoadSettings(settings);
        instanceUrl.Text.Should().Be("https://configured.gitlab.test");

        instanceUrl.Text = "not a URL";
        getProjectId.IsEnabled.Should().BeFalse();
        tokenManagement.IsEnabled.Should().BeFalse();

        instanceUrl.Text = "https://saved.gitlab.test";
        projectId.Text = "456";
        apiToken.Text = string.Empty;
        control.SaveSettings(settings);
        settings.GetString("InstanceUrl", null).Should().Be("https://saved.gitlab.test");
        settings.GetInt("ProjectId", 0).Should().Be(456);
        settings.GetString("ApiToken", null).Should().BeNull();
        settings.GetInt("PagesLimit", -1).Should().Be(0);

        IProcess process = Substitute.For<IProcess>();
        IExecutable executable = Substitute.For<IExecutable>();
        executable.Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<System.Text.Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(process);
        OsShellUtil.TestAccessor.MockExecutable = executable;
        try
        {
            tokenManagement.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            executable.Received(1).Start(
                Arg.Any<ArgumentString>(),
                createWindow: false,
                redirectInput: false,
                redirectOutput: false,
                outputEncoding: null,
                useShellExecute: true,
                throwOnErrorExit: false,
                Arg.Any<CancellationToken>());
        }
        finally
        {
            OsShellUtil.TestAccessor.MockExecutable = null;
            process.Dispose();
        }
    }

    [AvaloniaTest]
    public void GitHub_Actions_settings_control_should_be_a_native_export_and_round_trip_settings()
    {
        Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata> export = ManagedExtensibility
            .GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>()
            .Single(item => item.Metadata.BuildServerType == "GitHub Actions");
        GitHubActionsSettingsUserControl control = export.Value.Should()
            .BeAssignableTo<Control>()
            .Which.Should()
            .BeOfType<GitHubActionsSettingsUserControl>()
            .Subject;
        TextBox apiUrl = control.FindControl<TextBox>("txtApiUrl")!;
        TextBox owner = control.FindControl<TextBox>("txtOwner")!;
        TextBox repository = control.FindControl<TextBox>("txtRepository")!;
        TextBox apiToken = control.FindControl<TextBox>("txtApiToken")!;
        HyperlinkButton tokenManagement = control.FindControl<HyperlinkButton>("lnkTokenManagement")!;

        ITranslation translation = Substitute.For<ITranslation>();
        control.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(
            nameof(GitHubActionsSettingsUserControl), "lblApiUrl", "Text", "&API URL");
        translation.Received(1).AddTranslationItem(
            nameof(GitHubActionsSettingsUserControl), "lblOwner", "Text", "&Owner");
        translation.Received(1).AddTranslationItem(
            nameof(GitHubActionsSettingsUserControl), "lblRepository", "Text", "&Repository");
        translation.Received(1).AddTranslationItem(
            nameof(GitHubActionsSettingsUserControl), "lblApiToken", "Text", "API &Token");
        translation.Received(1).AddTranslationItem(
            nameof(GitHubActionsSettingsUserControl), "lnkTokenManagement", "Text",
            "Create a GitHub personal access token");

        TestSettingsSource settings = new();
        control.Initialize("unused", ["git@github.com:gitextensions/gitextensions.git"]);
        control.LoadSettings(settings);
        apiUrl.Text.Should().Be("https://api.github.com");
        owner.Text.Should().Be("gitextensions");
        repository.Text.Should().Be("gitextensions");
        apiToken.Text.Should().BeNullOrEmpty();
        apiToken.PasswordChar.Should().Be('●');

        settings.SetString("GitHubActionsApiUrl", "https://github.example.test/api/v3/");
        settings.SetString("GitHubActionsOwner", "configured-owner");
        settings.SetString("GitHubActionsRepository", "configured-repository");
        settings.SetString("GitHubActionsApiToken", "configured-token");
        control.LoadSettings(settings);
        apiUrl.Text.Should().Be("https://github.example.test/api/v3/");
        owner.Text.Should().Be("configured-owner");
        repository.Text.Should().Be("configured-repository");
        apiToken.Text.Should().Be("configured-token");

        apiUrl.Text = " HTTPS://API.GITHUB.COM/// ";
        owner.Text = string.Empty;
        repository.Text = "saved-repository";
        apiToken.Text = "saved-token";
        control.SaveSettings(settings);
        settings.GetString("GitHubActionsApiUrl", null).Should().BeNull();
        settings.GetString("GitHubActionsOwner", null).Should().BeNull();
        settings.GetString("GitHubActionsRepository", null).Should().Be("saved-repository");
        settings.GetString("GitHubActionsApiToken", null).Should().Be("saved-token");
        tokenManagement.Content.Should().Be("Create a GitHub personal access token");

        IProcess process = Substitute.For<IProcess>();
        IExecutable executable = Substitute.For<IExecutable>();
        executable.Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<System.Text.Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(process);
        OsShellUtil.TestAccessor.MockExecutable = executable;
        try
        {
            tokenManagement.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            executable.Received(1).Start(
                Arg.Any<ArgumentString>(),
                createWindow: false,
                redirectInput: false,
                redirectOutput: false,
                outputEncoding: null,
                useShellExecute: true,
                throwOnErrorExit: false,
                Arg.Any<CancellationToken>());
        }
        finally
        {
            OsShellUtil.TestAccessor.MockExecutable = null;
            process.Dispose();
        }
    }

    [AvaloniaTest]
    public void Jenkins_settings_control_should_be_a_native_export_and_round_trip_settings()
    {
        Lazy<IBuildServerSettingsUserControl, IBuildServerTypeMetadata> export = ManagedExtensibility
            .GetExports<IBuildServerSettingsUserControl, IBuildServerTypeMetadata>()
            .Single(item => item.Metadata.BuildServerType == "Jenkins");
        JenkinsSettingsUserControl control = export.Value.Should()
            .BeAssignableTo<Control>()
            .Which.Should()
            .BeOfType<JenkinsSettingsUserControl>()
            .Subject;
        TextBox serverUrl = control.FindControl<TextBox>("JenkinsServerUrl")!;
        TextBox projectName = control.FindControl<TextBox>("JenkinsProjectName")!;
        TextBox ignoreBuildBranch = control.FindControl<TextBox>("IgnoreBuildBranch")!;

        ITranslation translation = Substitute.For<ITranslation>();
        control.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(
            nameof(JenkinsSettingsUserControl), "lblJenkinsServerUrl", "Text", "Jenkins server URL");
        translation.Received(1).AddTranslationItem(
            nameof(JenkinsSettingsUserControl), "lblProjectName", "Text", "Project name");
        translation.Received(1).AddTranslationItem(
            nameof(JenkinsSettingsUserControl), "lblIgnoreBuildBranch", "Text", "Ignore build for branch");

        TestSettingsSource settings = new();
        control.Initialize("default-project", []);
        control.LoadSettings(settings);
        serverUrl.Text.Should().BeNullOrEmpty();
        projectName.Text.Should().Be("default-project");
        ignoreBuildBranch.Text.Should().BeNullOrEmpty();

        settings.SetString("BuildServerUrl", "https://jenkins.example.test");
        settings.SetString("ProjectName", "configured-project");
        settings.SetString("IgnoreBuildBranch", "dependabot/*");
        control.LoadSettings(settings);
        serverUrl.Text.Should().Be("https://jenkins.example.test");
        projectName.Text.Should().Be("configured-project");
        ignoreBuildBranch.Text.Should().Be("dependabot/*");

        serverUrl.Text = "https://saved.example.test";
        projectName.Text = "saved-project";
        ignoreBuildBranch.Text = string.Empty;
        control.SaveSettings(settings);
        settings.GetString("BuildServerUrl", null).Should().Be("https://saved.example.test");
        settings.GetString("ProjectName", null).Should().Be("saved-project");
        settings.GetString("IgnoreBuildBranch", null).Should().BeNull();
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
            GetColor(cell.Foreground).Should().Be(AdaptExpected(Colors.DarkRed));
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
    public void Build_status_column_should_use_the_original_selected_and_unselected_status_palette()
    {
        BuildStatusColumnProvider provider = new(_ => { });
        BuildStatusColumnProvider.BuildStatusTextBlock cell =
            provider.CreateCell().Should().BeOfType<BuildStatusColumnProvider.BuildStatusTextBlock>().Subject;
        (BuildStatus Status, Color Normal, Color Selected)[] cases =
        [
            (BuildStatus.Success, Colors.DarkGreen, Colors.LightGreen),
            (BuildStatus.Failure, Colors.DarkRed, Colors.Red),
            (BuildStatus.InProgress, Colors.Blue, Color.FromRgb(130, 180, 240)),
            (BuildStatus.Unstable, Colors.OrangeRed, Colors.OrangeRed),
            (BuildStatus.Stopped, Colors.Gray, Colors.LightGray),
        ];

        foreach ((BuildStatus status, Color normal, Color selected) in cases)
        {
            cell.SelectedForTest = false;
            cell.Status = status;
            GetColor(cell.Foreground).Should().Be(AdaptExpected(normal));

            cell.SelectedForTest = true;
            GetColor(cell.Foreground).Should().Be(AdaptExpected(selected));
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

    private sealed class StubClipboard(string text) : WinFormsShims.IClipboard
    {
        public bool ContainsText() => text.Length > 0;

        public string GetText() => text;

        public void SetText(string value)
        {
        }
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return WinFormsShims.DialogResult.OK;
        }
    }

    private sealed class LoopbackHttpServer : System.IAsyncDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly Dictionary<string, string> _responses = [];
        private readonly Task _serverTask;

        public LoopbackHttpServer()
        {
            _listener.Start();
            BaseUrl = $"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}";
            _serverTask = Task.Run(RunAsync);
        }

        public string BaseUrl { get; }

        public void AddResponse(string path, string content)
            => _responses[path] = content;

        public async ValueTask DisposeAsync()
        {
            await _cancellationTokenSource.CancelAsync();
            _listener.Stop();
#pragma warning disable VSTHRD003 // The owned server task is canceled and awaited during fixture disposal.
            await _serverTask;
#pragma warning restore VSTHRD003
            _cancellationTokenSource.Dispose();
        }

        private async Task RunAsync()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    using TcpClient client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                    await RespondAsync(client, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException) when (_cancellationTokenSource.IsCancellationRequested)
            {
            }
        }

        private async Task RespondAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new(stream, Encoding.ASCII, leaveOpen: true);
            string requestLine = await reader.ReadLineAsync(cancellationToken) ?? string.Empty;
            string? header;
            do
            {
                header = await reader.ReadLineAsync(cancellationToken);
            }
            while (!string.IsNullOrEmpty(header));

            string path = requestLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1) ?? "/";
            bool found = _responses.TryGetValue(path, out string? content);
            content ??= "<error>Not found</error>";
            byte[] body = Encoding.UTF8.GetBytes(content);
            byte[] responseHeader = Encoding.ASCII.GetBytes(
                $"HTTP/1.1 {(found ? "200 OK" : "404 Not Found")}\r\n" +
                "Content-Type: application/xml\r\n" +
                $"Content-Length: {body.Length}\r\n" +
                "Connection: close\r\n\r\n");
            await stream.WriteAsync(responseHeader, cancellationToken);
            await stream.WriteAsync(body, cancellationToken);
        }
    }

    private static Color GetColor(IBrush? brush)
        => brush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;

    private static Color AdaptExpected(Color color)
    {
        System.Drawing.Color original = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        System.Drawing.Color background = ThemeModule.Settings.Theme.GetColor(AppColor.PanelBackground);
        if (background.IsEmpty)
        {
            background = ThemeModule.Settings.InvariantTheme.GetColor(AppColor.PanelBackground);
        }

        System.Drawing.Color adapted = original.AdaptForeColor(background);
        return Color.FromArgb(adapted.A, adapted.R, adapted.G, adapted.B);
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
