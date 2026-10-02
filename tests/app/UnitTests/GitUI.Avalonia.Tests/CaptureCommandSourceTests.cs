using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitExtensions.Extensibility.Git;
using GitUI;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CaptureCommandSourceTests
{
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
