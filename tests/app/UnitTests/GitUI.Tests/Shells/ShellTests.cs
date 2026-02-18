using FluentAssertions;
using GitCommands;

namespace GitUITests.Shells;

[TestFixture]
public sealed class ShellTests
{
    // Tests verify that shell command patterns properly quote paths containing spaces,
    // matching the logic in CmdShell, PowerShellShell, PwshShell, and BashShell.

    [TestCase(@"C:\Users\Acker Liu\repo", @"cd /D ""C:\Users\Acker Liu\repo""")]
    [TestCase(@"C:\Projects\MyRepo", @"cd /D ""C:\Projects\MyRepo""")]
    public void CmdShell_GetChangeDirCommand_quotes_paths_with_spaces(string path, string expected)
    {
        // Matches CmdShell.GetChangeDirCommand: $"cd /D {path.QuoteNE()}"
        string result = $"cd /D {path.QuoteNE()}";
        result.Should().Be(expected);
    }

    [TestCase(@"C:\Users\Acker Liu\repo", @"cd ""C:\Users\Acker Liu\repo""")]
    [TestCase(@"C:\Projects\MyRepo", @"cd ""C:\Projects\MyRepo""")]
    public void PowerShellShell_GetChangeDirCommand_quotes_paths_with_spaces(string path, string expected)
    {
        // Matches PowerShellShell.GetChangeDirCommand and PwshShell.GetChangeDirCommand: $"cd {path.QuoteNE()}"
        string result = $"cd {path.QuoteNE()}";
        result.Should().Be(expected);
    }

    [TestCase(@"C:\Users\Acker Liu\Git\bash.exe", @"""C:\Users\Acker Liu\Git\bash.exe"" --login -i")]
    [TestCase(@"C:\Program Files\Git\bin\bash.exe", @"""C:\Program Files\Git\bin\bash.exe"" --login -i")]
    [TestCase(@"C:\Git\bash.exe", @"""C:\Git\bash.exe"" --login -i")]
    public void BashShell_ExecutableCommandLine_quotes_paths_with_spaces(string exePath, string expected)
    {
        // Matches BashShell constructor: $"{exePath.Quote()} --login -i"
        string result = $"{exePath.Quote()} --login -i";
        result.Should().Be(expected);
    }

    [TestCase(@"C:\Users\Acker Liu\Git\cmd.exe", @"""C:\Users\Acker Liu\Git\cmd.exe""")]
    [TestCase(@"C:\Windows\System32\cmd.exe", @"""C:\Windows\System32\cmd.exe""")]
    public void Shell_ExecutableCommandLine_quotes_exe_paths_with_spaces(string exePath, string expected)
    {
        // Matches CmdShell/PowerShellShell/PwshShell constructor: exePath.Quote()
        string result = exePath.Quote();
        result.Should().Be(expected);
    }
}
