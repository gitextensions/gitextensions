using GitCommands;
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
    }
}