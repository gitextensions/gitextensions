using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUI;
using NSubstitute;
using NUnit.Framework;

namespace GitExtensions.UITests.UserControls.CommitInfo
{
    [Apartment(ApartmentState.STA)]
    public class CommitInfoTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private MockExecutable _gitExecutable;
        private GitUICommands _commands;

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
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void GetSortedTags_should_throw_on_git_warning()
        {
            RunCommitInfoTest(commitInfo =>
            {
                _gitExecutable.StageOutput("for-each-ref --sort=-taggerdate --format=\"%(refname)\" refs/tags/",
                    "refs/heads/master\nwarning: message");

                ((Action)(() => commitInfo.GetTestAccessor().GetSortedTags())).Should().Throw<RefsWarningException>();
            });
        }

        [Test]
        public void GetSortedTags_should_split_output_if_no_warning()
        {
            RunCommitInfoTest(commitInfo =>
            {
                _gitExecutable.StageOutput("for-each-ref --sort=-taggerdate --format=\"%(refname)\" refs/tags/",
                    "refs/remotes/origin/master\nrefs/heads/master\nrefs/heads/warning"); // does not contain "warning:"

                var expected = new Dictionary<string, int>
                {
                    ["refs/remotes/origin/master"] = 0,
                    ["refs/heads/master"] = 1,
                    ["refs/heads/warning"] = 2
                };

                var refs = commitInfo.GetTestAccessor().GetSortedTags();

                refs.Count.Should().Be(3);
                refs.Should().BeEquivalentTo(expected);
            });
        }

        [Test]
        public void GetSortedTags_should_load_ref_different_in_case()
        {
            RunCommitInfoTest(commitInfo =>
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

                var refs = commitInfo.GetTestAccessor().GetSortedTags();

                refs.Count.Should().Be(4);
                refs.Should().BeEquivalentTo(expected);
            });
        }

        [Test]
        public void GetSortedTags_should_load_ref_with_extra_spaces()
        {
            RunCommitInfoTest(commitInfo =>
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

                var refs = commitInfo.GetTestAccessor().GetSortedTags();

                refs.Count.Should().Be(5);
                refs.Should().BeEquivalentTo(expected);
            });
        }

        [Test]
        public void GetSortedTags_should_remove_duplicate_refs()
        {
            RunCommitInfoTest(commitInfo =>
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

                var refs = commitInfo.GetTestAccessor().GetSortedTags();

                refs.Count.Should().Be(4);
                refs.Should().BeEquivalentTo(expected);
            });
        }

        private void RunCommitInfoTest(Action<GitUI.CommitInfo.CommitInfo> runTest)
        {
            UITest.RunControl(
                createControl: form =>
                {
                    var uiCommandsSource = Substitute.For<IGitUICommandsSource>();
                    uiCommandsSource.UICommands.Returns(x => _commands);

                    // the following assignment of CommitInfo.UICommandsSource will already call this command
                    _gitExecutable.StageOutput("for-each-ref --sort=-taggerdate --format=\"%(refname)\" refs/tags/", "");

                    return new GitUI.CommitInfo.CommitInfo
                    {
                        Parent = form,
                        UICommandsSource = uiCommandsSource
                    };
                },
                runTestAsync: async commitInfo =>
                {
                    // Wait for pending operations so the Control is loaded completely before testing it
                    await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                    runTest(commitInfo);
                });
        }
    }
}
