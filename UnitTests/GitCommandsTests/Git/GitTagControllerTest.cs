using GitCommands.Git;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;
using System;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitExtensionsTest.GitCommands.Git
{
    class GitTagControllerTest
    {
        private const string TagName = "bla";
        private const string Revision = "0123456789";
        private const string TagMessage = "foo";
        private const string KeyId = "A9876F";
        private string WorkingDir = TestContext.CurrentContext.TestDirectory;
        private IGitModule _module;
        private IGitTagController _controller;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _module.GetGitDirectory().Returns(x => WorkingDir);

            _controller = new GitTagController(_module);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Tag_sign_with_default_gpg(bool force)
        {
            var result = _controller.CreateTag(Revision, TagName, force, TagOperation.SignWithDefaultKey, TagMessage);

            _module.Received(1).RunGitCmd($"tag {(force ? "-f" : "")} -s -F \"{WorkingDir}\\TAGMESSAGE\" \"{TagName}\" -- \"{Revision}\"");
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Tag_name_null()
        {
            var result = _controller.CreateTag(Revision, null, true, TagOperation.SignWithDefaultKey, TagMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Tag_revision_null()
        {
            var result = _controller.CreateTag(null, TagName, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Tag_key_id_null()
        {
            var result = _controller.CreateTag(Revision, TagName, true, TagOperation.SignWithSpecificKey, TagMessage, null);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Tag_operation_not_supported()
        {
            var result = _controller.CreateTag(Revision, TagName, true, (TagOperation) 10, TagMessage, null);
        }

        [TestCase(TagOperation.Lightweight)]
        [TestCase(TagOperation.Annotate)]
        [TestCase(TagOperation.SignWithDefaultKey)]
        [TestCase(TagOperation.SignWithSpecificKey)]
        public void Tag_supported_operation(TagOperation operation)
        {
            var result = _controller.CreateTag(Revision, TagName, true, operation, TagMessage, KeyId);

            string switches = "";

            switch (operation)
            {
                case TagOperation.Lightweight:
                    break;
                case TagOperation.Annotate:
                    switches = $"-a -F \"{WorkingDir}\\TAGMESSAGE\"";
                    break;
                case TagOperation.SignWithDefaultKey:
                    switches = $"-s -F \"{WorkingDir}\\TAGMESSAGE\"";
                    break;
                case TagOperation.SignWithSpecificKey:
                    switches = $"-u {KeyId} -F \"{WorkingDir}\\TAGMESSAGE\"";
                    break;
            }

            _module.Received(1).RunGitCmd($"tag -f {switches} \"{TagName}\" -- \"{Revision}\"");
        }
    }
}
