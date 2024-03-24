using FluentAssertions;
using GitCommands.Logging;

namespace GitCommandsTests;

[TestFixture]
public class CommandLogTests
{
    [TestCase("", "")]
    [TestCase(@"verb -c config", "verb")]
    [TestCase(@"-c config verb", "verb")]
    [TestCase(@"-c config --no-optional-locks verb", "verb")]
    [TestCase(@"-c config1 -c config2=x verb", "verb")]
    [TestCase(@"-c config1 -c config2 x verb", "x verb")]
    [TestCase(@"-c config=""value with space"" verb", "verb")]
    // Not needed and not implemented yet. It just removes '-c core.xxx=""something with \""' at the moment.
    [TestCase(@"-c core.xxx=""something with \""value1 in escaped quotes\"" \""value2\"""" verb", @"value1 in escaped quotes\"" \""value2\"""" verb")]
    public void CommanLogEntry_ColumnLine(string arguments, string expectedMainArguments)
    {
        const string gitExecutable = "somegit.exe";
        CommandLogEntry commandLogEntry = new(gitExecutable, arguments, workingDir: "", startedAt: DateTime.MinValue, isOnMainThread: true);
        commandLogEntry.ColumnLine.Should().Be($"00:00:00.000   running         UI     git {expectedMainArguments}");
    }
}
