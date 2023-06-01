using CommonTestUtils;
using FluentAssertions;
using GitCommands.Git;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormPushTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        // Note: the DataBindings between ForcePushTags and ForcePushBranches or ckForceWithLease (depending on Git version) do not function in this test environment
        [TestCase(false, false, false, ForcePushOptions.DoNotForce)]
        [TestCase(false, true, false, ForcePushOptions.Force)]
        [TestCase(false, false, true, ForcePushOptions.Force)] // ForcePushTag requires normal force as with-lease is not allowed for tags
        [TestCase(false, true, true, ForcePushOptions.Force)]
        [TestCase(true, false, false, ForcePushOptions.ForceWithLease)] // would be ForcePushOptions.DoNotForce if DataBindings were working
        [TestCase(true, true, false, ForcePushOptions.Force)]
        [TestCase(true, false, true, ForcePushOptions.Force)] // ForcePushBranches and ForcePushTags take precedence over ckForceWithLease
        [TestCase(true, true, true, ForcePushOptions.Force)] // ForcePushBranches and ForcePushTags take precedence over ckForceWithLease
        public void Should_choose_correct_force_push_option_for_checkbox_state(
            bool forcePushBranchWithLeaseChecked, bool forcePushBranchChecked, bool forcePushTagChecked, ForcePushOptions forcePushOption)
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.ForcePushTags.Checked = forcePushTagChecked;
                    accessor.ckForceWithLease.Checked = forcePushBranchWithLeaseChecked;
                    accessor.ForcePushBranches.Checked = forcePushBranchChecked;

                    accessor.GetForcePushOption().Should().Be(forcePushOption);
                });
        }

        private void RunFormTest(Action<FormPush> testDriver)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                });
        }

        private void RunFormTest(Func<FormPush, Task> testDriverAsync)
        {
            UITest.RunForm(
                () =>
                {
                    // False because we haven't performed any actions
                    Assert.False(_commands.StartPushDialog(owner: null, pushOnShow: false, forceWithLease: false, out _));
                },
                testDriverAsync);
        }
    }
}
