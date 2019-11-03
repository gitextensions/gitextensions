using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUI;
using GitUITests.CommandsDialogs;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommitInfo
{
    [Apartment(ApartmentState.STA)]
    public class CommitInfoTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private MockExecutable _gitExecutable;
        private GitUICommands _commands;
        private GitUI.CommitInfo.CommitInfo _commitInfo;

        [SetUp]
        public void SetUp()
        {
            if (_referenceRepository == null)
            {
                _referenceRepository = new ReferenceRepository();
            }
            else
            {
                _referenceRepository.Reset();
            }

            _commands = new GitUICommands(_referenceRepository.Module);

            // mock git executable
            _gitExecutable = new MockExecutable();
            typeof(GitModule).GetField("_gitExecutable", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_commands.Module, _gitExecutable);
            var cmdRunner = new GitCommandRunner(_gitExecutable, () => GitModule.SystemEncoding);
            typeof(GitModule).GetField("_gitCommandRunner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_commands.Module, cmdRunner);

            var uiCommandsSource = Substitute.For<IGitUICommandsSource>();
            uiCommandsSource.UICommands.Returns(x => _commands);

            // the following assignment of _commitInfo.UICommandsSource will already call this command
            _gitExecutable.StageOutput("for-each-ref --sort=-taggerdate --format=\"%(refname)\" refs/tags/", "");

            _commitInfo = new GitUI.CommitInfo.CommitInfo
            {
                UICommandsSource = uiCommandsSource
            };

            // let the async call be executed before the mockup of _gitExecutable will be changed
            Thread.Sleep(100);
        }

        [TearDown]
        public void TearDown()
        {
            _commands = null;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void BranchComparer([Values(null, "current")] string currentBranch)
        {
            var expectedBranches = new List<string>
            {
                currentBranch,

                // local important
                "master",
                "master2",

                // remote important, important repos
                "remotes/origin/master",
                "remotes/upstream/master",

                // remote important, other repos
                "remotes/myrepo/master",
                "remotes/myrepo/master2",
                "remotes/other/master",
                "remotes/z_other/master",

                // local branches
                "1234_issue",
                "current/2",
                "current_2",
                "feature/1234_issue",
                "fix/master",
                "mastr",
                "repro/issue",

                // important repos
                "remotes/origin/b1",
                "remotes/origin/b2",
                "remotes/upstream/b1",
                "remotes/upstream/b2",

                // other repos
                "remotes/myrepo/b1",
                "remotes/myrepo/b2",
                "remotes/other/b1",
                "remotes/other/b2",
                "remotes/z_other/b1",
                "remotes/z_other/b2",
            };

            if (currentBranch == null)
            {
                expectedBranches.RemoveAt(0);
            }

            var branches = new List<string>(expectedBranches);

            SortAndCheckListsForEquality();

            branches.Sort();

            SortAndCheckListsForEquality();

            branches.Reverse();

            SortAndCheckListsForEquality();

            return;

            void SortAndCheckListsForEquality()
            {
                branches.Sort(new GitUI.CommitInfo.CommitInfo.BranchComparer(currentBranch));

                branches.Count.Should().Be(expectedBranches.Count);
                for (int index = 0; index < branches.Count; ++index)
                {
                    branches[index].Should().BeSameAs(expectedBranches[index]);
                }
            }
        }

        [Test]
        public void GetSortedTags_should_throw_on_git_warning()
        {
            _gitExecutable.StageOutput("for-each-ref --sort=-taggerdate --format=\"%(refname)\" refs/tags/",
                "refs/heads/master\nwarning: message");

            ((Action)(() => _commitInfo.GetTestAccessor().GetSortedTags())).Should().Throw<RefsWarningException>();
        }

        [Test]
        public void GetSortedTags_should_split_output_if_no_warning()
        {
            _gitExecutable.StageOutput("for-each-ref --sort=-taggerdate --format=\"%(refname)\" refs/tags/",
                "refs/remotes/origin/master\nrefs/heads/master\nrefs/heads/warning"); // does not contain "warning:"

            var expected = new Dictionary<string, int>
            {
                ["refs/remotes/origin/master"] = 0,
                ["refs/heads/master"] = 1,
                ["refs/heads/warning"] = 2
            };

            var refs = _commitInfo.GetTestAccessor().GetSortedTags();

            refs.Count.Should().Be(3);
            refs.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetSortedTags_should_load_ref_different_in_case()
        {
            _gitExecutable.StageOutput("for-each-ref --sort=-taggerdate --format=\"%(refname)\" refs/tags/",
                "refs/remotes/origin/master\nrefs/heads/master\nrefs/remotes/origin/bugfix/YS-38651-test-twist-changes-r100-on-s375\nrefs/remotes/origin/bugfix/ys-38651-test-twist-changes-r100-on-s375"); // case sensitive duplicates

            var expected = new Dictionary<string, int>
            {
                ["refs/remotes/origin/master"] = 0,
                ["refs/heads/master"] = 1,
                ["refs/remotes/origin/bugfix/YS-38651-test-twist-changes-r100-on-s375"] = 2,
                ["refs/remotes/origin/bugfix/ys-38651-test-twist-changes-r100-on-s375"] = 3
            };

            var refs = _commitInfo.GetTestAccessor().GetSortedTags();

            refs.Count.Should().Be(4);
            refs.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetSortedTags_should_load_ref_with_extra_spaces()
        {
            _gitExecutable.StageOutput("for-each-ref --sort=-taggerdate --format=\"%(refname)\" refs/tags/",
                "refs/remotes/origin/master\nrefs/heads/master\nrefs/tags/v3.1\nrefs/tags/v3.1 \n refs/tags/v3.1"); // have leading and trailing spaces

            var expected = new Dictionary<string, int>
            {
                ["refs/remotes/origin/master"] = 0,
                ["refs/heads/master"] = 1,
                ["refs/tags/v3.1"] = 2,
                ["refs/tags/v3.1 "] = 3,
                [" refs/tags/v3.1"] = 4
            };

            var refs = _commitInfo.GetTestAccessor().GetSortedTags();

            refs.Count.Should().Be(5);
            refs.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetSortedTags_should_remove_duplicate_refs()
        {
            _gitExecutable.StageOutput("for-each-ref --sort=-taggerdate --format=\"%(refname)\" refs/tags/",
                "refs/remotes/origin/master\nrefs/remotes/foo/duplicate\nrefs/remotes/foo/bar\nrefs/remotes/foo/duplicate\nrefs/remotes/foo/last"); // exact duplicates

            var expected = new Dictionary<string, int>
            {
                ["refs/remotes/origin/master"] = 0,
                ["refs/remotes/foo/duplicate"] = 1,
                ["refs/remotes/foo/bar"] = 2,
                ["refs/remotes/foo/last"] = 3,
            };

            var refs = _commitInfo.GetTestAccessor().GetSortedTags();

            refs.Count.Should().Be(4);
            refs.Should().BeEquivalentTo(expected);
        }
    }
}