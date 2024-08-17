using System.Text;
using CommonTestUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Editor;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormEditorTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void HasChanges_updated_correctly()
        {
            string filePath = Path.Combine(_referenceRepository.Module.WorkingDir, Path.GetRandomFileName());

            try
            {
                File.WriteAllText(filePath, "Hello↔world", _commands.Module.FilesEncoding);

                UITest.RunForm<FormEditor>(
                    () =>
                    {
                        using FormEditor formEditor = new(_commands, filePath, showWarning: false);
                        formEditor.ShowDialog();
                    },
                    form =>
                    {
                        Assert.False(form.GetTestAccessor().HasChanges);

                        FileViewerInternal fileViewerInternal = form.GetTestAccessor().FileViewer.GetTestAccessor().FileViewerInternal;
                        fileViewerInternal.SetText(fileViewerInternal.GetText() + "!", openWithDifftool: null);

                        Assert.True(form.GetTestAccessor().HasChanges);

                        form.GetTestAccessor().SaveChanges();

                        Assert.False(form.GetTestAccessor().HasChanges);

                        return Task.CompletedTask;
                    });
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public void Should_set_linenumber_correctly()
        {
            string filePath = Path.Combine(_referenceRepository.Module.WorkingDir, Path.GetRandomFileName());

            try
            {
                const int lineNumberDefault = 2;
                File.WriteAllText(filePath, "Hello↔world\nline2\nline3\n\n\n", _commands.Module.FilesEncoding);

                UITest.RunForm<FormEditor>(
                    () =>
                    {
                        using FormEditor formEditor = new(_commands, filePath, showWarning: false, lineNumber: lineNumberDefault);
                        formEditor.ShowDialog();
                    },
                    form =>
                    {
                        Assert.False(form.GetTestAccessor().HasChanges);

                        FileViewerInternal fileViewerInternal = form.GetTestAccessor().FileViewer.GetTestAccessor().FileViewerInternal;
                        fileViewerInternal.GetTestAccessor().CurrentViewPositionCache.GetTestAccessor().SetCurrentIdentification("hello-world");
                        fileViewerInternal.SetText(fileViewerInternal.GetText() + "!", openWithDifftool: null, ViewMode.Text, true, "hello-world");

                        Assert.True(form.GetTestAccessor().HasChanges);

                        form.GetTestAccessor().SaveChanges();

                        Assert.False(form.GetTestAccessor().HasChanges);

                        Assert.AreEqual(lineNumberDefault, fileViewerInternal.CurrentFileLine());

                        return Task.CompletedTask;
                    });
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Ignore("Unstable UTF8EncodingSealed result")]
        [Test]
        public void Should_preserve_encoding_utf8()
        {
            Should_preserve_encoding(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        [Ignore("Unstable UTF8EncodingSealed result")]
        [Test]
        public void Should_preserve_encoding_utf8_bom()
        {
            Should_preserve_encoding(new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        private void Should_preserve_encoding(Encoding encoding)
        {
            Assert.AreEqual(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), _commands.Module.FilesEncoding);

            string filePath = Path.Combine(_referenceRepository.Module.WorkingDir, Path.GetRandomFileName());

            try
            {
                File.WriteAllText(filePath, "Hello↔world", encoding);

                UITest.RunForm<FormEditor>(
                    () =>
                    {
                        using FormEditor formEditor = new(_commands, filePath, showWarning: false);
                        formEditor.ShowDialog();
                    },
                    form =>
                    {
                        FileViewerInternal fileViewerInternal = form.GetTestAccessor().FileViewer.GetTestAccessor().FileViewerInternal;
                        fileViewerInternal.SetText(fileViewerInternal.GetText() + "!", openWithDifftool: null);
                        form.GetTestAccessor().SaveChanges();
                        return Task.CompletedTask;
                    });

                Assert.That(File.ReadAllBytes(filePath), Is.EquivalentTo(encoding.GetPreamble().Concat(encoding.GetBytes("Hello↔world!"))));
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}
