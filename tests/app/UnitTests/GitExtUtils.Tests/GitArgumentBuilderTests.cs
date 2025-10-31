using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitExtUtilsTests;

[TestFixture]
public sealed class GitArgumentBuilderTests
{
    [Test]
    public void Command_with_one_argument()
    {
        ClassicAssert.AreEqual(
            "foo --bar",
            new GitArgumentBuilder("foo") { "--bar" }.ToString());
    }

    [Test]
    public void Command_with_no_arguments()
    {
        ClassicAssert.AreEqual(
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

        ClassicAssert.AreEqual(
            "-c log.showsignature=false log -n 1",
            args.ToString());
    }

    [Test]
    public void Command_with_custom_config_item()
    {
        GitArgumentBuilder args = new("foo")
        {
            new GitConfigItem("bar", "baz")
        };

        ClassicAssert.AreEqual(
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

        ClassicAssert.AreEqual(
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

        ClassicAssert.AreEqual(
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

        ClassicAssert.AreEqual(
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

        ClassicAssert.AreEqual(
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

        ClassicAssert.AreEqual(
            "-c log.showsignature=false -c bar=baz log -n 1",
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

        ClassicAssert.AreEqual(
            "-c log.showsignature=false -c bar=baz log -n 1",
            args.ToString());
    }

    [Test]
    public void Command_with_config_item_that_overrides_default()
    {
        GitArgumentBuilder args = new("log")
        {
            new GitConfigItem("log.showsignature", "true"),
            "-n 1"
        };

        ClassicAssert.AreEqual(
            "-c log.showsignature=true log -n 1",
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

        ClassicAssert.AreEqual(
            "-c LOG.showSIGNATURE=true log -n 1",
            args.ToString());
    }

    [Test]
    public void Throws_for_invalid_command_strings()
    {
        ClassicAssert.Throws<ArgumentNullException>(() => new GitArgumentBuilder(null));
        ClassicAssert.Throws<ArgumentException>(() => new GitArgumentBuilder(""));
        ClassicAssert.Throws<ArgumentException>(() => new GitArgumentBuilder(" "));
        ClassicAssert.Throws<ArgumentException>(() => new GitArgumentBuilder(" a banana "));
        ClassicAssert.Throws<ArgumentException>(() => new GitArgumentBuilder("!£$%^&*("));
    }
}
