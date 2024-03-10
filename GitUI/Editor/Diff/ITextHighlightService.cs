using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using JetBrains.Annotations;

namespace GitUI.Editor.Diff;

public interface ITextHighlightService
{
    /// <summary>
    /// Add text highlighting to the document.
    /// This is primarily used to highlight changed files for diffs.
    /// Called when the text is changed.
    /// </summary>
    /// <param name="document">The document to highlight.</param>
    void AddTextHighlighting([NotNull] IDocument document);

    /// <summary>
    /// Set info in line number control for e.g. Patch/Diff views
    /// where the line number is non sequential.
    /// Other views (like normal text) do not use this control.
    /// </summary>
    /// <param name="lineNumbersControl">The line number control</param>
    /// <param name="textEditor">The textEditor contol with text to adjust for.</param>
    void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor);
}
