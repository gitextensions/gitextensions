namespace GitExtensions.Extensibility.Plugins;

public sealed class ConsoleTextEventArgs : EventArgs
{
    public string Text { get; }

    public ConsoleTextEventArgs(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }
}
