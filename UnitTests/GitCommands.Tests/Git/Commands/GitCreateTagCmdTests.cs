using System;
using GitCommands.Git.Commands;
using GitCommands.Git.Tag;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests.Git.Commands
{
    [TestFixture]
    public sealed class GitCreateTagCmdTests
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
            GitCreateTagCmd cmd = new(args, TagMessageFile);

            Assert.Throws<ArgumentException>(() => cmd.Validate());
        }

        [Test]
        public void Validate_should_throw_if_tag_revision_invalid()
        {
            GitCreateTagArgs args = new(TagName, null);
            GitCreateTagCmd cmd = new(args, TagMessageFile);

            Assert.Throws<ArgumentException>(() => cmd.Validate());
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Validate_should_throw_for_SignWithSpecificKey_if_tag_keyId_invalid(string signKeyId)
        {
            GitCreateTagArgs args = new(TagName, Revision, TagOperation.SignWithSpecificKey, signKeyId: signKeyId);
            GitCreateTagCmd cmd = new(args, TagMessageFile);

            Assert.Throws<ArgumentException>(() => cmd.Validate());
        }

        [Test]
        public void ToLine_should_throw_if_operation_not_supported()
        {
            GitCreateTagArgs args = new(TagName, Revision, (TagOperation)10);
            GitCreateTagCmd cmd = new(args, TagMessageFile);

            Assert.Throws<NotSupportedException>(() => _ = cmd.Arguments);
        }

        [TestCase(true, "tag -f -s -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        [TestCase(false, "tag -s -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        public void ToLine_should_render_force_flag(bool force, string expected)
        {
            GitCreateTagArgs args = new(TagName, Revision, TagOperation.SignWithDefaultKey, TagMessage, KeyId, force);
            GitCreateTagCmd cmd = new(args, TagMessageFile);

            var cmdLine = cmd.Arguments;

            Assert.AreEqual(expected, cmdLine);
        }

        [TestCase(TagOperation.Lightweight, "tag -f \"bla\" -- 0123456789012345678901234567890123456789")]
        [TestCase(TagOperation.Annotate, "tag -f -a -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        [TestCase(TagOperation.SignWithDefaultKey, "tag -f -s -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        [TestCase(TagOperation.SignWithSpecificKey, "tag -f -u A9876F -F \"c:/.git/TAGMESSAGE\" \"bla\" -- 0123456789012345678901234567890123456789")]
        public void ToLine_should_render_different_operations(TagOperation operation, string expected)
        {
            GitCreateTagArgs args = new(TagName, Revision, operation, signKeyId: KeyId, force: true);
            GitCreateTagCmd cmd = new(args, TagMessageFile);

            var actualCmdLine = cmd.Arguments;

            Assert.AreEqual(expected, actualCmdLine);
        }
    }
}
