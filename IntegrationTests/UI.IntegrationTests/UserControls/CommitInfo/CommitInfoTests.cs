using System.Reflection;
using CommonTestUtils;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitExtensions.UITests.CommandsDialogs;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;
using NSubstitute;
using ResourceManager;

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
            AppSettings.ShowGitNotes = false;
            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _commands = new GitUICommands(_referenceRepository.Module);

            // mock git executable
            _gitExecutable = new MockExecutable();
            typeof(GitModule).GetField("_gitExecutable", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_commands.Module, _gitExecutable);
            GitCommandRunner cmdRunner = new(_gitExecutable, () => GitModule.SystemEncoding);
            typeof(GitModule).GetField("_gitCommandRunner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_commands.Module, cmdRunner);

            var composition = TestComposition.Empty
                .AddParts(typeof(MockLinkFactory));
            ExportProvider mefExportProvider = composition.ExportProviderFactory.CreateExportProvider();
            ManagedExtensibility.SetTestExportProvider(mefExportProvider);
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
                _gitExecutable.StageOutput(@"for-each-ref --sort=""-taggerdate"" --format=""%(refname)"" refs/tags/",
                    "refs/heads/master\nwarning: message");

                ((Action)(() => commitInfo.GetTestAccessor().GetSortedTags())).Should().Throw<RefsWarningException>();

                return Task.CompletedTask;
            });
        }

        [Test]
        public void GetSortedTags_should_split_output_if_no_warning()
        {
            RunCommitInfoTest(commitInfo =>
            {
                _gitExecutable.StageOutput(@"for-each-ref --sort=""-taggerdate"" --format=""%(refname)"" refs/tags/",
                    "refs/remotes/origin/master\nrefs/heads/master\nrefs/heads/warning"); // does not contain "warning:"

                Dictionary<string, int> expected = new()
                {
                    ["refs/remotes/origin/master"] = 0,
                    ["refs/heads/master"] = 1,
                    ["refs/heads/warning"] = 2
                };

                var refs = commitInfo.GetTestAccessor().GetSortedTags();

                refs.Count.Should().Be(3);
                refs.Should().BeEquivalentTo(expected);

                return Task.CompletedTask;
            });
        }

        [Test]
        public void GetSortedTags_should_load_ref_different_in_case()
        {
            RunCommitInfoTest(commitInfo =>
            {
                _gitExecutable.StageOutput(@"for-each-ref --sort=""-taggerdate"" --format=""%(refname)"" refs/tags/",
                    "refs/remotes/origin/master\nrefs/heads/master\nrefs/remotes/origin/bugfix/YS-38651-test-twist-changes-r100-on-s375\nrefs/remotes/origin/bugfix/ys-38651-test-twist-changes-r100-on-s375"); // case sensitive duplicates

                Dictionary<string, int> expected = new()
                {
                    ["refs/remotes/origin/master"] = 0,
                    ["refs/heads/master"] = 1,
                    ["refs/remotes/origin/bugfix/YS-38651-test-twist-changes-r100-on-s375"] = 2,
                    ["refs/remotes/origin/bugfix/ys-38651-test-twist-changes-r100-on-s375"] = 3
                };

                var refs = commitInfo.GetTestAccessor().GetSortedTags();

                refs.Count.Should().Be(4);
                refs.Should().BeEquivalentTo(expected);

                return Task.CompletedTask;
            });
        }

        [Test]
        public void GetSortedTags_should_load_ref_with_extra_spaces()
        {
            RunCommitInfoTest(commitInfo =>
            {
                _gitExecutable.StageOutput(@"for-each-ref --sort=""-taggerdate"" --format=""%(refname)"" refs/tags/",
                    "refs/remotes/origin/master\nrefs/heads/master\nrefs/tags/v3.1\nrefs/tags/v3.1 \n refs/tags/v3.1"); // have leading and trailing spaces

                Dictionary<string, int> expected = new()
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

                return Task.CompletedTask;
            });
        }

        [Test]
        public void GetSortedTags_should_remove_duplicate_refs()
        {
            RunCommitInfoTest(commitInfo =>
            {
                _gitExecutable.StageOutput(@"for-each-ref --sort=""-taggerdate"" --format=""%(refname)"" refs/tags/",
                    "refs/remotes/origin/master\nrefs/remotes/foo/duplicate\nrefs/remotes/foo/bar\nrefs/remotes/foo/duplicate\nrefs/remotes/foo/last"); // exact duplicates

                Dictionary<string, int> expected = new()
                {
                    ["refs/remotes/origin/master"] = 0,
                    ["refs/remotes/foo/duplicate"] = 1,
                    ["refs/remotes/foo/bar"] = 2,
                    ["refs/remotes/foo/last"] = 3,
                };

                var refs = commitInfo.GetTestAccessor().GetSortedTags();

                refs.Count.Should().Be(4);
                refs.Should().BeEquivalentTo(expected);

                return Task.CompletedTask;
            });
        }

        [Test]
        public void ReloadCommitInfo_should_render_links_correctly()
        {
            string hash = "a48da1aba59a65b2a7f0df7e3512817caf16819f";

            // Generate branches: branch01...branch15
            _gitExecutable.StageOutput($"branch --contains {hash}", string.Join('\n', Enumerable.Range(1, 15).Select(i => $"branch{i:00}")));

            // Generate tags: v1.0...v1.15
            _gitExecutable.StageOutput($"tag --contains {hash}", string.Join('\n', Enumerable.Range(0, 15).Select(i => $"v1.{i}")));

            _gitExecutable.StageOutput($"describe --tags --first-parent --abbrev=40 {hash}", "");

            var realCommitObjectId = ObjectId.Parse(hash);
            GitRevision revision = new(realCommitObjectId)
            {
                Author = "John Doe",
                AuthorUnixTime = DateTimeUtils.ToUnixTime(DateTime.Parse("2010-03-24 13:37:12")),
                AuthorEmail = "j.doe@some.email.dotcom",
                Subject = "fix: bugs",
                Body = "fix: bugs\r\n\r\nall bugs fixed"
            };

            RunCommitInfoTest(async (commitInfo) =>
            {
                commitInfo.SetRevisionWithChildren(revision, children: null);

                // Wait for pending operations so the Control is loaded completely before testing it
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                await Verifier.Verify(commitInfo.GetTestAccessor().RevisionInfo.Text);
            });
        }

        // Link start numbers are obtained manually.
        [TestCase("branch05", 183, "gitext://gotobranch/branch05")]
        [TestCase("v1.9", 745, "gitext://gototag/v1.9")]
        public void ReloadCommitInfo_should_extract_links_correctly(string refText, int linkStart, string expectedUri)
        {
            string hash = "a48da1aba59a65b2a7f0df7e3512817caf16819f";
            string hashInBody = "a48da1aba59a65b2a7f0df7e3512817caf16819a";
            string hashLink = $"gitext://gotocommit/{hashInBody}";

            // Generate branches: branch01...branch15
            _gitExecutable.StageOutput($"branch --contains {hash}", string.Join('\n', Enumerable.Range(1, 15).Select(i => $"branch{i:00}")));

            // Generate tags: v1.0...v1.15
            _gitExecutable.StageOutput($"tag --contains {hash}", string.Join('\n', Enumerable.Range(0, 15).Select(i => $"v1.{i}")));

            _gitExecutable.StageOutput($"describe --tags --first-parent --abbrev=40 {hash}", "");

            var realCommitObjectId = ObjectId.Parse(hash);
            GitRevision revision = new(realCommitObjectId)
            {
                Author = "John Doe",
                AuthorUnixTime = DateTimeUtils.ToUnixTime(DateTime.Parse("2010-03-24 13:37:12")),
                AuthorEmail = "j.doe@some.email.dotcom",
                Subject = "fix: bugs",
                Body = $"fix: bugs\r\n\r\nall bugs from {hashInBody} fixed"
            };

            RunCommitInfoTest(async (commitInfo) =>
            {
                if (ManagedExtensibility.GetExport<ILinkFactory>().Value is not MockLinkFactory mockLinkFactory)
                {
                    Assert.Fail($"Unexpected {typeof(ILinkFactory)} implementation.");
                    return;
                }

                object commandClickedSender = null;
                commitInfo.CommandClicked += (s, e) => commandClickedSender = s;
                commitInfo.SetRevisionWithChildren(revision, children: null);

                // Wait for pending operations so the Control is loaded completely before testing it
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                var ta = commitInfo.GetTestAccessor();

                // simulate a click on refText link
                ta.LinkClicked(ta.RevisionInfo, new(refText, linkStart, linkLength: refText.Length));
                mockLinkFactory.LastExecutedLinkUri.Should().Be(expectedUri);

                // simulate a click on hash link
                ta.LinkClicked(ta.CommitMessage, new(hashInBody, linkStart: 25, linkLength: hashInBody.Length));
                mockLinkFactory.LastExecutedLinkUri.Should().Be(hashLink);
                commandClickedSender.Should().Be(ta.CommitMessage);
            });
        }

        [Test]
        public void ReloadCommitInfo_should_handle_ShowAll_branches_correctly()
        {
            string hash = "a48da1aba59a65b2a7f0df7e3512817caf16819f";

            // Generate branches: branch01...branch15
            _gitExecutable.StageOutput($"branch --contains {hash}", string.Join('\n', Enumerable.Range(1, 15).Select(i => $"branch{i:00}")));

            // Generate tags: v1.0...v1.15
            _gitExecutable.StageOutput($"tag --contains {hash}", string.Join('\n', Enumerable.Range(0, 15).Select(i => $"v1.{i}")));

            _gitExecutable.StageOutput($"describe --tags --first-parent --abbrev=40 {hash}", "");

            var realCommitObjectId = ObjectId.Parse(hash);
            GitRevision revision = new(realCommitObjectId)
            {
                Author = "John Doe",
                AuthorUnixTime = DateTimeUtils.ToUnixTime(DateTime.Parse("2010-03-24 13:37:12")),
                AuthorEmail = "j.doe@some.email.dotcom",
                Subject = "fix: bugs",
                Body = "fix: bugs\r\n\r\nall bugs fixed"
            };

            RunCommitInfoTest(async (commitInfo) =>
            {
                if (ManagedExtensibility.GetExport<ILinkFactory>().Value is not MockLinkFactory mockLinkFactory)
                {
                    Assert.Fail($"Unexpected {typeof(ILinkFactory)} implementation.");
                    return;
                }

                commitInfo.SetRevisionWithChildren(revision, children: null);

                // Wait for pending operations so the Control is loaded completely before testing it
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                // simulate a click on refText link
                var ta = commitInfo.GetTestAccessor();
                ta.LinkClicked(ta.RevisionInfo, new("not important", linkStart: 423, linkLength: 0));
                mockLinkFactory.LastExecutedLinkUri.Should().Be("gitext://showall/branches");

                await Verifier.Verify(commitInfo.GetTestAccessor().RevisionInfo.Text);
            });
        }

        [Test]
        public void ReloadCommitInfo_should_handle_ShowAll_tags_correctly()
        {
            string hash = "a48da1aba59a65b2a7f0df7e3512817caf16819f";

            // Generate branches: branch01...branch15
            _gitExecutable.StageOutput($"branch --contains {hash}", string.Join('\n', Enumerable.Range(1, 15).Select(i => $"branch{i:00}")));

            // Generate tags: v1.0...v1.15
            _gitExecutable.StageOutput($"tag --contains {hash}", string.Join('\n', Enumerable.Range(0, 15).Select(i => $"v1.{i}")));

            _gitExecutable.StageOutput($"describe --tags --first-parent --abbrev=40 {hash}", "");

            var realCommitObjectId = ObjectId.Parse(hash);
            GitRevision revision = new(realCommitObjectId)
            {
                Author = "John Doe",
                AuthorUnixTime = DateTimeUtils.ToUnixTime(DateTime.Parse("2010-03-24 13:37:12")),
                AuthorEmail = "j.doe@some.email.dotcom",
                Subject = "fix: bugs",
                Body = "fix: bugs\r\n\r\nall bugs fixed"
            };

            RunCommitInfoTest(async (commitInfo) =>
            {
                if (ManagedExtensibility.GetExport<ILinkFactory>().Value is not MockLinkFactory mockLinkFactory)
                {
                    Assert.Fail($"Unexpected {typeof(ILinkFactory)} implementation.");
                    return;
                }

                commitInfo.SetRevisionWithChildren(revision, children: null);

                // Wait for pending operations so the Control is loaded completely before testing it
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                // simulate a click on refText link
                var ta = commitInfo.GetTestAccessor();
                ta.LinkClicked(ta.RevisionInfo, new("not important", linkStart: 774, linkLength: 0));
                mockLinkFactory.LastExecutedLinkUri.Should().Be("gitext://showall/tags");

                await Verifier.Verify(commitInfo.GetTestAccessor().RevisionInfo.Text);
            });
        }

        private void RunCommitInfoTest(Func<GitUI.CommitInfo.CommitInfo, Task> runTestAsync)
        {
            UITest.RunControl(
                createControl: form =>
                {
                    var uiCommandsSource = Substitute.For<IGitUICommandsSource>();
                    uiCommandsSource.UICommands.Returns(x => _commands);

                    // the following assignment of CommitInfo.UICommandsSource will already call this command
                    _gitExecutable.StageOutput(@"for-each-ref --sort=""-taggerdate"" --format=""%(refname)"" refs/tags/", "");

                    form.Size = new(600, 480);

                    return new GitUI.CommitInfo.CommitInfo
                    {
                        Dock = DockStyle.Fill,
                        Parent = form,
                        ShowBranchesAsLinks = true,
                        UICommandsSource = uiCommandsSource,
                    };
                },
                runTestAsync: async commitInfo =>
                {
                    // Wait for pending operations so the Control is loaded completely before testing it
                    await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                    await runTestAsync(commitInfo);
                });
        }
    }
}
