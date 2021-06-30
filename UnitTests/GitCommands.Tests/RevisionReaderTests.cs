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
        public void BuildArguments_should_be_NUL_terminated()
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(RefFilterOptions.All, "", "", "");

            args.ToString().Should().Contain(" log -z ");
        }

        [TestCase(RefFilterOptions.FirstParent, false)]
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.Reflogs, false)]
        [TestCase(RefFilterOptions.All, false)]
        [TestCase(RefFilterOptions.All | RefFilterOptions.Reflogs, true)]
        public void BuildArguments_should_add_reflog_if_requested(RefFilterOptions refFilterOptions, bool expected)
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(refFilterOptions, "", "", "");

            if (expected)
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

            if (expectedToContain is not null)
            {
                args.ToString().Should().Contain(expectedToContain);
            }

            if (notExpectedToContain is not null)
            {
                args.ToString().Should().NotContain(notExpectedToContain);
            }
        }
    }
}
