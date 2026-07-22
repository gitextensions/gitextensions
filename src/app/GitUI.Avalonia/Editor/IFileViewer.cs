namespace GitUI.Editor;

public class SelectedLineEventArgs : EventArgs
{
    public SelectedLineEventArgs(int selectedLine)
    {
        SelectedLine = selectedLine;
    }

    public int SelectedLine { get; }
}

/// <summary>
/// Portable portion of the WinForms IFileViewer contract. Framework-specific mouse, key,
/// font, and scrollbar event types remain at each UI implementation boundary.
/// </summary>
public interface IFileViewer
{
    event EventHandler? TextChanged;
    event EventHandler<SelectedLineEventArgs>? SelectedLineChanged;
    event EventHandler? TextLoaded;

    void Find(bool replace);
    Task FindNextAsync(bool searchForwardOrOpenWithDifftool);
    string GetText();
    string GetSelectedText();
    int GetSelectionPosition();
    int GetSelectionLength();
    void HighlightLines(int startLine, int endLine, System.Drawing.Color color);
    void ClearHighlighting();
    void Refresh();
    int GetLineFromVisualPosY(double visualPosY);
    void GoToLine(int lineNumber);
    void SetFileLoader(GetNextFileFnc fileLoader);
    void GoToNextOccurrence();
    void GoToPreviousOccurrence();

    Action? OpenWithDifftool { get; }
    bool? ShowLineNumbers { get; set; }
    int VRulerPosition { get; set; }
    bool IsReadOnly { get; set; }
    int TotalNumberOfLines { get; }
    int MaxLineNumber { get; }
}
