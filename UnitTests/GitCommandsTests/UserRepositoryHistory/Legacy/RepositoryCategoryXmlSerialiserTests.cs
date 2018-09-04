using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using ApprovalTests;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.UserRepositoryHistory.Legacy;
using NUnit.Framework;
using Current = GitCommands.UserRepositoryHistory;

namespace GitCommandsTests.UserRepositoryHistory.Legacy
{
    [TestFixture]
    public class RepositoryCategorySerialiserTests
    {
        private RepositoryCategoryXmlSerialiser _repositoryXmlSerialiser;

        [SetUp]
        public void Setup()
        {
            _repositoryXmlSerialiser = new RepositoryCategoryXmlSerialiser();
        }

        [Test]
        public void Deserialize_should_throw_if_null()
        {
            ((Action)(() => _repositoryXmlSerialiser.Deserialize(null))).Should().Throw<ArgumentException>();
        }

        [Test]
        public void Deserialize_should_load_legacy_categorised_repositories()
        {
            var xml = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.CategorisedRepositories01.xml");
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new FileFormatException("Unexpected data");
            }

            var categorisedHistory = _repositoryXmlSerialiser.Deserialize(xml);

            categorisedHistory.Count.Should().Be(2);
            categorisedHistory[0].Description.Should().Be("3rd Party");
            categorisedHistory[0].CategoryType.Should().Be("Repositories");
            categorisedHistory[0].Repositories.Count.Should().Be(2);
            categorisedHistory[0].Repositories[0].Description.Should().Be("Check it out!");
            categorisedHistory[0].Repositories[0].Path.Should().Be("C:\\Development\\RibbonWinForms\\");
            categorisedHistory[0].Repositories[0].Anchor.Should().Be("None");
            categorisedHistory[0].Repositories[1].Path.Should().Be("");
            categorisedHistory[0].Repositories[1].Anchor.Should().Be("None");
            categorisedHistory[1].Description.Should().Be("Test");
            categorisedHistory[1].CategoryType.Should().Be("Repositories");
            categorisedHistory[1].Repositories.Count.Should().Be(1);
            categorisedHistory[1].Repositories[0].Title.Should().Be("Git Extensions");
            categorisedHistory[1].Repositories[0].Description.Should().Be("Mega project!");
            categorisedHistory[1].Repositories[0].Path.Should().Be("C:\\Development\\gitextensions\\");
            categorisedHistory[1].Repositories[0].Anchor.Should().Be("MostRecent");
        }

        [Test]
        public void Serialize_must_fail_as_not_supported()
        {
            ((Action)(() => ((Current.IRepositorySerialiser<RepositoryCategory>)_repositoryXmlSerialiser).Serialize(null))).Should().Throw<NotSupportedException>();
        }

        [Test]
        public void Verify_backwards_compatibility_of_object_graph()
        {
            var surrogate = new List<RepositoryCategory>
            {
                new RepositoryCategory
                {
                    Repositories = new List<Repository>(
                        new[]
                        {
                            new Repository { Path = "C:\\Development\\RibbonWinForms\\", Description = "Check it out!", Anchor = "None" },
                            new Repository { Path = "", Anchor = "None" },
                        }),
                    CategoryType = "Repositories",
                    Description = "3rd Party"
                },
                new RepositoryCategory
                {
                    Repositories = new List<Repository>(
                        new[]
                        {
                            new Repository { Title = "Git Extensions", Path = "C:\\Development\\gitextensions\\", Description = "Mega project!", Anchor = "MostRecent" }
                        }),
                    CategoryType = "Repositories",
                    Description = "Test"
                },
            };

            string xml;
            using (var sw = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(List<RepositoryCategory>));
                var ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                serializer.Serialize(sw, surrogate, ns);
                xml = sw.ToString();
            }

            Approvals.VerifyXml(xml);
        }
    }
}