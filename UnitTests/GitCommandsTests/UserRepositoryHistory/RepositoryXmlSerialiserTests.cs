using System;
using System.Collections.Generic;
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
        public void Deserialize_remote_repositories_with_ns()
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
        public void Deserialize_remote_repositories_without_ns()
        {
            const string settingHistoryValue = @"<?xml version=""1.0"" encoding=""utf-16""?>
<RepositoryHistory>
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
            var history = new List<Repository>
            {
                new Repository(@"C:\Development\gitextensions\"),
                new Repository(@"C:\Development\gitextensions\Externals\NBug\")
                {
                    Anchor = Repository.RepositoryAnchor.MostRecent,
                },
                new Repository(@"C:\Development\gitextensions\GitExtensionsDoc\")
                {
                    Anchor = Repository.RepositoryAnchor.LessRecent,
                }
            };

            var xml = _repositoryXmlSerialiser.Serialize(history);
            Approvals.VerifyXml(xml);
        }
    }
}