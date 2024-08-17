using FluentAssertions;
using GitUI.Editor;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor;

namespace GitUITests.Editor
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public class CurrentViewPositionCacheTests
    {
        private FileViewerInternal _fileViewerInternal;
        private FileViewerInternal.CurrentViewPositionCache _viewPositionCache;

        [SetUp]
        public void Setup()
        {
            _fileViewerInternal = new FileViewerInternal();

            _viewPositionCache = new FileViewerInternal.CurrentViewPositionCache(_fileViewerInternal);
        }

        [TearDown]
        public void TearDown()
        {
            _fileViewerInternal.Dispose();
        }

        [TestCase(null)]
        [TestCase("a")]
        public void Capture_should_not_change_capture_if_less_then_two_lines(string text)
        {
            FileViewerInternal.CurrentViewPositionCache.TestAccessor test = _viewPositionCache.GetTestAccessor();

            FileViewerInternal.ViewPosition existingViewPosition = new()
            {
                FirstLine = "first line",
                FirstVisibleLine = 24,
                TotalNumberOfLines = 35
            };
            test.ViewPosition = existingViewPosition;

            test.TextEditor.ShowLineNumbers = true;
            test.TextEditor.Text = text;

            _viewPositionCache.Capture();

            test.ViewPosition.Should().Be(existingViewPosition);
        }

        [Test]
        public void Capture_should_capture_current_position_if_ShowLineNumbers_true_start()
        {
            FileViewerInternal.CurrentViewPositionCache.TestAccessor test = _viewPositionCache.GetTestAccessor();
            test.TextEditor.ShowLineNumbers = true;
            test.TextEditor.Text = "a\r\nb\r\nc\r\n";

            _viewPositionCache.GetTestAccessor().SetCurrentIdentification("dummy");
            _viewPositionCache.Capture();

            test.ViewPosition.ActiveLineNum.Should().BeNull();
            test.ViewPosition.CaretPosition.Should().Be(new TextLocation(0, 0));
            test.ViewPosition.CaretVisible.Should().BeTrue();
            test.ViewPosition.FirstLine.Should().Be("a");
            test.ViewPosition.FirstVisibleLine.Should().Be(0);
            test.ViewPosition.TotalNumberOfLines.Should().Be(4);
        }

        [Test]
        public void Capture_should_capture_current_position_if_ShowLineNumbers_true_scrolled()
        {
            FileViewerInternal.CurrentViewPositionCache.TestAccessor test = _viewPositionCache.GetTestAccessor();
            test.TextEditor.ShowLineNumbers = true;
            test.TextEditor.Text = "a\r\nb\r\nc\r\nd\r\ne\r\nf\r\ng\r\nh\r\ni\r\nj\r\nk\r\nl\r\nm\r\nn\r\no\r\np\r\nr\r\ns\r\nt\r\nu\r\nv\r\nw\r\nx\r\ny\r\nz\r\n0\r\n1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n0";
            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Line = 23;
            test.TextEditor.ActiveTextAreaControl.TextArea.ScrollToCaret();
            test.TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = 22;

            _viewPositionCache.GetTestAccessor().SetCurrentIdentification("dummy");
            _viewPositionCache.Capture();

            test.ViewPosition.ActiveLineNum.Should().BeNull();
            test.ViewPosition.CaretPosition.Should().Be(new TextLocation(0, 23));
            test.ViewPosition.CaretVisible.Should().BeFalse();
            test.ViewPosition.FirstLine.Should().Be("a");
            test.ViewPosition.FirstVisibleLine.Should().Be(22);
            test.ViewPosition.TotalNumberOfLines.Should().Be(36);
        }

        [Test]
        public void Capture_should_capture_current_position_if_ShowLineNumbers_false_without_margin()
        {
            FileViewerInternal.CurrentViewPositionCache.TestAccessor test = _viewPositionCache.GetTestAccessor();
            test.TextEditor.ShowLineNumbers = false;
            test.TextEditor.Text = "a\r\nb\r\nc\r\nd\r\ne\r\nf\r\ng\r\nh\r\ni\r\nj\r\nk\r\nl\r\nm\r\nn\r\no\r\np\r\nr\r\ns\r\nt\r\nu\r\nv\r\nw\r\nx\r\ny\r\nz\r\n0\r\n1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n0";
            test.TextEditor.ActiveTextAreaControl.TextArea.TextView.DrawingPosition = new Rectangle(0, 0, 100, 100);
            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Line = 23;
            test.TextEditor.ActiveTextAreaControl.TextArea.ScrollToCaret();
            test.TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = 22;

            _viewPositionCache.GetTestAccessor().SetCurrentIdentification("dummy");
            _viewPositionCache.Capture();

            test.ViewPosition.ActiveLineNum.Should().BeNull();
            test.ViewPosition.CaretPosition.Should().Be(new TextLocation(0, 23));
            test.ViewPosition.CaretVisible.Should().BeTrue();
            test.ViewPosition.FirstLine.Should().Be("a");
            test.ViewPosition.FirstVisibleLine.Should().Be(22);
            test.ViewPosition.TotalNumberOfLines.Should().Be(36);
        }

        [Test]
        public void Capture_should_capture_current_position_and_calculate_active_line_if_ShowLineNumbers_false_with_line_margin()
        {
            FileViewerInternal.CurrentViewPositionCache.TestAccessor test = _viewPositionCache.GetTestAccessor();
            test.TextEditor.ShowLineNumbers = false;
            test.TextEditor.Text = Given.GitDiff;
            test.TextEditor.ActiveTextAreaControl.TextArea.TextView.DrawingPosition = new Rectangle(0, 0, 100, 100);
            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Line = 19;
            test.TextEditor.ActiveTextAreaControl.TextArea.ScrollToCaret();
            test.TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = 18;
            test.LineNumberControl = new DiffViewerLineNumberControl(test.TextEditor.ActiveTextAreaControl.TextArea);
            DiffLinesInfo result = DiffLineNumAnalyzer.Analyze(test.TextEditor, isCombinedDiff: false);
            test.LineNumberControl.DisplayLineNum(result, showLeftColumn: true);

            _viewPositionCache.GetTestAccessor().SetCurrentIdentification("dummy");
            _viewPositionCache.Capture();

            test.ViewPosition.ActiveLineNum.Should().NotBeNull();
            test.ViewPosition.ActiveLineNum.LeftLineNumber.Should().Be(57);
            test.ViewPosition.ActiveLineNum.LineNumInDiff.Should().Be(20);
            test.ViewPosition.ActiveLineNum.LineType.Should().Be(DiffLineType.Minus);
            test.ViewPosition.ActiveLineNum.RightLineNumber.Should().Be(-1);
            test.ViewPosition.CaretPosition.Should().Be(new TextLocation(0, 19));
            test.ViewPosition.CaretVisible.Should().BeTrue();
            test.ViewPosition.FirstLine.Should().Be("diff --git a/GitUI/CommandsDialogs/SettingsDialog/Pages/ShellExtensionSettingsPage.Designer.cs b/GitUI/CommandsDialogs/SettingsDialog/Pages/ShellExtensionSettingsPage.Designer.cs");
            test.ViewPosition.FirstVisibleLine.Should().Be(18);
            test.ViewPosition.TotalNumberOfLines.Should().Be(23);
        }

        [Test]
        public void Restore_should_restore_current_position()
        {
            FileViewerInternal.CurrentViewPositionCache.TestAccessor test = _viewPositionCache.GetTestAccessor();
            string text = Given.GitDiff;
            test.TextEditor.Text = text;
            test.TextEditor.ActiveTextAreaControl.TextArea.TextView.DrawingPosition = new Rectangle(0, 0, 100, 100);
            test.TextEditor.ShowLineNumbers = false;

            FileViewerInternal.ViewPosition existingViewPosition = new()
            {
                ActiveLineNum = new DiffLineInfo
                {
                    LeftLineNumber = 57,
                    LineNumInDiff = 20,
                    LineType = DiffLineType.Minus,
                    RightLineNumber = -1
                },
                CaretPosition = new TextLocation(0, 19),
                CaretVisible = true,
                FirstLine = "diff --git a/GitUI/CommandsDialogs/SettingsDialog/Pages/ShellExtensionSettingsPage.Designer.cs b/GitUI/CommandsDialogs/SettingsDialog/Pages/ShellExtensionSettingsPage.Designer.cs",
                FirstVisibleLine = 18,
                TotalNumberOfLines = 23
            };
            test.ViewPosition = existingViewPosition;

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Line.Should().Be(0);
            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Column.Should().Be(0);

            _viewPositionCache.GetTestAccessor().SetCapturedIdentification("current");

            ITextHighlightService highlightService = new PatchHighlightService(ref text, true);
            test.LineNumberControl.Clear();
            highlightService.SetLineControl(test.LineNumberControl, test.TextEditor);
            _viewPositionCache.Restore("current");

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Position.Should().Be(existingViewPosition.CaretPosition,
                "position should be restored to the (inserted) existingViewPosition");

            _viewPositionCache.Capture();
            test.TextEditor.Text = "";
            test.LineNumberControl.Clear();
            highlightService.SetLineControl(test.LineNumberControl, test.TextEditor);
            _viewPositionCache.Restore("empty");

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Position.Should().Be(new TextLocation(0, 0),
                "empty file, set the position to the first");

            highlightService = new PatchHighlightService(ref text, true);
            _viewPositionCache.Capture();
            test.TextEditor.Text = text;
            test.LineNumberControl.Clear();
            highlightService.SetLineControl(test.LineNumberControl, test.TextEditor);
            _viewPositionCache.Restore("current");

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Position.Should().Be(existingViewPosition.CaretPosition,
                "the captured position should not be reset by an empty text");

            _viewPositionCache.Capture();
            test.TextEditor.Text = "some\ninfo\n\nnot empty";
            test.LineNumberControl.Clear();
            highlightService.SetLineControl(test.LineNumberControl, test.TextEditor);
            _viewPositionCache.Restore("");

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Position.Should().Be(new TextLocation(0, 0),
                "Empty name, position should be the first");

            highlightService = new PatchHighlightService(ref text, true);
            _viewPositionCache.Capture();
            test.TextEditor.Text = text;
            test.LineNumberControl.Clear();
            highlightService.SetLineControl(test.LineNumberControl, test.TextEditor);
            _viewPositionCache.Restore("current");

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Position.Should().Be(existingViewPosition.CaretPosition,
                "the captured position should not be reset by empty name");

            _viewPositionCache.Capture();
            test.TextEditor.Text = "some\ninfo\n\nnot empty";
            test.LineNumberControl.Clear();
            highlightService.SetLineControl(test.LineNumberControl, test.TextEditor);
            _viewPositionCache.Restore(null);

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Position.Should().Be(new TextLocation(0, 0),
                "null name, set the position to the first");

            highlightService = new PatchHighlightService(ref text, true);
            _viewPositionCache.Capture();
            test.TextEditor.Text = text;
            test.LineNumberControl.Clear();
            highlightService.SetLineControl(test.LineNumberControl, test.TextEditor);
            _viewPositionCache.Restore("current");

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Position.Should().Be(existingViewPosition.CaretPosition,
                "the captured position should not be reset by null identification");

            string text2 = "dummy text\ndummy";
            _viewPositionCache.Capture();
            highlightService = new PatchHighlightService(ref text2, true);
            test.TextEditor.Text = text2;
            test.LineNumberControl.Clear();
            highlightService.SetLineControl(test.LineNumberControl, test.TextEditor);
            _viewPositionCache.Restore("dummy");

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Position.Should().Be(new TextLocation(0, 0),
                "a new file will set the position back to 0,0");

            highlightService = new PatchHighlightService(ref text, true);
            _viewPositionCache.Capture();
            test.TextEditor.Text = text;
            test.LineNumberControl.Clear();
            highlightService.SetLineControl(test.LineNumberControl, test.TextEditor);
            _viewPositionCache.Restore("current");

            test.TextEditor.ActiveTextAreaControl.TextArea.Caret.Position.Should().Be(new TextLocation(0, 0),
                "reset to 0,0. the cached position is not restored");
        }

        private static class Given
        {
            public const string GitDiff = @"diff --git a/GitUI/CommandsDialogs/SettingsDialog/Pages/ShellExtensionSettingsPage.Designer.cs b/GitUI/CommandsDialogs/SettingsDialog/Pages/ShellExtensionSettingsPage.Designer.cs
index 7ac22add9..77aa0364a 100644
--- a/GitUI/CommandsDialogs/SettingsDialog/Pages/ShellExtensionSettingsPage.Designer.cs
+++ b/GitUI/CommandsDialogs/SettingsDialog/Pages/ShellExtensionSettingsPage.Designer.cs
@@ -46,23 +46,24 @@ private void InitializeComponent()
             // 
             // panel1
             // 
-            panel1.AutoSize = true;
             panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
+            panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
             panel1.Controls.Add(this.labelPreview);
             panel1.Controls.Add(this.label1);
-            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
-            panel1.Location = new System.Drawing.Point(323, 77);
+            panel1.Dock = System.Windows.Forms.DockStyle.Top;
+            panel1.Location = new System.Drawing.Point(540, 81);
             panel1.Margin = new System.Windows.Forms.Padding(2);
             panel1.Name = ""panel1"";
-            panel1.Size = new System.Drawing.Size(317, 391);
+            panel1.Size = new System.Drawing.Size(533, 167);
            panel1.TabIndex = 0;
            ";
        }
    }
}
