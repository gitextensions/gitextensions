namespace GitUI.ConsoleEmulation.PlainText;

internal interface IPlainTextConsoleCommandRunner : IConsoleCommandRunner
{
    void WriteOutputText(string text);
}
