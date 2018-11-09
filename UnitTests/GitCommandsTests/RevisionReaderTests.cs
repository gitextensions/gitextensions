using FluentAssertions;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class RevisionReaderTests
    {
        private bool _showReflogReferences;
        private RevisionReader _revisionReader;

        [SetUp]
        public void Setup()
        {
            _showReflogReferences = AppSettings.ShowReflogReferences;
            _revisionReader = new RevisionReader();
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.ShowReflogReferences = _showReflogReferences;
        }

        [Test]
        public void BuildArguments_should_appy_first_parent_correct()
        {
            RefFilterOptions refFilterOptions = RefFilterOptions.FirstParent;

            string arguments = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(refFilterOptions, string.Empty, string.Empty, string.Empty).ToString();

            // Check arguments that I expect
            Assert.IsTrue(arguments.Contains("--first-parent"));

            // Check for combinations that cannot be used together
            Assert.IsFalse(arguments.Contains("--first-parent") && arguments.Contains("--all")); // first-parent contradicts --all (it does work together, but makes no sense)
            Assert.IsFalse(arguments.Contains("--first-parent") && arguments.Contains("--reflog")); // first-parent works with reflog, but breaks the graph. Also, it doesn't make sense
            Assert.IsFalse(arguments.Contains("--first-parent") && arguments.Contains("--no-merges"));
            Assert.IsFalse(arguments.Contains("--first-parent") && arguments.Contains("--boundary")); // Having this and --first-parent gives strange result.
            Assert.IsFalse(arguments.Contains("--first-parent") && arguments.Contains("--not --glob=notes --not")); // Not sure why, but first-parent is not compatible with this
            Assert.IsFalse(arguments.Contains("--first-parent") && arguments.Contains("--simplify-by-decoration")); // first-parent is more restrictive, it should win
            Assert.IsFalse(arguments.Contains("--all") && arguments.Contains("--branches")); // Show all branches and filter them, doesn't make sense
        }

        [Test]
        public void BuildArguments_should_be_NUL_terminated()
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(RefFilterOptions.All, "", "", "");

            args.ToString().Should().Contain(" log -z ");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BuildArguments_should_add_reflog_if_requested(bool reflog)
        {
            AppSettings.ShowReflogReferences = reflog;

            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(RefFilterOptions.All, "", "", "");

            if (reflog)
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
    }
}