using ICSharpCode.TextEditor.Document;
using JetBrains.Annotations;

namespace GitUI.Editor.Diff
{
    public interface ITextHighlightService
    {
        /// <summary>
        /// Add text highlighting to the document.
        /// This is primarily used to highlight changed files for diffs
        /// </summary>
        /// <param name="document">The document to highlight.</param>
        void AddTextHighlighting([NotNull] IDocument document);
    }
}
