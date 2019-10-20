﻿using System.Linq;
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
    }
}
