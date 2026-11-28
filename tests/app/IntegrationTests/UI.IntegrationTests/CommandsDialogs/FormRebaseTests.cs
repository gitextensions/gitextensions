using CommonTestUtils;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormRebaseTests
{
    private ReferenceRepository _referenceRepository = null!;
    private GitUICommands _commands = null!;

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();
        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
    {
        _referenceRepository.Dispose();
    }

    [Test]
    public void Interactive_check_enables_autosquash()
    {
        RunFormTest(
            form =>
            {
                FormRebase.TestAccessor accessor = form.GetTestAccessor();

                accessor.chkInteractive.Checked = true;

                accessor.chkAutosquash.Enabled.Should().BeTrue();
            },
            from: "", to: null!, onto: null!, interactive: false, startRebaseImmediately: false);
    }

    [Test]
    public void Interactive_uncheck_disables_autosquash()
    {
        RunFormTest(
            form =>
            {
                FormRebase.TestAccessor accessor = form.GetTestAccessor();
                accessor.chkInteractive.Checked = true;

                accessor.chkInteractive.Checked = false;

                accessor.chkAutosquash.Enabled.Should().BeFalse();
            },
            from: "", to: null!, onto: null!, interactive: false, startRebaseImmediately: false);
    }

    [Test]
    public void Ignore_date_check_disables_only_the_other_date_option()
    {
        RunFormTest(
            form =>
            {
                FormRebase.TestAccessor accessor = form.GetTestAccessor();
                accessor.chkInteractive.Checked = true;

                accessor.chkIgnoreDate.Checked = true;

                accessor.chkInteractive.Enabled.Should().BeTrue();
                accessor.chkPreserveMerges.Enabled.Should().BeTrue();
                accessor.chkAutosquash.Enabled.Should().BeTrue();
                accessor.chkCommitterDateIsAuthorDate.Enabled.Should().BeFalse();
            },
            from: "", to: null!, onto: null!, interactive: false, startRebaseImmediately: false);
    }

    [Test]
    public void Ignore_date_uncheck_enables_the_other_date_option()
    {
        RunFormTest(
            form =>
            {
                FormRebase.TestAccessor accessor = form.GetTestAccessor();
                accessor.chkIgnoreDate.Checked = true;

                accessor.chkIgnoreDate.Checked = false;

                accessor.chkCommitterDateIsAuthorDate.Enabled.Should().BeTrue();
            },
            from: "", to: null!, onto: null!, interactive: false, startRebaseImmediately: false);
    }

    [Test]
    public void Ignore_date_check_keeps_autosquash_disabled_if_interactive_not_checked()
    {
        RunFormTest(
            form =>
            {
                FormRebase.TestAccessor accessor = form.GetTestAccessor();

                accessor.chkIgnoreDate.Checked = true;

                accessor.chkInteractive.Enabled.Should().BeTrue();
                accessor.chkAutosquash.Enabled.Should().BeFalse();
            },
            from: "", to: null!, onto: null!, interactive: false, startRebaseImmediately: false);
    }

    [Test]
    public void Committer_date_check_disables_only_the_other_date_option()
    {
        RunFormTest(
            form =>
            {
                FormRebase.TestAccessor accessor = form.GetTestAccessor();
                accessor.chkInteractive.Checked = true;

                accessor.chkCommitterDateIsAuthorDate.Checked = true;

                accessor.chkInteractive.Enabled.Should().BeTrue();
                accessor.chkPreserveMerges.Enabled.Should().BeTrue();
                accessor.chkAutosquash.Enabled.Should().BeTrue();
                accessor.chkIgnoreDate.Enabled.Should().BeFalse();
            },
            from: "", to: null!, onto: null!, interactive: false, startRebaseImmediately: false);
    }

    [Test]
    public void Committer_date_uncheck_enables_the_other_date_option()
    {
        RunFormTest(
            form =>
            {
                FormRebase.TestAccessor accessor = form.GetTestAccessor();
                accessor.chkCommitterDateIsAuthorDate.Checked = true;

                accessor.chkCommitterDateIsAuthorDate.Checked = false;

                accessor.chkIgnoreDate.Enabled.Should().BeTrue();
            },
            from: "", to: null!, onto: null!, interactive: false, startRebaseImmediately: false);
    }

    [Test]
    public void Committer_date_check_keeps_autosquash_disabled_if_interactive_not_checked()
    {
        RunFormTest(
            form =>
            {
                FormRebase.TestAccessor accessor = form.GetTestAccessor();

                accessor.chkCommitterDateIsAuthorDate.Checked = true;

                accessor.chkInteractive.Enabled.Should().BeTrue();
                accessor.chkAutosquash.Enabled.Should().BeFalse();
            },
            from: "", to: null!, onto: null!, interactive: false, startRebaseImmediately: false);
    }

    private void RunFormTest(Action<FormRebase> testDriver, string from, string? to, string? onto,
        bool interactive, bool startRebaseImmediately)
    {
        RunFormTest(
            form =>
            {
                testDriver(form);
                return Task.CompletedTask;
            },
            from, to, onto, interactive, startRebaseImmediately);
    }

    private void RunFormTest(Func<FormRebase, Task> testDriverAsync, string from, string? to,
        string? onto, bool interactive, bool startRebaseImmediately)
    {
        UITest.RunForm(
            () =>
            {
                _commands.StartRebaseDialog(owner: null, from, to, onto, interactive, startRebaseImmediately);
            },
            testDriverAsync);
    }
}
