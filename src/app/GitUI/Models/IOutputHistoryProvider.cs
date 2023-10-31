namespace GitUI.Models;

public interface IOutputHistoryProvider
{
    /// <summary>
    ///  Is invoked when something was added to the output history.
    /// </summary>
    event EventHandler HistoryChanged;

    /// <summary>
    ///  Gets whether the output history is enabled.
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    ///  Gets the current output history formatted as string.
    /// </summary>
    string History { get; }

    /// <summary>
    ///  Clears the recorded history.
    /// </summary>
    void ClearHistory();
}
