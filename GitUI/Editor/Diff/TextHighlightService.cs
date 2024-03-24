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

    public virtual void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
    {
    }

    public virtual void AddTextHighlighting(IDocument document)
    {
    }
}
