using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Tag;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests_Git
{
    partial class CommandsTests
    {
        private const string TagName = "bla";
        private static readonly ObjectId Revision = ObjectId.Parse("0123456789012345678901234567890123456789");
        private const string TagMessage = "foo";
        private const string KeyId = "A9876F";
        private const string TagMessageFile = "c:/.git/TAGMESSAGE";

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Validate_should_throw_if_tag_name_invalid(string tagName)
        {
            GitCreateTagArgs args = new(tagName, Revision);
            Assert.Throws<ArgumentException>(() => Commands.CreateTag(args, TagMessageFile, PathUtil.ToPosixPath));
        }

        [Test]
        public void Validate_should_throw_if_tag_revision_invalid()
        {
            GitCreateTagArgs args = new(TagName, null);
            Assert.Throws<ArgumentException>(() => Commands.CreateTag(args, TagMessageFile, PathUtil.ToPosixPath));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Validate_should_throw_for_SignWithSpecificKey_if_tag_keyId_invalid(string signKeyId)
        {
            GitCreateTagArgs args = new(TagName, Revision, TagOperation.SignWithSpecificKey, signKeyId: signKeyId);
            Assert.Throws<ArgumentException>(() => Commands.CreateTag(args, TagMessageFile, PathUtil.ToPosixPath));
        }

        [Test]
        public void ToLine_should_throw_if_operation_not_supported()
        {
            GitCreateTagArgs args = new(TagName, Revision, (TagOperation)10);
            Assert.Throws<NotSupportedException>(() => Commands.CreateTag(args, TagMessageFile, PathUtil.ToPosixPath));
        }

        [TestCase(true, "tag -f -s -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        [TestCase(false, "tag -s -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        public void ToLine_should_render_force_flag(bool force, string expected)
        {
            GitCreateTagArgs args = new(TagName, Revision, TagOperation.SignWithDefaultKey, TagMessage, KeyId, force);
            IGitCommand cmd = Commands.CreateTag(args, TagMessageFile, PathUtil.ToPosixPath);

            string cmdLine = cmd.Arguments;

            Assert.AreEqual(expected, cmdLine);
        }

        [TestCase(TagOperation.Lightweight, "tag -f \"bla\" -- 0123456789012345678901234567890123456789")]
        [TestCase(TagOperation.Annotate, "tag -f -a -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        [TestCase(TagOperation.SignWithDefaultKey, "tag -f -s -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        [TestCase(TagOperation.SignWithSpecificKey, "tag -f -u A9876F -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        public void ToLine_should_render_different_operations(TagOperation operation, string expected)
        {
            GitCreateTagArgs args = new(TagName, Revision, operation, signKeyId: KeyId, force: true);
            IGitCommand cmd = Commands.CreateTag(args, TagMessageFile, PathUtil.ToPosixPath);

            string actualCmdLine = cmd.Arguments;

            Assert.AreEqual(expected, actualCmdLine);
        }
    }
}
