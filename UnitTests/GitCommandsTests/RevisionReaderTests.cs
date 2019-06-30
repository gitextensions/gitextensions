using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class RevisionReaderTests
    {
        private GitModule _gitModule;
        private MockExecutable _executable;
        private bool _showReflogReferences;
        private RevisionReader _revisionReader;

        [SetUp]
        public void Setup()
        {
            _showReflogReferences = AppSettings.ShowReflogReferences;
            _revisionReader = new RevisionReader();

            _executable = new MockExecutable();

            _gitModule = _executable.GetGitModule();
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.ShowReflogReferences = _showReflogReferences;
        }

        [Test]
        public void BuildArguments_should_be_NUL_terminated()
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(RefFilterOptions.All, "", "", "");

            args.ToString().Should().Contain(" log -z ");
        }

        [TestCase(RefFilterOptions.FirstParent, false, false)]
        [TestCase(RefFilterOptions.FirstParent, true, false)]
        [TestCase(RefFilterOptions.All, false, false)]
        [TestCase(RefFilterOptions.All, true, true)]
        public void BuildArguments_should_add_reflog_if_requested(RefFilterOptions refFilterOptions, bool reflog, bool expected)
        {
            AppSettings.ShowReflogReferences = reflog;

            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(refFilterOptions, "", "", "");

            if (expected && reflog)
            {
                args.ToString().Should().Contain(" --reflog ");
            }
            else
            {
                args.ToString().Should().NotContain(" --reflog ");
            }
        }

        /* first 'parent first' */
        [TestCase(RefFilterOptions.FirstParent, " --first-parent ", null)]
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.NoMerges, " --first-parent ", null)]
        [TestCase(RefFilterOptions.All, null, " --first-parent ")]
        /* if not 'first parent', then 'all' */
        [TestCase(RefFilterOptions.FirstParent, null, " --all ")]
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.All, null, " --all ")]
        [TestCase(RefFilterOptions.All, " --all ", null)]
        [TestCase(RefFilterOptions.Branches | RefFilterOptions.All, " --all ", null)]
        /* if not 'first parent' and not 'all' - selected branches, if requested */
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.Remotes, " --first-parent ", " --branches=")]
        [TestCase(RefFilterOptions.All | RefFilterOptions.Remotes, " --all ", " --branches=")]
        [TestCase(RefFilterOptions.Branches, " --branches=", null)]
        /* if not 'first parent' and not 'all' - *ALL* remotes, if requested */
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.Remotes, " --first-parent ", " --remotes ")]
        [TestCase(RefFilterOptions.All | RefFilterOptions.Remotes, " --all ", " --remotes ")]
        [TestCase(RefFilterOptions.Remotes, " --remotes ", null)]
        /* if not 'first parent' and not 'all' - *ALL* tags, if requested */
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.Tags, " --first-parent ", " --tags ")]
        [TestCase(RefFilterOptions.All | RefFilterOptions.Tags, " --all ", " --tags ")]
        [TestCase(RefFilterOptions.Tags, " --tags ", null)]
        public void BuildArguments_check_parameters(RefFilterOptions refFilterOptions, string expectedToContain, string notExpectedToContain)
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(refFilterOptions, "my_*", "my_revision", "my_path");

            if (expectedToContain != null)
            {
                args.ToString().Should().Contain(expectedToContain);
            }

            if (notExpectedToContain != null)
            {
                args.ToString().Should().NotContain(notExpectedToContain);
            }
        }

        [Test]
        public void RunAndParseGitLogCommand_should_not_fail_with_ArgumentException()
        {
            var command = "-c log.showSignature=false log -z --pretty=format:\"%H%T%P%n%at%n%ct%n%e%n%aN%n%aE%n%cN%n%cE%n%s%n%n%b\" --reflog --all --boundary --not --glob=notes --not --max-count=100000 --full-history  --name-only --parents --find-renames --find-copies -- \"Support\\Build\\Build.ps1\" \"Support/Build/Build.ps1\" \"Support\"";
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData/xpand.log");
            var output = File.ReadAllText(path);
            var grefs = new List<IGitRef>().ToLookup(head => head.ObjectId);
            var revisions = new Subject<GitRevision>();

            var token = new CancellationToken();
            using (_executable.StageOutput(command, output))
            {
                // throws
                // System.AggregateException: One or more errors occurred.
                // ---> System.ArgumentException: Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.

                _revisionReader.GetTestAccessor().RunAndParseGitLogCommand(_gitModule, command, token, Encoding.UTF8, null, grefs, revisions);
            }
        }
    }
}