using GitCommands;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests
{
    public sealed class GitRevisionTests
    {
        [Test]
        public void Should_validate_full_sha1_correctly()
        {
            Assert.True(GitRevision.IsFullSha1Hash("0000000000000000000000000000000000000000"));
            Assert.True(GitRevision.IsFullSha1Hash("1111111111111111111111111111111111111111"));
            Assert.True(GitRevision.IsFullSha1Hash("0123456789abcdefa0123456789abcdefa012345"));

            Assert.False(GitRevision.IsFullSha1Hash("0123456789ABCDEFA0123456789ABCDEFA012345"));
            Assert.False(GitRevision.IsFullSha1Hash("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz"));
            Assert.False(GitRevision.IsFullSha1Hash("00000000000000000000000000000000000000000"));
            Assert.False(GitRevision.IsFullSha1Hash("000000000000000000000000000000000000000"));
            Assert.False(GitRevision.IsFullSha1Hash("0000000000000000000000000000000000000000 "));
            Assert.False(GitRevision.IsFullSha1Hash(" 0000000000000000000000000000000000000000"));
        }

        [TestCase(null)]
        [TestCase("toto")]
        [TestCase("toto\ntata\ntiti")]
        public void Should_have_same_content_When_no_ellipsis(string bodyContent)
        {
            var gitRevision = new GitRevision(ObjectId.Random());
            gitRevision.Body = bodyContent;
            gitRevision.HasMultiLineMessage = true;
            Assert.AreEqual(bodyContent, gitRevision.BodySummary);
        }

        [Test]
        public void Should_do_ellipsis_When_too_many_lines()
        {
            var gitRevision = new GitRevision(ObjectId.Random());
            gitRevision.Body = "line1\nline2\nline3\nline4\nline5\nline6\nline7\nline8\nline9\nline10\nline11\nline12\nline13\nline14\nline15\nline16\nline17\nline18\nline19\nline20\nline21\nline22\nline23\nline24\nline25\nline26\nline27\nline28\nline29\nline30\nline31";
            gitRevision.HasMultiLineMessage = true;
            Assert.AreEqual("line1\nline2\nline3\nline4\nline5\nline6\nline7\nline8\nline9\nline10\nline11\nline12\nline13\nline14\nline15\nline16\nline17\nline18\nline19\nline20\nline21\nline22\nline23\nline24\nline25\nline26\nline27\nline28\nline29\nline30\n[...]", gitRevision.BodySummary);
        }

        [Test]
        public void Should_do_ellipsis_When_too_long_lines()
        {
            var gitRevision = new GitRevision(ObjectId.Random());
            gitRevision.Body = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890|<-150 chars <==> too long";
            gitRevision.HasMultiLineMessage = true;
            Assert.AreEqual("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890 [...]", gitRevision.BodySummary);
        }
    }
}