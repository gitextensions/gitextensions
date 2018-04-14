using System;
using ApprovalTests;
using FluentAssertions;
using GitCommands.UserRepositoryHistory;
using NUnit.Framework;

namespace GitCommandsTests.UserRepositoryHistory
{
    [TestFixture]
    public class RepositoryXmlSerialiserTests
    {
        private RepositoryXmlSerialiser _repositoryXmlSerialiser;

        [SetUp]
        public void Setup()
        {
            _repositoryXmlSerialiser = new RepositoryXmlSerialiser();
        }

        [Test]
        public void Deserialize_should_throw_if_null()
        {
            ((Action)(() => _repositoryXmlSerialiser.Deserialize(null))).Should().Throw<ArgumentException>();
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

            var repositories = _repositoryXmlSerialiser.Deserialize(settingHistoryValue);

            Approvals.VerifyAll(repositories, "path");
        }

        [Test]
        public void Serialize_should_throw_if_null()
        {
            ((Action)(() => _repositoryXmlSerialiser.Serialize(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Serialize_recent_repositories()
        {
            var history = new RepositoryHistory();
            history.AddRepository(new Repository(@"C:\Development\gitextensions\"));
            history.AddRepository(new Repository(@"C:\Development\gitextensions\Externals\NBug\")
            {
                Anchor = Repository.RepositoryAnchor.MostRecent,
            });
            history.AddRepository(new Repository(@"C:\Development\gitextensions\GitExtensionsDoc\")
            {
                Anchor = Repository.RepositoryAnchor.LessRecent,
            });

            var xml = _repositoryXmlSerialiser.Serialize(history.Repositories);
            Approvals.VerifyXml(xml);
        }
    }
}