using AwesomeAssertions;
using GitCommands;
using GitCommands.Logging;

namespace GitCommandsTests;

[TestFixture]
public class CommandLogTests
{
    [TestCase("somegit.exe", "", "")]
    [TestCase("somegit.exe", @"verb -c config", "verb")]
    [TestCase("somegit.exe", @"-c config verb", "verb")]
    [TestCase("somegit.exe", @"-c config --no-optional-locks verb", "verb")]
    [TestCase("somegit.exe", @"-c config1 -c config2=x verb", "verb")]
    [TestCase("somegit.exe", @"-c config1 -c config2 x verb", "x verb")]
    [TestCase("something.exe", @"-c config=""value with space"" verb", "verb")]
    [TestCase("gitOnTheRocks", "config list --local --includes --null", "config list --local --includes --null")]
    [TestCase("bitbucket", "--pam --param --parapam", "--pam --param --parapam")]
    [TestCase(@"C:\bitbucket\bucketbit.kexe", "--pam --param --parapam", "--pam --param --parapam")]
    // Not needed and not implemented yet. It just removes '-c core.xxx=""something with \""' at the moment.
    [TestCase("somegit.exe", @"-c core.xxx=""something with \""value1 in escaped quotes\"" \""value2\"""" verb", @"value1 in escaped quotes\"" \""value2\"""" verb")]
    [TestCase(@"C:\yesterday\someday.plexe", @"-c core.xxx=""something with \""value1 in escaped quotes\"" \""value2\"""" verb", @"value1 in escaped quotes\"" \""value2\"""" verb")]
    public void CommanLogEntry_ColumnLine_should_display_native_git_command(string fileName, string arguments, string expectedMainArguments)
    {
        string origGitCommandValue = AppSettings.GitCommand;
        AppSettings.GitCommandValue = fileName;

        CommandLogEntry commandLogEntry = new(fileName, arguments, workingDir: "", startedAt: DateTime.MinValue, isOnMainThread: true);
        commandLogEntry.ColumnLine.Should().Be($"00:00:00.000   running         UI     {CommandLogEntry.NativeGitLogName} {expectedMainArguments}");

        AppSettings.GitCommandValue = origGitCommandValue;
    }

    [TestCase("wsl", @"-d Ubuntu --cd ""\\wsl$\Ubuntu\home\user\repo\project"" git -c config=""value with space"" arg1 arg2", "arg1 arg2")]
    [TestCase(@"wsl -d Ubuntu --cd ""\\wsl$\Ubuntu\home\user\repo\project"" git", @"-c config=""value with space"" arg1 arg2", "arg1 arg2")]
    [TestCase("wsl", @"-d Ubuntu --cd ""\\wsl$\Ubuntu\home\user\repo\project"" git config list --local --includes --null", "config list --local --includes --null")]
    [TestCase(@"wsl -d Ubuntu --cd ""\\wsl$\\Ubuntu\\home\\user\\repo\\project git", "config list --local --includes --null", "config list --local --includes --null")]
    public void CommanLogEntry_ColumnLine_should_display_wsl_git_command(string fileName, string arguments, string expectedMainArguments)
    {
        string origGitCommandValue = AppSettings.GitCommand;
        AppSettings.GitCommandValue = "git";

        CommandLogEntry commandLogEntry = new(fileName, arguments, workingDir: "", startedAt: DateTime.MinValue, isOnMainThread: true);
        commandLogEntry.ColumnLine.Should().Be($"00:00:00.000   running         UI     {CommandLogEntry.WslGitLogName} {expectedMainArguments}");

        AppSettings.GitCommandValue = origGitCommandValue;
    }

    [TestCase("anycmd", @"-d Ubuntu --cd ""\\wsl$\Ubuntu\home\user\repo\project"" git -c config=""value with space"" agr1 arg2", @"-d Ubuntu --cd ""\\wsl$\Ubuntu\home\user\repo\project"" git -c config=""value with space"" agr1 arg2")]
    [TestCase(@"anycmd -d Ubuntu --cd ""\\wsl$\Ubuntu\home\user\repo\project"" git", @"-c config=""value with space"" arg1 arg2", @"-c config=""value with space"" arg1 arg2")]
    [TestCase("notAgit.exe", "", "")]
    [TestCase("notAgit.exe", "69 < 420", "69 < 420")]
    [TestCase("notAgit", "69 < 420", "69 < 420")]
    [TestCase("git", "69 < 420", "69 < 420")]
    public void CommanLogEntry_ColumnLine_should_display_non_git_command(string fileName, string arguments, string expectedMainArguments)
    {
        string origGitCommandValue = AppSettings.GitCommand;
        AppSettings.GitCommandValue = "notanycmd";

        CommandLogEntry commandLogEntry = new(fileName, arguments, workingDir: "", startedAt: DateTime.MinValue, isOnMainThread: true);
        commandLogEntry.ColumnLine.Should().Be($"00:00:00.000   running         UI     {fileName} {expectedMainArguments}");

        AppSettings.GitCommandValue = origGitCommandValue;
    }
}
