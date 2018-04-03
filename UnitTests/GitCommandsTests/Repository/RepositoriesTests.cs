using ApprovalTests;
using FluentAssertions;
using GitCommands.Repository;
using NUnit.Framework;

namespace GitCommandsTests.Repository
{
    [TestFixture]
    public class RepositoriesTests
    {
        [Test]
        public void Deserialize_recent_repositories()
        {
            const string settingHistoryValue = @"<?xml version=""1.0"" encoding=""utf-16""?>
<RepositoryHistory xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Repositories>
    <Repository>
      <Path>C:\Development\gitextensions\</Path>
      <Anchor>MostRecent</Anchor>
    </Repository>
    <Repository>
      <Path>C:\Development\RibbonWinForms\</Path>
      <Anchor>None</Anchor>
    </Repository>
    <Repository>
      <Path>C:\Development\System.IO.Abstractions\</Path>
      <Anchor>MostRecent</Anchor>
    </Repository>
    <Repository>
      <Path>C:\Development\gitextensions\Externals\Git.hub\</Path>
      <Anchor>None</Anchor>
    </Repository>
    <Repository>
      <Path>C:\Development\test - Copy\</Path>
      <Anchor>None</Anchor>
    </Repository>
    <Repository>
      <Path>C:\Development\gitextensions\Externals\conemu-inside\</Path>
      <Anchor>None</Anchor>
    </Repository>
    <Repository>
      <Path>C:\Development\gitextensions.github.io\</Path>
      <Anchor>LessRecent</Anchor>
    </Repository>
    <Repository>
      <Path>C:\Development\gitextensions\Externals\NBug\</Path>
      <Anchor>LessRecent</Anchor>
    </Repository>
    <Repository>
      <Path>C:\Development\gitextensions\GitExtensionsDoc\</Path>
      <Anchor>LessRecent</Anchor>
    </Repository>
    <Repository>
      <Path>C:\Development\test\</Path>
      <Anchor>None</Anchor>
    </Repository>
  </Repositories>
  <Description>Recent Repositories</Description>
  <CategoryType>Repositories</CategoryType>
</RepositoryHistory>";

            var history = Repositories.DeserializeHistoryFromXml(settingHistoryValue);

            history.Repositories.Count.Should().Be(10);
            history.Repositories[0].Path.Should().Be(@"C:\Development\gitextensions\");

            var s = new RepositoryXmlSerialiser();
            var h = s.Deserialize(settingHistoryValue);

            Approvals.VerifyAll(h, "path");
        }

        [Test]
        public void Deserialize_remote_repositories()
        {
            const string settingHistoryValue = @"<?xml version=""1.0"" encoding=""utf-16""?>
<RepositoryHistory xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Repositories>
    <Repository>
      <Path>https://github.com/RussKie/gitextensions.github.io.git</Path>
      <Anchor>None</Anchor>
    </Repository>
    <Repository>
      <Path>https://github.com/RussKie/gitextensions.git</Path>
      <Anchor>None</Anchor>
    </Repository>
    <Repository>
      <Path>https://github.com/gregsohl/gitextensions.git</Path>
      <Anchor>None</Anchor>
    </Repository>
    <Repository>
      <Path>https://github.com/gitextensions/gitextensions.git</Path>
      <Anchor>None</Anchor>
    </Repository>
    <Repository>
      <Path>https://github.com/EbenZhang/gitextensions.git</Path>
      <Anchor>None</Anchor>
    </Repository>
  </Repositories>
  <Description>Recent Repositories</Description>
  <CategoryType>Repositories</CategoryType>
</RepositoryHistory>";

            var history = Repositories.DeserializeHistoryFromXml(settingHistoryValue);

            history.Repositories.Count.Should().Be(5);
            history.Repositories[0].Path.Should().Be(@"https://github.com/RussKie/gitextensions.github.io.git");

            var s = new RepositoryXmlSerialiser();
            var h = s.Deserialize(settingHistoryValue);

            Approvals.VerifyAll(h, "path");
        }

        [Test]
        public void Serialize_recent_repositories()
        {
            // NB: don't add categories as the original type didn't have them
            var history = new RepositoryHistory();
            history.AddRepository(new GitCommands.Repository.Repository(@"C:\Development\gitextensions\"));
            history.AddRepository(new GitCommands.Repository.Repository(@"C:\Development\gitextensions\Externals\NBug\")
            {
                Anchor = GitCommands.Repository.Repository.RepositoryAnchor.MostRecent,
            });
            history.AddRepository(new GitCommands.Repository.Repository(@"C:\Development\gitextensions\GitExtensionsDoc\")
            {
                Anchor = GitCommands.Repository.Repository.RepositoryAnchor.LessRecent,
            });

            var xml1 = Repositories.SerializeHistoryIntoXml(history);

            // HACK: original type RepositoryHistory had a Description field set to "Recent Repositories" text
            // remove it
            xml1 = xml1.Replace("<Description>Recent Repositories</Description>\r\n</RepositoryHistory>", "</RepositoryHistory>");
            Approvals.VerifyXml(xml1);

            var s = new RepositoryXmlSerialiser();
            var xml2 = s.Serialize(history.Repositories);
            Approvals.VerifyXml(xml2);
        }

        [Test]
        public void Serialize_remote_repositories()
        {
            // NB: don't add categories as the original type didn't have them
            var history = new RepositoryHistory();
            history.AddRepository(new GitCommands.Repository.Repository(@"https://github.com/RussKie/gitextensions.git"));
            history.AddRepository(new GitCommands.Repository.Repository(@"https://github.com/gregsohl/gitextensions.git")
            {
                Anchor = GitCommands.Repository.Repository.RepositoryAnchor.MostRecent,
            });
            history.AddRepository(new GitCommands.Repository.Repository(@"https://github.com/EbenZhang/gitextensions.git")
            {
                Anchor = GitCommands.Repository.Repository.RepositoryAnchor.LessRecent,
            });

            var xml1 = Repositories.SerializeHistoryIntoXml(history);

            // HACK: original type RepositoryHistory had a Description field set to "Recent Repositories" text
            // remove it
            xml1 = xml1.Replace("<Description>Recent Repositories</Description>\r\n</RepositoryHistory>", "</RepositoryHistory>");
            Approvals.VerifyXml(xml1);

            var s = new RepositoryXmlSerialiser();
            var xml2 = s.Serialize(history.Repositories);
            Approvals.VerifyXml(xml2);
        }
    }
}