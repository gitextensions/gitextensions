using System;
using System.Threading;
using System.Threading.Tasks;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormPullTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;
        private AppSettings.PullAction _originalDefaultPullAction;
        private AppSettings.PullAction _originalFormPullAction;
        private bool _originalAutoStash;

        [SetUp]
        public void SetUp()
        {
            _originalDefaultPullAction = AppSettings.DefaultPullAction;
            _originalFormPullAction = AppSettings.FormPullAction;
            _originalAutoStash = AppSettings.AutoStash;

            if (_referenceRepository == null)
            {
                _referenceRepository = new ReferenceRepository();
            }
            else
            {
                _referenceRepository.Reset();
            }

            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.DefaultPullAction = _originalDefaultPullAction;
            AppSettings.FormPullAction = _originalFormPullAction;
            AppSettings.AutoStash = _originalAutoStash;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void Should_correctly_setup_form_title_for_merge_action()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.Merge.Checked = true;
                    var expected = string.Format(accessor.PullTitleText, PathUtil.GetDisplayPath(_referenceRepository.Module.WorkingDir));

                    accessor.Title.Should().StartWith(expected);
                },
                null, null,
                //// select an action different from Merge
                AppSettings.PullAction.FetchAll);
        }

        [Test]
        public void Should_correctly_setup_form_title_for_rebase_action()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.Rebase.Checked = true;
                    var expected = string.Format(accessor.PullTitleText, PathUtil.GetDisplayPath(_referenceRepository.Module.WorkingDir));

                    accessor.Title.Should().StartWith(expected);
                },
                null, null,
                //// select an action different from Rebase
                AppSettings.PullAction.FetchAll);
        }

        [Test]
        public void Should_correctly_setup_form_title_for_fetch_action()
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.Fetch.Checked = true;
                    var expected = string.Format(accessor.FetchTitleText, PathUtil.GetDisplayPath(_referenceRepository.Module.WorkingDir));

                    accessor.Title.Should().StartWith(expected);
                },
                null, null,
                //// select an action different from None/fetch
                AppSettings.PullAction.Merge);
        }

        [TestCase(AppSettings.PullAction.None, true, false, false, false, false, true)]
        [TestCase(AppSettings.PullAction.Merge, true, false, false, false, false, true)]
        [TestCase(AppSettings.PullAction.Rebase, false, false, true, false, false, false)]
        [TestCase(AppSettings.PullAction.Fetch, false, false, false, true, false, true)]
        [TestCase(AppSettings.PullAction.FetchAll, false, false, false, true, false, true)]
        [TestCase(AppSettings.PullAction.FetchPruneAll, false, true, false, true, false, true)]
        public void Should_correctly_setup_for_defined_pull_action(AppSettings.PullAction pullAction,
            bool mergeChecked, bool pruneChecked, bool rebaseChecked, bool fetchChecked, bool autoStashChecked, bool pruneEnabled)
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.Merge.Checked.Should().Be(mergeChecked);
                    accessor.Prune.Checked.Should().Be(pruneChecked);
                    accessor.Rebase.Checked.Should().Be(rebaseChecked);
                    accessor.Fetch.Checked.Should().Be(fetchChecked);
                    accessor.AutoStash.Checked.Should().Be(autoStashChecked);
                    accessor.Prune.Enabled.Should().Be(pruneEnabled);
                    accessor.Remotes.Text.Should().Be("[ All ]");
                },
                null, null, pullAction);
        }

        [TestCase(AppSettings.PullAction.None, true, false, false, false, false, true)]
        [TestCase(AppSettings.PullAction.Merge, true, false, false, false, false, true)]
        [TestCase(AppSettings.PullAction.Rebase, false, false, true, false, false, false)]
        [TestCase(AppSettings.PullAction.Fetch, false, false, false, true, false, true)]
        [TestCase(AppSettings.PullAction.FetchAll, false, false, false, true, false, true)]
        [TestCase(AppSettings.PullAction.FetchPruneAll, false, true, false, true, false, true)]
        public void Should_use_user_DefaultPullAction_pull_action_None(AppSettings.PullAction pullAction,
            bool mergeChecked, bool pruneChecked, bool rebaseChecked, bool fetchChecked, bool autoStashChecked, bool pruneEnabled)
        {
            AppSettings.DefaultPullAction = pullAction;

            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.Merge.Checked.Should().Be(mergeChecked);
                    accessor.Prune.Checked.Should().Be(pruneChecked);
                    accessor.Rebase.Checked.Should().Be(rebaseChecked);
                    accessor.Fetch.Checked.Should().Be(fetchChecked);
                    accessor.AutoStash.Checked.Should().Be(autoStashChecked);
                    accessor.Prune.Enabled.Should().Be(pruneEnabled);
                    accessor.Remotes.Text.Should().Be("[ All ]");
                },
                null, null, AppSettings.PullAction.None);
        }

        [TestCase(true, false, false, AppSettings.PullAction.Merge)]
        [TestCase(false, true, false, AppSettings.PullAction.Rebase)]
        [TestCase(false, false, true, AppSettings.PullAction.Fetch)]
        public void Should_remember_user_choice_upon_pull(bool mergeChecked, bool rebaseChecked, bool fetchChecked, AppSettings.PullAction expectedFormPullAction)
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.Merge.Checked = mergeChecked;
                    accessor.Rebase.Checked = rebaseChecked;
                    accessor.Fetch.Checked = fetchChecked;

                    accessor.UpdateSettingsDuringPull();

                    AppSettings.FormPullAction.Should().Be(expectedFormPullAction);
                },
                null, null,
                //// select an action different from None/fetch
                AppSettings.PullAction.Merge);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Should_remember_user_choice_upon_pull(bool autoStashChecked)
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();
                    accessor.AutoStash.Checked = autoStashChecked;

                    accessor.UpdateSettingsDuringPull();

                    AppSettings.AutoStash.Should().Be(autoStashChecked);
                },
                null, null,
                //// select an action different from None/fetch
                AppSettings.PullAction.Merge);
        }

        private void RunFormTest(Action<FormPull> testDriver, string remoteBranch, string remote, AppSettings.PullAction pullAction)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                },
                remoteBranch, remote, pullAction);
        }

        private void RunFormTest(Func<FormPull, Task> testDriverAsync, string remoteBranch, string remote, AppSettings.PullAction pullAction)
        {
            UITest.RunForm(
                () =>
                {
                    // False because we haven't performed any actions
                    Assert.False(_commands.StartPullDialog(owner: null, remoteBranch: remoteBranch, remote: remote, pullAction: pullAction));
                },
                testDriverAsync);
        }
    }
}
