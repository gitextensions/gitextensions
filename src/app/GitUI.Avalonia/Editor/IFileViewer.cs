namespace GitUI.Editor;

// Twin of the event args declared in GitUI/Editor/IFileViewer.cs; the IFileViewer
// interface itself is not ported yet (the reduced FileViewer is used directly).
public class SelectedLineEventArgs : EventArgs
{
    public SelectedLineEventArgs(int selectedLine)
    {
        SelectedLine = selectedLine;
    }

    public int SelectedLine { get; }
}
