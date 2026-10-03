using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Compat;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CaptureCommandSourceTests
{
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Capture_tree_should_read_the_actual_XHTML_caption_and_URI_for_both_renderers(bool nativeHeader)
    {
        XhtmlTextBlock block = new() { Name = "RevisionInfo" };
        block.SetTabStops([48, 96], nativeHeader ? [48, 96] : null);
        block.SetXHTMLText("Author:\t<a href='gitext://author/one'>office affinity</a><br/>Child:\t<a href='gitext://revision/two'>second</a>");
        Window window = new() { Width = 360, Height = 80, Content = block };
        window.Show();
        try
        {
            window.UpdateLayout();
            new AvaloniaControlTreeReader(block, 1).ReadPrimary(block, new PixelSize(360, 80)).Root.Text
                .Should().Be("Author:\toffice affinity|||gitext://author/one\nChild:\tsecond|||gitext://revision/two");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Capture_preparation_should_preserve_existing_child_sources_and_initialize_missing_sources_once()
    {
        IGitUICommandsSource existing = Substitute.For<IGitUICommandsSource>();
        existing.UICommands.Returns(Substitute.For<IGitUICommands>());
        IGitUICommandsSource capture = Substitute.For<IGitUICommandsSource>();
        capture.UICommands.Returns(Substitute.For<IGitUICommands>());
        GitModuleControl initialized = new() { UICommandsSource = existing };
        GitModuleControl uninitialized = new();
        StackPanel root = new() { Children = { initialized, uninitialized } };
        int initializations = 0;
        uninitialized.UICommandsSourceSet += (_, _) => initializations++;

        ParityScreenshotTests.SetCaptureUICommandsSource(root, capture);
        ParityScreenshotTests.SetCaptureUICommandsSource(root, capture);

        initialized.UICommandsSource.Should().BeSameAs(existing);
        uninitialized.UICommandsSource.Should().BeSameAs(capture);
        initializations.Should().Be(1);
    }
}
