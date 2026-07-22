using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class BuildServerIntegrationTests
{
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
}
