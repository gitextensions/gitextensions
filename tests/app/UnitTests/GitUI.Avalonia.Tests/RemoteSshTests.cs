using GitUI.Infrastructure;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RemoteSshTests
{
    [Test]
    [Platform(Exclude = "Win")]
    public void StartPageantIfConfigured_should_defer_to_the_platform_SSH_agent_off_Windows()
    {
        bool keyRequested = false;

        bool started = PuttyHelpers.StartPageantIfConfigured(() =>
        {
            keyRequested = true;
            return "unused.ppk";
        });

        started.Should().BeFalse();
        keyRequested.Should().BeFalse("OpenSSH and the platform ssh-agent own portable authentication");
    }
}
