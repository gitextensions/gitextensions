using AvaloniaEdit.Document;
using GitExtensions.Extensibility;

namespace GitUI.Editor.Diff;

public class TextHighlightService : ITextHighlightService
{
    /// <summary>
    /// Base class for highlighting, not adding any highlighting.
    /// </summary>
    public static TextHighlightService Instance { get; } = new();

    protected TextHighlightService()
    {
    }

    public virtual void AddTextHighlighting(TextDocument document)
    {
    }

    public virtual bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
    {
        DebugHelpers.Fail($"Unexpected highlight service {GetType()}, not a diff/grep type.");
        return false;
    }
}
