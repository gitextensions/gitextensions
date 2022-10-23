using CommonTestUtils;
using FluentAssertions;
using GitCommands;
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
            _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
        }

        private void RebasePrep(string branch = "RebaseTest")
        {
            // master commits
            string content = $"{Guid.NewGuid():N}";
            string from = _referenceRepository.CreateCommit("I", content);
            _referenceRepository.CreateCommit("A", $"{content}{Guid.NewGuid():N}");

            _referenceRepository.CreateBranch(branch, from, true);
            _referenceRepository.CheckoutBranch(branch);

            // {branch} commits
            for (int i = 0; i <= 3; i++)
            {
                _referenceRepository.CreateCommit($"{i}", $"{Guid.NewGuid():N}", $"{i}.txt");
            }
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

        [Test]
        public void StartRebaseDialog_GpgPanel_Visible()
        {
            RunFormTest(
               form =>
               {
                   var accessor = form.GetTestAccessor();
                   accessor.flpnlGPG.Visible.Should().BeTrue();
               },
               from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        [Test]
        public void startRebaseImmediately_GpgPanel_NotVisible()
        {
            RebasePrep();
            // _referenceRepository.Module.RunGitK(); // Before Rebase
            RunFormTest(
              form =>
              {
                  var accessor = form.GetTestAccessor();
                  accessor.flpnlGPG.Visible.Should().BeFalse();
              },
              from: "", to: null, onto: "master", interactive: false, startRebaseImmediately: true);
            // _referenceRepository.Module.RunGitK(); // After rebase
        }

        [Test(Description = "Used to verify appveyor build has a gpg key.  This test will fail if you run locally and have no keys ")]
        public void StartRebaseDialog_HasGpgKeys()
        {
            RunFormTest(
               form =>
               {
                   var accessor = form.GetTestAccessor();
                   var keys = accessor.cboGpgSecretKeys.CurrentKeys
                        .Skip(1) // Ignore the "No key" selected entry
                        .ToList();
                   foreach (var k in keys)
                   {
                       Console.WriteLine(k.Caption);
                   }

                   keys.Any().Should().BeTrue();
               },
               from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        [Test]
        public void GpgAction_Not_Unselected()
        {
            RunFormTest(
               form =>
               {
                   var accessor = form.GetTestAccessor();
                   accessor.cboGpgAction.SelectedIndex.Should().NotBe(-1);
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
                    if (startRebaseImmediately)
                    {
                        AppSettings.CloseProcessDialog = true; // Prevent modal process dialog from blocking test run.
                    }

                    _commands.StartRebaseDialog(owner: null, from, to, onto, interactive, startRebaseImmediately);
                },
                testDriverAsync);
        }
    }
}
