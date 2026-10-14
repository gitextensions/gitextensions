using System.Text;
using GitUI;
using GitUI.Editor;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FileViewerInlineHighlightingTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void Deferred_markers_match_synchronous_highlighting(bool useGitColoring)
    {
        UITest.RunControl(form => new FileViewerInternal { Parent = form, Dock = DockStyle.Fill }, async editor =>
        {
            string diff = CreateDiff(useGitColoring);
            using TextEditorControl expected = new();
            DiffViewerLineNumberControl margin = new(expected.ActiveTextAreaControl.TextArea);
            PatchHighlightService service = new(ref diff, useGitColoring, margin);
            expected.Text = diff;
            service.AddTextHighlighting(expected.Document);

            editor.SetText(CreateDiff(useGitColoring), null, ViewMode.Diff, useGitColoring, null);
            editor.InlineHighlightingTask.IsCompleted.Should().BeFalse();
#pragma warning disable VSTHRD003 // Await the control's background calculation while pumping its UI context.
            await editor.InlineHighlightingTask;
#pragma warning restore VSTHRD003
            ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
            TextEditorControl actual = editor.GetTestAccessor().TextEditor;
            actual.Text.Should().Be(diff);
            actual.Document.MarkerStrategy.TextMarker.Select(Describe).Should().Equal(expected.Document.MarkerStrategy.TextMarker.Select(Describe));
        });
    }

    [Test]
    public void Deferred_highlighting_preserves_selection_and_ignores_replaced_documents()
    {
        UITest.RunControl(form => new FileViewerInternal { Parent = form, Dock = DockStyle.Fill }, async editor =>
        {
            editor.SetText(CreateDiff(false), null, ViewMode.Diff, false, null);
            TextEditorControl textEditor = editor.GetTestAccessor().TextEditor;
            textEditor.ActiveTextAreaControl.SelectionManager.SetSelection(new TextLocation(2, 4), new TextLocation(10, 4));
            string selected = textEditor.ActiveTextAreaControl.SelectionManager.SelectedText;
#pragma warning disable VSTHRD003 // Await work owned by the control under test.
            await editor.InlineHighlightingTask;
            textEditor.ActiveTextAreaControl.SelectionManager.SelectedText.Should().Be(selected);

            editor.SetText(CreateDiff(false), null, ViewMode.Diff, false, null);
            Task obsolete = editor.InlineHighlightingTask;
            editor.SetText("replacement", null);
            await obsolete;
#pragma warning restore VSTHRD003
            editor.GetText().Should().Be("replacement");
            textEditor.Document.MarkerStrategy.TextMarker.Should().BeEmpty();

            editor.SetText(CreateDiff(false), null, ViewMode.Diff, false, null);
            Task disposedWork = editor.InlineHighlightingTask;
            editor.Dispose();
#pragma warning disable VSTHRD003 // Verify disposal cancels the outstanding calculation.
            await disposedWork;
#pragma warning restore VSTHRD003
        });
    }

    private static object Describe(TextMarker marker) => new { marker.Offset, marker.Length, marker.TextMarkerType, marker.Color, marker.ForeColor };

    private static string CreateDiff(bool useGitColoring)
    {
        StringBuilder result = new("diff --git a/x b/x\n--- a/x\n+++ b/x\n@@ -1,6000 +1,6000 @@\n");
        for (int i = 0; i < 12000; i++)
        {
            if (useGitColoring)
            {
                result.Append(i % 2 == 0 ? "\x1b[7;31m" : "\x1b[7;32m");
            }

            result.Append(i % 2 == 0 ? '-' : '+').Append(" a sufficiently long changed source line ").Append(i);
            if (useGitColoring)
            {
                result.Append("\x1b[m");
            }

            result.Append('\n');
        }

        return result.ToString();
    }
}
