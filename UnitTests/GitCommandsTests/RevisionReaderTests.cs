using FluentAssertions;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class RevisionReaderTests
    {
        private RevisionReader _revisionReader;

        [SetUp]
        public void Setup()
        {
            _revisionReader = new RevisionReader();
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
    }
}