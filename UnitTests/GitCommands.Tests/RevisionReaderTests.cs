using System;
using System.Text;
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

        [TestCase("subject", null, null)]
        [TestCase("subject", null, "filename")]
        [TestCase("subject", "line2\nline3", null)]
        [TestCase("subject", "line2\nline3", "filename")]
        [TestCase("", "l2", "f")]
        [TestCase("s", "l2", "f")]
        [TestCase("s", "l2", "f.ext")]
        [TestCase("s", "l2\n", "f.ext")]
        [TestCase("s", "l2\n\nl4", "f.ext")]
        [TestCase("s", "l2\n\nl4\n", "f.ext")]
        [TestCase("s", "l2\n\nl4 \n", "f.ext")]
        [TestCase("s", "l2\n\nl4 \n", "f.ext ")]
        public void ParseCommitBody_should_work(string subject, string lines, string expectedAdditionalData)
        {
            var encodedBody = new StringBuilder();
            encodedBody.Append(subject);
            if (!string.IsNullOrEmpty(lines))
            {
                encodedBody.Append('\n').Append(lines);
            }

            var expectedBody = encodedBody.ToString().TrimEnd();

            encodedBody.Append(RevisionReader.TestAccessor.EndOfBody);
            if (!string.IsNullOrEmpty(expectedAdditionalData))
            {
                encodedBody.Append(expectedAdditionalData);
            }

            var reader = RevisionReader.TestAccessor.MakeReader(encodedBody.ToString());

            var (body, additionalData) = RevisionReader.TestAccessor.ParseCommitBody(reader, subject);

            body.Should().Be(expectedBody);
            additionalData.Should().Be(expectedAdditionalData);
        }

        [TestCase("subject", "subject\n\n1DEA7CC4-FB39-450A-8DDF-762FCEA28B05", "filename")]
        [TestCase("subject", "SUBJECT\n\n1DEA7CC4-FB39-450A-8DDF-762FCEA28B05", "filename")]
        [TestCase("subject", "subject\n\n1DEA7CC4-FB39-450A-8DDF-762FCEA28B05")]
        [TestCase("subject", "subject\0\01DEA7CC4-FB39-450A-8DDF-762FCEA28B05")]
        [TestCase("subject", "SUBJECT\n\n1DEA7CC4-FB39-450A-8DDF-762FCEA28B05")]
        [TestCase("subject", "SUBJECT\n\n____________________________________")]
        [TestCase("subject", "_____________________________________________")]
        [TestCase("subject", "sub\n1DEA7CC4-FB39-450A-8DDF-762FCEA28B05\nject")]
        public void ParseCommitBody_should_return_subject_ignoring_contents(string subject, string encodedMessage, string expectedAdditionalData = null)
        {
            var reader = RevisionReader.TestAccessor.MakeReader(encodedMessage + expectedAdditionalData);

            var (body, additionalData) = RevisionReader.TestAccessor.ParseCommitBody(reader, subject);

            body.Should().BeSameAs(subject);
            additionalData.Should().Be(expectedAdditionalData);
        }

        [TestCase("subject", "")]
        [TestCase("subject", "subject")]
        [TestCase("subject", "subject\nl2")]
        public void ParseCommitBody_should_return_null_if_no_EndOfBody(string subject, string encodedBody)
        {
            using (new NoAssertContext())
            {
                var reader = RevisionReader.TestAccessor.MakeReader(encodedBody);
                RevisionReader.TestAccessor.ParseCommitBody(reader, subject).Should().Be((null, null));
            }
        }
    }
}
