using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using GitCommands;
using GitUI.Editor;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ContinuousScrollEventManagerTests
{
    [Test]
    public void Scroll_should_require_exact_alt_when_automatic_scrolling_is_disabled()
    {
        DateTime currentTime = new(2026, 7, 28, 12, 0, 0);
        ContinuousScrollEventManager manager = CreateManager(
            automaticContinuousScroll: false,
            automaticContinuousScrollDelay: 600,
            () => currentTime);
        int raised = 0;
        manager.BottomScrollReached += (_, _) => raised++;

        manager.RaiseBottomScrollReached(KeyModifiers.None).Should().BeFalse();
        manager.RaiseBottomScrollReached(KeyModifiers.Alt | KeyModifiers.Control).Should().BeFalse();
        manager.RaiseBottomScrollReached(KeyModifiers.Alt).Should().BeTrue();

        raised.Should().Be(1);
    }

    [Test]
    public void Scroll_should_not_require_alt_when_automatic_scrolling_is_enabled()
    {
        ContinuousScrollEventManager manager = CreateManager(
            automaticContinuousScroll: true,
            automaticContinuousScrollDelay: 600,
            () => new DateTime(2026, 7, 28, 12, 0, 0));
        int raised = 0;
        manager.TopScrollReached += (_, _) => raised++;

        manager.RaiseTopScrollReached(KeyModifiers.None).Should().BeTrue();

        raised.Should().Be(1);
    }

    [Test]
    public void Scroll_delay_should_throttle_both_directions()
    {
        DateTime currentTime = new(2026, 7, 28, 12, 0, 0);
        ContinuousScrollEventManager manager = CreateManager(
            automaticContinuousScroll: true,
            automaticContinuousScrollDelay: 600,
            () => currentTime);
        int topRaised = 0;
        int bottomRaised = 0;
        manager.TopScrollReached += (_, _) => topRaised++;
        manager.BottomScrollReached += (_, _) => bottomRaised++;

        manager.RaiseBottomScrollReached(KeyModifiers.None).Should().BeTrue();
        currentTime = currentTime.AddMilliseconds(599);
        manager.RaiseTopScrollReached(KeyModifiers.None).Should().BeFalse();
        currentTime = currentTime.AddMilliseconds(1);
        manager.RaiseTopScrollReached(KeyModifiers.None).Should().BeTrue();

        bottomRaised.Should().Be(1);
        topRaised.Should().Be(1);
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void FileViewer_should_gate_text_and_image_edge_wheels_through_the_manager()
    {
        bool originalAutomaticContinuousScroll = AppSettings.AutomaticContinuousScroll;
        int originalDelay = AppSettings.AutomaticContinuousScrollDelay;
        Window? window = null;
        try
        {
            AppSettings.AutomaticContinuousScroll = false;
            AppSettings.AutomaticContinuousScrollDelay = 0;
            FileViewer viewer = new();
            viewer.TextEditor.Text = "one line";
            window = new Window
            {
                Width = 500,
                Height = 300,
                Content = viewer,
            };
            window.Show();
            int bottomReached = 0;
            viewer.BottomScrollReached += (_, _) => bottomReached++;

            Point textPoint = viewer.TextEditor.TranslatePoint(new Point(10, 10), window)
                ?? throw new InvalidOperationException("The text viewer was not attached.");
            window.MouseWheel(textPoint, new Vector(0, -1), RawInputModifiers.None);
            bottomReached.Should().Be(0);
            window.MouseWheel(textPoint, new Vector(0, -1), RawInputModifiers.Alt);
            bottomReached.Should().Be(1);

            FileViewer.TestAccessor accessor = viewer.GetTestAccessor();
            accessor.RaiseContinuousScroll(-1, KeyModifiers.None).Should().BeFalse();
            bottomReached.Should().Be(1);
            accessor.RaiseContinuousScroll(-1, KeyModifiers.Alt).Should().BeTrue();
            bottomReached.Should().Be(2);

            AppSettings.AutomaticContinuousScroll = true;
            accessor.RaiseContinuousScroll(-1, KeyModifiers.Shift).Should().BeFalse();
            bottomReached.Should().Be(2);
            accessor.RaiseContinuousScroll(-1, KeyModifiers.None).Should().BeTrue();
            bottomReached.Should().Be(3);
        }
        finally
        {
            window?.Close();
            AppSettings.AutomaticContinuousScroll = originalAutomaticContinuousScroll;
            AppSettings.AutomaticContinuousScrollDelay = originalDelay;
        }
    }

    private static ContinuousScrollEventManager CreateManager(
        bool automaticContinuousScroll,
        int automaticContinuousScrollDelay,
        Func<DateTime> getCurrentTime)
        => new(
            () => automaticContinuousScroll,
            () => automaticContinuousScrollDelay,
            getCurrentTime);
}
