using GitExtensions.Extensibility;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

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

    public virtual void AddTextHighlighting(IDocument document)
    {
    }

    public virtual bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
    {
        DebugHelpers.Fail($"Unexpected highlight service {GetType()}, not a diff/grep type.");
        return false;
    }

    public virtual void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
    {
    }
}
