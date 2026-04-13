using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitExtUtilsTests;
public sealed class GitArgumentBuilderTests
{
    [Test]
    public void Command_with_one_argument()
    {
        new GitArgumentBuilder("foo") { "--bar" }.ToString().Should().Be("foo --bar");
    }

    [Test]
    public void Command_with_no_arguments()
    {
        new GitArgumentBuilder("foo").ToString().Should().Be("foo");
    }

    [Test]
    public void Command_with_default_config_item()
    {
        GitArgumentBuilder args = new("log")
        {
            "-n 1"
        };

        args.ToString().Should().Be("-c log.showsignature=false log -n 1");
    }

    [Test]
    public void Command_with_custom_config_item()
    {
        GitArgumentBuilder args = new("foo")
        {
            new GitConfigItem("bar", "baz")
        };

        args.ToString().Should().Be("-c bar=baz foo");
    }

    [Test]
    public void Command_with_custom_config_item_quoted()
    {
        GitArgumentBuilder args = new("foo")
        {
            new GitConfigItem("bar", "baz bax")
        };

        args.ToString().Should().Be("-c bar=\"baz bax\" foo");
    }

    [Test]
    public void Command_with_custom_config_item_already_quoted()
    {
        GitArgumentBuilder args = new("foo")
        {
            new GitConfigItem("bar", "\"baz bax\"")
        };

        args.ToString().Should().Be("-c bar=\"baz bax\" foo");
    }

    [Test]
    public void Command_with_custom_config_item_and_argument()
    {
        GitArgumentBuilder args = new("foo")
        {
            new GitConfigItem("bar", "baz"),
            "--arg"
        };

        args.ToString().Should().Be("-c bar=baz foo --arg");
    }

    [Test]
    public void Command_with_custom_config_item_defined_after_argument()
    {
        GitArgumentBuilder args = new("foo")
        {
            "--arg",
            new GitConfigItem("bar", "baz") // order doesn't matter
        };

        args.ToString().Should().Be("-c bar=baz foo --arg");
    }

    [Test]
    public void Command_with_default_and_custom_config_items()
    {
        GitArgumentBuilder args = new("log")
        {
            new GitConfigItem("bar", "baz"),
            "-n 1"
        };

        args.ToString().Should().Be("-c log.showsignature=false -c bar=baz log -n 1");
    }

    [Test]
    public void Command_with_default_and_custom_config_items_and_argument()
    {
        GitArgumentBuilder args = new("log")
        {
            new GitConfigItem("bar", "baz"),
            "-n 1"
        };

        args.ToString().Should().Be("-c log.showsignature=false -c bar=baz log -n 1");
    }

    [Test]
    public void Command_with_config_item_that_overrides_default()
    {
        GitArgumentBuilder args = new("log")
        {
            new GitConfigItem("log.showsignature", "true"),
            "-n 1"
        };

        args.ToString().Should().Be("-c log.showsignature=true log -n 1");
    }

    [Test]
    public void Command_with_config_item_that_overrides_default_different_case()
    {
        GitArgumentBuilder args = new("log")
        {
            new GitConfigItem("LOG.showSIGNATURE", "true"),
            "-n 1"
        };

        args.ToString().Should().Be("-c LOG.showSIGNATURE=true log -n 1");
    }

    [Test]
    public void Throws_for_invalid_command_strings()
    {
        ((Action)(() => new GitArgumentBuilder(null!))).Should().Throw<ArgumentNullException>();
        ((Action)(() => new GitArgumentBuilder(""))).Should().Throw<ArgumentException>();
        ((Action)(() => new GitArgumentBuilder(" "))).Should().Throw<ArgumentException>();
        ((Action)(() => new GitArgumentBuilder(" a banana "))).Should().Throw<ArgumentException>();
        ((Action)(() => new GitArgumentBuilder("!£$%^&*("))).Should().Throw<ArgumentException>();
    }
}
