using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormPullTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;
        private GitPullAction _originalDefaultPullAction;
        private GitPullAction _originalFormPullAction;
        private bool _originalAutoStash;

        [SetUp]
        public void SetUp()
        {
            _originalDefaultPullAction = AppSettings.DefaultPullAction;
            _originalFormPullAction = AppSettings.FormPullAction;
            _originalAutoStash = AppSettings.AutoStash;

            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
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
                    FormPull.TestAccessor accessor = form.GetTestAccessor();

                    accessor.Merge.Checked = true;
                    string expected = string.Format(accessor.PullTitleText, PathUtil.GetDisplayPath(_referenceRepository.Module.WorkingDir));

                    accessor.Title.Should().StartWith(expected);
                },
                null, null,
                //// select an action different from Merge
                GitPullAction.FetchAll);
        }

        [Test]
        public void Should_correctly_setup_form_title_for_rebase_action()
        {
            RunFormTest(
                form =>
                {
                    FormPull.TestAccessor accessor = form.GetTestAccessor();

                    accessor.Rebase.Checked = true;
                    string expected = string.Format(accessor.PullTitleText, PathUtil.GetDisplayPath(_referenceRepository.Module.WorkingDir));

                    accessor.Title.Should().StartWith(expected);
                },
                null, null,
                //// select an action different from Rebase
                GitPullAction.FetchAll);
        }

        [Test]
        public void Should_correctly_setup_form_title_for_fetch_action()
        {
            RunFormTest(
                form =>
                {
                    FormPull.TestAccessor accessor = form.GetTestAccessor();

                    accessor.Fetch.Checked = true;
                    string expected = string.Format(accessor.FetchTitleText, PathUtil.GetDisplayPath(_referenceRepository.Module.WorkingDir));

                    accessor.Title.Should().StartWith(expected);
                },
                null, null,
                //// select an action different from None/fetch
                GitPullAction.Merge);
        }

        [TestCase(GitPullAction.None, true, false, false, false, false, false, false, false)]
        [TestCase(GitPullAction.Merge, true, false, false, false, false, false, false, false)]
        [TestCase(GitPullAction.Rebase, false, false, false, true, false, false, false, false)]
        [TestCase(GitPullAction.Fetch, false, false, false, false, true, false, true, true)]
        [TestCase(GitPullAction.FetchAll, false, false, false, false, true, false, true, true)]
        [TestCase(GitPullAction.FetchPruneAll, false, true, false, false, true, false, true, true)]
        public void Should_correctly_setup_for_defined_pull_action(GitPullAction pullAction,
            bool mergeChecked, bool pruneRemoteBranches, bool pruneRemoteBranchesAndTags, bool rebaseChecked, bool fetchChecked, bool autoStashChecked, bool pruneRemoteBranchesEnabled, bool pruneRemoteBranchesAndTagsEnabled)
        {
            GitPullAction defaultPullAction = AppSettings.DefaultPullAction;
            AppSettings.DefaultPullAction = GitPullAction.Merge;
            RunFormTest(
                form =>
                {
                    try
                    {
                        FormPull.TestAccessor accessor = form.GetTestAccessor();

                        accessor.Merge.Checked.Should().Be(mergeChecked);
                        accessor.Prune.Checked.Should().Be(pruneRemoteBranches);
                        accessor.PruneTags.Checked.Should().Be(pruneRemoteBranchesAndTags);
                        accessor.Rebase.Checked.Should().Be(rebaseChecked);
                        accessor.Fetch.Checked.Should().Be(fetchChecked);
                        accessor.AutoStash.Checked.Should().Be(autoStashChecked);
                        accessor.Prune.Enabled.Should().Be(pruneRemoteBranchesEnabled);
                        accessor.PruneTags.Enabled.Should().Be(pruneRemoteBranchesAndTagsEnabled);
                        accessor.Remotes.Text.Should().Be("[ All ]");
                    }
                    finally
                    {
                        AppSettings.DefaultPullAction = defaultPullAction;
                    }
                },
                null, null, pullAction);
        }

        [TestCase(GitPullAction.None, true, false, false, false, false, false, false, false)]
        [TestCase(GitPullAction.Merge, true, false, false, false, false, false, false, false)]
        [TestCase(GitPullAction.Rebase, false, false, false, true, false, false, false, false)]
        [TestCase(GitPullAction.Fetch, false, false, false, false, true, false, true, true)]
        [TestCase(GitPullAction.FetchAll, false, false, false, false, true, false, true, true)]
        [TestCase(GitPullAction.FetchPruneAll, false, true, false, false, true, false, true, true)]
        public void Should_use_user_DefaultPullAction_pull_action_None(GitPullAction pullAction,
            bool mergeChecked, bool pruneRemoteBranches, bool pruneRemoteBranchesAndTags, bool rebaseChecked, bool fetchChecked, bool autoStashChecked, bool pruneRemoteBranchesEnabled, bool pruneRemoteBranchesAndTagsEnabled)
        {
            AppSettings.DefaultPullAction = pullAction;

            RunFormTest(
                form =>
                {
                    FormPull.TestAccessor accessor = form.GetTestAccessor();

                    accessor.Merge.Checked.Should().Be(mergeChecked);
                    accessor.Prune.Checked.Should().Be(pruneRemoteBranches);
                    accessor.PruneTags.Checked.Should().Be(pruneRemoteBranchesAndTags);
                    accessor.Rebase.Checked.Should().Be(rebaseChecked);
                    accessor.Fetch.Checked.Should().Be(fetchChecked);
                    accessor.AutoStash.Checked.Should().Be(autoStashChecked);
                    accessor.Prune.Enabled.Should().Be(pruneRemoteBranchesEnabled);
                    accessor.PruneTags.Enabled.Should().Be(pruneRemoteBranchesAndTagsEnabled);
                    accessor.Remotes.Text.Should().Be("[ All ]");
                },
                null, null, GitPullAction.None);
        }

        [TestCase(true, false, false, GitPullAction.Merge)]
        [TestCase(false, true, false, GitPullAction.Rebase)]
        [TestCase(false, false, true, GitPullAction.Fetch)]
        public void Should_remember_user_choice_upon_pull(bool mergeChecked, bool rebaseChecked, bool fetchChecked, GitPullAction expectedFormPullAction)
        {
            RunFormTest(
                form =>
                {
                    FormPull.TestAccessor accessor = form.GetTestAccessor();

                    accessor.Merge.Checked = mergeChecked;
                    accessor.Rebase.Checked = rebaseChecked;
                    accessor.Fetch.Checked = fetchChecked;

                    accessor.UpdateSettingsDuringPull();

                    AppSettings.FormPullAction.Should().Be(expectedFormPullAction);
                },
                null, null,
                //// select an action different from None/fetch
                GitPullAction.Merge);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Should_remember_user_choice_upon_pull(bool autoStashChecked)
        {
            RunFormTest(
                form =>
                {
                    FormPull.TestAccessor accessor = form.GetTestAccessor();
                    accessor.AutoStash.Checked = autoStashChecked;

                    accessor.UpdateSettingsDuringPull();

                    AppSettings.AutoStash.Should().Be(autoStashChecked);
                },
                null, null,
                //// select an action different from None/fetch
                GitPullAction.Merge);
        }

        private void RunFormTest(Action<FormPull> testDriver, string remoteBranch, string remote, GitPullAction pullAction)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                },
                remoteBranch, remote, pullAction);
        }

        private void RunFormTest(Func<FormPull, Task> testDriverAsync, string remoteBranch, string remote, GitPullAction pullAction)
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
