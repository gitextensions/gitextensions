using System;
using GitExtUtils;
using NUnit.Framework;

namespace GitExtUtilsTests
{
    [TestFixture]
    public sealed class GitArgumentBuilderTests
    {
        [Test]
        public void Command_with_one_argument()
        {
            Assert.AreEqual(
                "foo --bar",
                new GitArgumentBuilder("foo") { "--bar" }.ToString());
        }

        [Test]
        public void Command_with_no_arguments()
        {
            Assert.AreEqual(
                "foo",
                new GitArgumentBuilder("foo").ToString());
        }

        [Test]
        public void Command_with_default_config_item()
        {
            GitArgumentBuilder args = new("log")
            {
                "-n 1"
            };

            Assert.AreEqual(
                "-c log.showSignature=false log -n 1",
                args.ToString());
        }

        [Test]
        public void Command_with_custom_config_item()
        {
            GitArgumentBuilder args = new("foo")
            {
                new GitConfigItem("bar", "baz")
            };

            Assert.AreEqual(
                "-c bar=baz foo",
                args.ToString());
        }

        [Test]
        public void Command_with_custom_config_item_quoted()
        {
            GitArgumentBuilder args = new("foo")
            {
                new GitConfigItem("bar", "baz bax")
            };

            Assert.AreEqual(
                "-c bar=\"baz bax\" foo",
                args.ToString());
        }

        [Test]
        public void Command_with_custom_config_item_already_quoted()
        {
            GitArgumentBuilder args = new("foo")
            {
                new GitConfigItem("bar", "\"baz bax\"")
            };

            Assert.AreEqual(
                "-c bar=\"baz bax\" foo",
                args.ToString());
        }

        [Test]
        public void Command_with_custom_config_item_and_argument()
        {
            GitArgumentBuilder args = new("foo")
            {
                new GitConfigItem("bar", "baz"),
                "--arg"
            };

            Assert.AreEqual(
                "-c bar=baz foo --arg",
                args.ToString());
        }

        [Test]
        public void Command_with_custom_config_item_defined_after_argument()
        {
            GitArgumentBuilder args = new("foo")
            {
                "--arg",
                new GitConfigItem("bar", "baz") // order doesn't matter
            };

            Assert.AreEqual(
                "-c bar=baz foo --arg",
                args.ToString());
        }

        [Test]
        public void Command_with_default_and_custom_config_items()
        {
            GitArgumentBuilder args = new("log")
            {
                new GitConfigItem("bar", "baz"),
                "-n 1"
            };

            Assert.AreEqual(
                "-c log.showSignature=false -c bar=baz log -n 1",
                args.ToString());
        }

        [Test]
        public void Command_with_default_and_custom_config_items_and_argument()
        {
            GitArgumentBuilder args = new("log")
            {
                new GitConfigItem("bar", "baz"),
                "-n 1"
            };

            Assert.AreEqual(
                "-c log.showSignature=false -c bar=baz log -n 1",
                args.ToString());
        }

        [Test]
        public void Command_with_config_item_that_overrides_default()
        {
            GitArgumentBuilder args = new("log")
            {
                new GitConfigItem("log.showSignature", "true"),
                "-n 1"
            };

            Assert.AreEqual(
                "-c log.showSignature=true log -n 1",
                args.ToString());
        }

        [Test]
        public void Command_with_config_item_that_overrides_default_different_case()
        {
            GitArgumentBuilder args = new("log")
            {
                new GitConfigItem("LOG.showSIGNATURE", "true"),
                "-n 1"
            };

            Assert.AreEqual(
                "-c LOG.showSIGNATURE=true log -n 1",
                args.ToString());
        }

        [Test]
        public void Throws_for_invalid_command_strings()
        {
            Assert.Throws<ArgumentNullException>(() => new GitArgumentBuilder(null));
            Assert.Throws<ArgumentException>(() => new GitArgumentBuilder(""));
            Assert.Throws<ArgumentException>(() => new GitArgumentBuilder(" "));
            Assert.Throws<ArgumentException>(() => new GitArgumentBuilder(" a banana "));
            Assert.Throws<ArgumentException>(() => new GitArgumentBuilder("!£$%^&*("));
        }
    }
}
