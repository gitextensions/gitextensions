using System.ComponentModel.Design;
using AwesomeAssertions;
using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormRemotesTests
{
    private ReferenceRepository _referenceRepository = null!;
    private GitUICommands _commands = null!;
    private bool _originalAlwaysShowAdvOpt;
    private Font _originalFont = null!;

    [SetUp]
    public void SetUp()
    {
        _originalAlwaysShowAdvOpt = AppSettings.AlwaysShowAdvOpt;
        _originalFont = AppSettings.Font;

        _referenceRepository = new ReferenceRepository();

        ServiceContainer serviceContainer = GlobalServiceContainer.CreateDefaultMockServiceContainer();

        _commands = new GitUICommands(serviceContainer, _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.AlwaysShowAdvOpt = _originalAlwaysShowAdvOpt;
        AppSettings.Font = _originalFont;
        _referenceRepository.Dispose();
    }

    [Test]
    public void Should_display_remotes_tab_by_default()
    {
        RunFormTest(
            form =>
            {
                FormRemotes.TestAccessor accessor = form.GetTestAccessor();
                accessor.TabControl.SelectedTab.Should().NotBeNull();
                accessor.TabControl.SelectedIndex.Should().Be(0);
            });
    }

    [Test]
    public void Should_stay_on_remotes_tab_when_preselecting_local_without_remotes()
    {
        // Tab switch to "Default pull behaviors" only occurs when remotes are configured
        RunFormTest(
            form =>
            {
                FormRemotes.TestAccessor accessor = form.GetTestAccessor();
                accessor.TabControl.SelectedIndex.Should().Be(0);
            },
            preselectLocal: "master");
    }

    [Test]
    public void Should_enable_save_button_when_remote_name_is_entered()
    {
        RunFormTest(
            form =>
            {
                FormRemotes.TestAccessor accessor = form.GetTestAccessor();

                accessor.RemoteName.Text = "origin";
                accessor.Save.Enabled.Should().BeTrue();

                accessor.RemoteName.Text = string.Empty;
                accessor.Save.Enabled.Should().BeFalse();
            });
    }

    [Test]
    public void Should_disable_delete_and_toggle_buttons_when_no_remotes_configured()
    {
        RunFormTest(
            form =>
            {
                FormRemotes.TestAccessor accessor = form.GetTestAccessor();
                accessor.Delete.Enabled.Should().BeFalse();
                accessor.ToggleState.Enabled.Should().BeFalse();
            });
    }

    [Test]
    public void Should_show_empty_remote_name_when_no_remotes_configured()
    {
        RunFormTest(
            form =>
            {
                FormRemotes.TestAccessor accessor = form.GetTestAccessor();
                accessor.RemoteName.Text.Should().BeEmpty();
            });
    }

    [Test]
    public void Should_normalise_remote_prefix_on_leave()
    {
        AppSettings.AlwaysShowAdvOpt = true;

        IGitBranchNameNormaliser branchNameNormaliser = _commands.GetRequiredService<IGitBranchNameNormaliser>();
        branchNameNormaliser.Normalise("invalid branch prefix/", Arg.Any<GitBranchNameOptions>()).Returns("invalid-branch-prefix/");

        RunFormTest(
            form =>
            {
                FormRemotes.TestAccessor accessor = form.GetTestAccessor();

                accessor.RemotePrefix.Focus();
                accessor.RemotePrefix.Text = "invalid branch prefix/";
                accessor.RemoteName.Focus();

                accessor.RemotePrefix.Text.Should().Be("invalid-branch-prefix/");
            });
    }

    /// <summary>
    ///  The Windows "Text size" accessibility setting enlarges <see cref="SystemFonts.MessageBoxFont"/>,
    ///  which <see cref="AppSettings.Font"/> defaults to, without changing the DPI - so nothing scales
    ///  the sizes recorded by the designer. 12pt stands for the 134% text size of issue #13113.
    /// </summary>
    [TestCase(9F)]
    [TestCase(12F)]
    public void Should_fit_the_default_pull_behaviour_details_at_larger_text_sizes(float fontSizeInPoints)
    {
        AppSettings.Font = new Font(SystemFonts.MessageBoxFont!.FontFamily, fontSizeInPoints);

        RunFormTest(
            form =>
            {
                FormRemotes.TestAccessor accessor = form.GetTestAccessor();
                accessor.TabControl.SelectedIndex = 1;

                LayoutAssert.Report(accessor.PullBehaviourDetails);

                LayoutAssert.ChildrenDoNotOverlap(accessor.PullBehaviourDetails);
                LayoutAssert.ChildrenFitIntoContainer(accessor.PullBehaviourDetails);
                LayoutAssert.CaptionFits(accessor.SaveDefaultPushPull);
            });
    }

    private void RunFormTest(Action<FormRemotes> testDriver, string? preselectRemote = null, string? preselectLocal = null)
    {
        RunFormTest(
            form =>
            {
                testDriver(form);
                return Task.CompletedTask;
            },
            preselectRemote,
            preselectLocal);
    }

    private void RunFormTest(Func<FormRemotes, Task> testDriverAsync, string? preselectRemote = null, string? preselectLocal = null)
    {
        UITest.RunForm(
            () =>
            {
                _commands.StartRemotesDialog(owner: null, preselectRemote: preselectRemote, preselectLocal: preselectLocal);
            },
            testDriverAsync);
    }
}
