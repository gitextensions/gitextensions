using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using ApprovalTests;
using CommonTestUtils;
using FluentAssertions;
using GitUI.Script;
using NUnit.Framework;

namespace GitUITests.Script
{
    [TestFixture]
    public class ScriptInfoSerialiserTests
    {
        private ScriptInfoSerialiser _scriptInfoSerialiser;

        [SetUp]
        public void Setup()
        {
            _scriptInfoSerialiser = new ScriptInfoSerialiser();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void Deserialise_should_return_default_scripts_if_null_or_whitespace(string xml)
        {
            var scripts = _scriptInfoSerialiser.Deserialise(xml);

            scripts.Should().BeEquivalentTo(_scriptInfoSerialiser.GetTestAccessor().GetDefaultScripts());
        }

        [Test]
        public void Deserialise_should_deserialise()
        {
            var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.OwnScripts.xml");

            var scripts = _scriptInfoSerialiser.Deserialise(content);

            using (var sw = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(List<ScriptInfo>));
                serializer.Serialize(sw, scripts);

                Approvals.VerifyXml(sw.ToString());
            }
        }

        [Test]
        public void Serialise_should_throw_if_null()
        {
            ((Action)(() => _scriptInfoSerialiser.Serialise(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Serialise_should_return_null_if_exception()
        {
            // No idea how to test...
            Assert.Inconclusive();
        }

        [Test]
        public void Serialise_should_serialise()
        {
            Approvals.VerifyXml(_scriptInfoSerialiser.Serialise(GetDefaultScripts()));
        }

        private IList<ScriptInfo> GetDefaultScripts() => new Collection<ScriptInfo>
        {
            new ScriptInfo
            {
                HotkeyCommandIdentifier = 9000,
                Name = "Fetch changes after commit",
                Command = "git",
                Arguments = "fetch",
                RunInBackground = false,
                AskConfirmation = true,
                OnEvent = ScriptEvent.AfterCommit,
                AddToRevisionGridContextMenu = false,
                Enabled = false
            },
            new ScriptInfo
            {
                HotkeyCommandIdentifier = 9001,
                Name = "Update submodules after pull",
                Command = "git",
                Arguments = "submodule update --init --recursive",
                RunInBackground = false,
                AskConfirmation = true,
                OnEvent = ScriptEvent.AfterPull,
                AddToRevisionGridContextMenu = false,
                Enabled = false
            }
        };
    }
}
