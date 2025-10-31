using System.Text;
using CommonTestUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Editor;

namespace GitExtensions.UITests.CommandsDialogs;

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
        _referenceRepository = new ReferenceRepository();
        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
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
                    ClassicAssert.False(form.GetTestAccessor().HasChanges);

                    FileViewerInternal fileViewerInternal = form.GetTestAccessor().FileViewer.GetTestAccessor().FileViewerInternal;
                    fileViewerInternal.SetText(fileViewerInternal.GetText() + "!", openWithDifftool: null);

                    ClassicAssert.True(form.GetTestAccessor().HasChanges);

                    form.GetTestAccessor().SaveChanges();

                    ClassicAssert.False(form.GetTestAccessor().HasChanges);

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
                    ClassicAssert.False(form.GetTestAccessor().HasChanges);

                    FileViewerInternal fileViewerInternal = form.GetTestAccessor().FileViewer.GetTestAccessor().FileViewerInternal;
                    fileViewerInternal.GetTestAccessor().CurrentViewPositionCache.GetTestAccessor().SetCurrentIdentification("hello-world");
                    fileViewerInternal.SetText(fileViewerInternal.GetText() + "!", openWithDifftool: null, ViewMode.Text, true, "hello-world");

                    ClassicAssert.True(form.GetTestAccessor().HasChanges);

                    form.GetTestAccessor().SaveChanges();

                    ClassicAssert.False(form.GetTestAccessor().HasChanges);

                    ClassicAssert.AreEqual(lineNumberDefault, fileViewerInternal.CurrentFileLine());

                    return Task.CompletedTask;
                });
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Ignore("Test cannot pass because _commands.Module.FilesEncoding has a preamble which is always used by FormEditor.SaveChanges")]
    [Test]
    public void Should_preserve_encoding_utf8()
    {
        Should_preserve_encoding(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    [Test]
    public void Should_preserve_encoding_utf8_bom()
    {
        Should_preserve_encoding(new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private void Should_preserve_encoding(Encoding encoding)
    {
        ClassicAssert.AreEqual(_commands.Module.FilesEncoding.ToString(), UTF8Encoding.Default.ToString());
        ClassicAssert.That(_commands.Module.FilesEncoding.GetPreamble().SequenceEqual(UTF8Encoding.Default.GetPreamble())
            || _commands.Module.FilesEncoding.GetPreamble().SequenceEqual(encoding.GetPreamble()));

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

            ClassicAssert.That(File.ReadAllBytes(filePath), Is.EquivalentTo(encoding.GetPreamble().Concat(encoding.GetBytes("Hello↔world!"))));
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
