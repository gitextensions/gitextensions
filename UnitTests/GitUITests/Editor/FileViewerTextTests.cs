using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using ApprovalTests;
using ApprovalTests.Namers;
using CommonTestUtils;
using GitCommands.Settings;
using GitUI;
using GitUI.Editor;
using ICSharpCode.TextEditor;
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
                testHelper.CreateRepoFile(@".git\config", $"[core]\n\tautocrlf = {autoCRLFType}");
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
    }
}
