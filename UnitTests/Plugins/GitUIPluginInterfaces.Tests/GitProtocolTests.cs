using FluentAssertions;
using GitUIPluginInterfaces.RepositoryHosts;
using NUnit.Framework;

namespace GitUIPluginInterfacesTests
{
    [TestFixture]
    public class GitProtocolTests
    {
        [TestCase("https://github.com/gitextensions/gitextensions.git", true)]
        [TestCase("htTps://github.com/gitextensions/gitextensions.git", true)]
        [TestCase("HTTPS://github.com/gitextensions/gitextensions.git", true)]
        [TestCase("http://github.com/gitextensions/gitextensions.git", true)]
        [TestCase("https_github.com/gitextensions/gitextensions.git", false)]
        [TestCase("ssh://git@github.com/gitextensions/gitextensions.git", false)]
        [TestCase("git@github.com/gitextensions/gitextensions.git", false)]
        [TestCase("github.com/gitextensions/gitextensions.git", false)]
        public void IsUrlUsingHttp(string url, bool expected)
        {
            url.IsUrlUsingHttp().Should().Be(expected);
        }
    }
}