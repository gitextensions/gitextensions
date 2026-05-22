namespace GitUI.ConsoleEmulation;

/// <summary>
///  Represents console output produced by a process.
/// </summary>
public sealed class ConsoleOutputEventArgs(string text) : EventArgs
{
    /// <summary>
    ///  Gets the output text.
    /// </summary>
    public string Text { get; } = text;
}
