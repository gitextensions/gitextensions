using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using ApprovalTests;
using ApprovalTests.Namers;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.Settings;
using GitUI;
using GitUI.Editor;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Editor
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public class FileViewerTextTests
    {
        private IGitUICommandsSource _uiCommandsSource;
        private FileViewer _fileViewer;

        [SetUp]
        public void SetUp()
        {
            _uiCommandsSource = Substitute.For<IGitUICommandsSource>();
            _fileViewer = new FileViewer();
        }

        [Test]
        [TestCase(AutoCRLFType.@true, "UnixLines")]
        [TestCase(AutoCRLFType.@true, "MacLines")]
        [TestCase(AutoCRLFType.@true, "WindowsLines")]
        [TestCase(AutoCRLFType.@false, "UnixLines")]
        [TestCase(AutoCRLFType.@false, "MacLines")]
        [TestCase(AutoCRLFType.@false, "WindowsLines")]
        [TestCase(AutoCRLFType.input, "UnixLines")]
        [TestCase(AutoCRLFType.input, "MacLines")]
        [TestCase(AutoCRLFType.input, "WindowsLines")]
        public void DoAutoCRLF_should_not_unnecessarily_duplicate_line_ending(AutoCRLFType autoCRLFType, string file)
        {
            using (var testHelper = new GitModuleTestHelper())
            {
                testHelper.Module.LocalConfigFile.SetEnum("core.autocrlf", autoCRLFType);
                testHelper.Module.LocalConfigFile.Save();

                var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.{file}.bin");

                var uiCommands = new GitUICommands(testHelper.Module);
                _uiCommandsSource.UICommands.Returns(x => uiCommands);
                _fileViewer.UICommandsSource = _uiCommandsSource;

                var fvi = _fileViewer.GetTestAccessor().FileViewerInternal;
                fvi.SetText(content, null, false);
                var doc = fvi.GetTestAccessor().TextEditor.Document;
                var end = doc.OffsetToPosition(content.Length);
                fvi.GetTestAccessor().TextEditor.ActiveTextAreaControl.SelectionManager.SetSelection(new TextLocation(0, 0), end);

                // act - 'copy to selection' menu click will call through to DoAutoCRLF
                _fileViewer.GetTestAccessor().CopyToolStripMenuItem.PerformClick();

                // assert
                var text = Clipboard.GetText();
                using (ApprovalResults.ForScenario(file, autoCRLFType))
                {
                    Approvals.Verify(text);
                }
            }
        }

        /// <summary>
        /// When: some text is selected in a <see cref="FileViewer"/>.
        /// Assert: All occurences of the selected text are highlighted.
        /// </summary>
        [Test]
        public void FileViewer_highlight_selected_text_when_enabled()
        {
            const string textToSelect = "Selected";
            const string selectedLinePrefix = "2:";
            const string sampleText =
                        "0\n" +
                        "1:Selected\n" +
                        "2:\n" +
                        "3:Selected\n" +
                        "4:";
            using (var testHelper = new GitModuleTestHelper())
            {
                var uiCommands = new GitUICommands(testHelper.Module);
                _uiCommandsSource.UICommands.Returns(x => uiCommands);
                _fileViewer.UICommandsSource = _uiCommandsSource;
                _fileViewer.GetTestAccessor().FileViewerInternal.SetText(sampleText, null);
                SelectionManager selectionManager = _fileViewer.GetTestAccessor().FileViewerInternal.GetTestAccessor().TextEditor.ActiveTextAreaControl.TextArea.SelectionManager;

                // act
                selectionManager.SetSelection(new TextLocation(selectedLinePrefix.Length, 1), new TextLocation(selectedLinePrefix.Length + textToSelect.Length, 1));

                // assert
                selectionManager.SelectedText.Should().Be(textToSelect);
                selectionManager.SelectionCollection.Single().StartPosition.Line.Should().Be(1);
                int caretOffset = _fileViewer.GetTestAccessor().FileViewerInternal.GetTestAccessor().TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset;
                caretOffset.Should().Be(0, "Caret position should be in default state");

                _fileViewer.GetTestAccessor().FileViewerInternal.GoToNextOccurrence();
                caretOffset = _fileViewer.GetTestAccessor().FileViewerInternal.GetTestAccessor().TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset;
                caretOffset.Should().Be(4, $"Caret should have been moved to the first occurrence of '{textToSelect}'");

                _fileViewer.GetTestAccessor().FileViewerInternal.GoToNextOccurrence();
                caretOffset = _fileViewer.GetTestAccessor().FileViewerInternal.GetTestAccessor().TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset;
                caretOffset.Should().Be(18, "Caret should have been moved to the second occurence of '" + textToSelect + "'");

                _fileViewer.GetTestAccessor().FileViewerInternal.GoToPreviousOccurrence();
                caretOffset = _fileViewer.GetTestAccessor().FileViewerInternal.GetTestAccessor().TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset;
                caretOffset.Should().Be(4, "Caret should have been moved back to the first occurence of '" + textToSelect + "'");
            }
        }

        [Test]
        public void FileViewer_ShowSyntaxHighlightingInDiff_enabled_use_highlighting()
        {
            // a sample c sharp file diff
            const string sampleCsharpPatch =
@"diff --git a/GitUI/Editor/FileViewerInternal.cs b/GitUI/Editor/FileViewerInternal.cs
index 62a5c2f08..2bc482714 100644
--- a/GitUI/Editor/FileViewerInternal.cs
+++ b/GitUI/Editor/FileViewerInternal.cs
@@ -229 +229 @@ public void SetText(string text, Action openWithDifftool, bool isDiff = false)
-            int scrollPos = ScrollPos;
+            int scrollPos = VScrollPosition;";

            using (var testHelper = new GitModuleTestHelper())
            {
                var uiCommands = new GitUICommands(testHelper.Module);
                _uiCommandsSource.UICommands.Returns(x => uiCommands);
                _fileViewer.UICommandsSource = _uiCommandsSource;

                FileViewer.TestAccessor testAccessor = _fileViewer.GetTestAccessor();
                testAccessor.ShowSyntaxHighlightingInDiff = true;

                // act
                _fileViewer.ViewPatch(sampleCsharpPatch, null, "FileViewerInternal.cs");

                // assert
                IHighlightingStrategy csharpHighlighting = HighlightingManager.Manager.FindHighlighterForFile("anycsharpfile.cs");

                csharpHighlighting.Should().NotBe(HighlightingManager.Manager.DefaultHighlighting);

                _fileViewer.GetTestAccessor().FileViewerInternal.GetTestAccessor().TextEditor.Document
                    .HighlightingStrategy.Should().Be(csharpHighlighting);
            }
        }

        [Test]
        public void FileViewer_ShowSyntaxHighlightingInDiff_disabled_do_not_use_highlighting()
        {
            // a sample c sharp file diff
            const string sampleCsharpPatch =
@"diff --git a/GitUI/Editor/FileViewerInternal.cs b/GitUI/Editor/FileViewerInternal.cs
index 62a5c2f08..2bc482714 100644
--- a/GitUI/Editor/FileViewerInternal.cs
+++ b/GitUI/Editor/FileViewerInternal.cs
@@ -229 +229 @@ public void SetText(string text, Action openWithDifftool, bool isDiff = false)
-            int scrollPos = ScrollPos;
+            int scrollPos = VScrollPosition;";

            using (var testHelper = new GitModuleTestHelper())
            {
                var uiCommands = new GitUICommands(testHelper.Module);
                _uiCommandsSource.UICommands.Returns(x => uiCommands);
                _fileViewer.UICommandsSource = _uiCommandsSource;

                FileViewer.TestAccessor testAccessor = _fileViewer.GetTestAccessor();
                testAccessor.ShowSyntaxHighlightingInDiff = false;

                // act
                _fileViewer.ViewPatch(sampleCsharpPatch, null, "FileViewerInternal.cs");

                // assert
                _fileViewer.GetTestAccessor().FileViewerInternal.GetTestAccessor().TextEditor.Document.HighlightingStrategy.Should().Be(HighlightingManager.Manager.DefaultHighlighting);
            }
        }

        [Test]
        public void FileViewer_given_non_text_use_default_highlighting()
        {
            // a sample c sharp file diff
            const string sampleBinaryPatch =
                @"diff --git a/binaryfile.bin b/binaryfile.bin
index b25b745..5194740 100644
Binary files a/binaryfile.bin and b/binaryfile.bin differ";

            using (var testHelper = new GitModuleTestHelper())
            {
                var uiCommands = new GitUICommands(testHelper.Module);
                _uiCommandsSource.UICommands.Returns(x => uiCommands);
                _fileViewer.UICommandsSource = _uiCommandsSource;

                // act
                _fileViewer.ViewPatch(sampleBinaryPatch, null, "binaryfile.bin");

                // assert
                _fileViewer.GetTestAccessor().FileViewerInternal.GetTestAccessor().TextEditor.Document.HighlightingStrategy.Should().Be(HighlightingManager.Manager.DefaultHighlighting);

                const string sampleRandomText =
                @"fldaksjflkdsjlfj";

                // act
                _fileViewer.ViewPatch(sampleRandomText, null, null);

                // assert
                _fileViewer.GetTestAccessor().FileViewerInternal.GetTestAccessor().TextEditor.Document.HighlightingStrategy.Should().Be(HighlightingManager.Manager.DefaultHighlighting);
            }
        }
    }
}
