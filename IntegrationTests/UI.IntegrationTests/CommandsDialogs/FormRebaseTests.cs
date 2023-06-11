using CommonTestUtils;
using FluentAssertions;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormRebaseTests
    {
        private ReferenceRepository _referenceRepository;
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [Test]
        public void Interactive_check_enables_autosquash()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.chkInteractive.Checked = true;

                    accessor.chkAutosquash.Enabled.Should().BeTrue();
                },
                from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        [Test]
        public void Interactive_uncheck_disables_autosquash()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();
                    accessor.chkInteractive.Checked = true;

                    accessor.chkInteractive.Checked = false;

                    accessor.chkAutosquash.Enabled.Should().BeFalse();
                },
                from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        [Test]
        public void Ignore_date_check_disables_all_other_options()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.chkIgnoreDate.Checked = true;

                    accessor.chkInteractive.Enabled.Should().BeFalse();
                    accessor.chkPreserveMerges.Enabled.Should().BeFalse();
                    accessor.chkAutosquash.Enabled.Should().BeFalse();
                    accessor.chkCommitterDateIsAuthorDate.Enabled.Should().BeFalse();
                },
                from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        [Test]
        public void Ignore_date_uncheck_enables_all_options_if_interactive_checked()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();
                    accessor.chkInteractive.Checked = true;
                    accessor.chkIgnoreDate.Checked = true;

                    accessor.chkIgnoreDate.Checked = false;

                    accessor.chkInteractive.Enabled.Should().BeTrue();
                    accessor.chkPreserveMerges.Enabled.Should().BeTrue();
                    accessor.chkAutosquash.Enabled.Should().BeTrue();
                    accessor.chkCommitterDateIsAuthorDate.Enabled.Should().BeTrue();
                },
                from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        [Test]
        public void Ignore_date_uncheck_enables_all_options_but_autosquash_if_interactive_not_checked()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();
                    accessor.chkIgnoreDate.Checked = true;

                    accessor.chkIgnoreDate.Checked = false;

                    accessor.chkInteractive.Enabled.Should().BeTrue();
                    accessor.chkPreserveMerges.Enabled.Should().BeTrue();
                    accessor.chkAutosquash.Enabled.Should().BeFalse();
                    accessor.chkCommitterDateIsAuthorDate.Enabled.Should().BeTrue();
                },
                from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        [Test]
        public void Committer_date_check_disables_all_other_options()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.chkCommitterDateIsAuthorDate.Checked = true;

                    accessor.chkInteractive.Enabled.Should().BeFalse();
                    accessor.chkPreserveMerges.Enabled.Should().BeFalse();
                    accessor.chkAutosquash.Enabled.Should().BeFalse();
                    accessor.chkIgnoreDate.Enabled.Should().BeFalse();
                },
                from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        [Test]
        public void Committer_date_uncheck_enables_all_options_if_interactive_is_checked()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();
                    accessor.chkInteractive.Checked = true;
                    accessor.chkCommitterDateIsAuthorDate.Checked = true;

                    accessor.chkCommitterDateIsAuthorDate.Checked = false;

                    accessor.chkInteractive.Enabled.Should().BeTrue();
                    accessor.chkPreserveMerges.Enabled.Should().BeTrue();
                    accessor.chkAutosquash.Enabled.Should().BeTrue();
                    accessor.chkIgnoreDate.Enabled.Should().BeTrue();
                },
                from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        [Test]
        public void Committer_date_uncheck_enables_all_options_but_autosquash_if_interactive_not_checked()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();
                    accessor.chkCommitterDateIsAuthorDate.Checked = true;

                    accessor.chkCommitterDateIsAuthorDate.Checked = false;

                    accessor.chkInteractive.Enabled.Should().BeTrue();
                    accessor.chkPreserveMerges.Enabled.Should().BeTrue();
                    accessor.chkAutosquash.Enabled.Should().BeFalse();
                    accessor.chkIgnoreDate.Enabled.Should().BeTrue();
                },
                from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        private void RunFormTest(Action<FormRebase> testDriver, string from, string to, string onto,
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

        private void RunFormTest(Func<FormRebase, Task> testDriverAsync, string from, string to,
            string onto, bool interactive, bool startRebaseImmediately)
        {
            UITest.RunForm(
                () =>
                {
                    _commands.StartRebaseDialog(owner: null, from, to, onto, interactive, startRebaseImmediately);
                },
                testDriverAsync);
        }
    }
}
