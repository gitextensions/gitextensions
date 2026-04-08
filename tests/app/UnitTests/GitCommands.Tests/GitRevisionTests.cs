using GitUIPluginInterfaces;

namespace GitCommandsTests;

public sealed class GitRevisionTests
{
    [Test]
    public void Should_validate_full_sha1_correctly()
    {
        GitRevision.IsFullSha1Hash("0000000000000000000000000000000000000000").Should().BeTrue();
        GitRevision.IsFullSha1Hash("1111111111111111111111111111111111111111").Should().BeTrue();
        GitRevision.IsFullSha1Hash("0123456789abcdefa0123456789abcdefa012345").Should().BeTrue();

        GitRevision.IsFullSha1Hash("0123456789ABCDEFA0123456789ABCDEFA012345").Should().BeFalse();
        GitRevision.IsFullSha1Hash("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz").Should().BeFalse();
        GitRevision.IsFullSha1Hash("00000000000000000000000000000000000000000").Should().BeFalse();
        GitRevision.IsFullSha1Hash("000000000000000000000000000000000000000").Should().BeFalse();
        GitRevision.IsFullSha1Hash("0000000000000000000000000000000000000000 ").Should().BeFalse();
        GitRevision.IsFullSha1Hash(" 0000000000000000000000000000000000000000").Should().BeFalse();
    }
}
