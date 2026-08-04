using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RevisionGridSupportTests
{
    [Test]
    public void Navigation_history_should_walk_backward_forward_and_clear_forward_on_push()
    {
        ObjectId first = Id('1');
        ObjectId second = Id('2');
        ObjectId third = Id('3');
        NavigationHistory history = new();

        history.Push(first);
        history.Push(second);
        history.Push(third);

        history.NavigateBackward().Should().Be(second);
        history.NavigateBackward().Should().Be(first);
        history.NavigateForward().Should().Be(second);

        history.Push(third);

        history.CanNavigateForward.Should().BeFalse();
        history.NavigateBackward().Should().Be(second);
    }

    [Test]
    public void Parent_child_navigation_history_should_reverse_the_last_direction()
    {
        ObjectId child = Id('1');
        ObjectId parent = Id('2');
        ObjectId selected = default;
        ParentChildNavigationHistory? history = null;
        history = new ParentChildNavigationHistory(objectId =>
        {
            selected = objectId;
            history!.RevisionsSelectionChanged();
        });

        history.NavigateToParent(child, parent);

        selected.Should().Be(parent);
        history.HasPreviousChild.Should().BeTrue();

        history.NavigateToPreviousChild(parent);

        selected.Should().Be(child);
        history.HasPreviousParent.Should().BeTrue();
    }

    [Test]
    public void Visible_row_range_should_enumerate_and_compare_like_the_original()
    {
        VisibleRowRange range = new(fromIndex: 3, count: 4);

        range.Should().Equal(3, 4, 5, 6);
        range.Contains(2).Should().BeFalse();
        range.Contains(3).Should().BeTrue();
        range.Contains(6).Should().BeTrue();
        range.Contains(7).Should().BeFalse();
        range.Equals(new VisibleRowRange(3, 4)).Should().BeTrue();
        range.ToString().Should().Be("[3, 6] 4 rows");
    }

    [AvaloniaTest]
    public void Quick_search_should_select_matching_revisions_wrap_and_show_status()
    {
        int originalTimeout = AppSettings.RevisionGridQuickSearchTimeout;
        try
        {
            AppSettings.RevisionGridQuickSearchTimeout = 60_000;
            ListBox revisions = new()
            {
                ItemsSource = new[]
                {
                    Revision('1', "first"),
                    Revision('2', "target one"),
                    Revision('3', "target two"),
                },
                SelectedIndex = 0,
            };
            SolidColorBrush successBrush = new(Colors.Green);
            SolidColorBrush errorBrush = new(Colors.Red);
            revisions.Resources.Add("GitExtensionsToolTipForegroundBrush", successBrush);
            revisions.Resources.Add("GitExtensionsErrorForegroundBrush", errorBrush);
            Grid overlay = new();
            overlay.Children.Add(revisions);
            QuickSearchProvider provider = new(revisions, overlay, () => string.Empty);
            Window window = new() { Width = 500, Height = 200, Content = overlay };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                TextInputEventArgs input = new()
                {
                    RoutedEvent = InputElement.TextInputEvent,
                    Text = "target",
                };

                provider.OnTextInput(input);

                input.Handled.Should().BeTrue();
                revisions.SelectedIndex.Should().Be(1);
                Border status = overlay.Children.OfType<Border>().Single();
                status.IsVisible.Should().BeTrue();
                TextBlock statusText = (TextBlock)status.Child!;
                statusText.Text.Should().EndWith("target");
                statusText.Foreground.Should().BeSameAs(successBrush);

                provider.NextResult(down: true);
                revisions.SelectedIndex.Should().Be(2);
                provider.NextResult(down: true);
                revisions.SelectedIndex.Should().Be(1);

                input.Text = "missing";
                provider.OnTextInput(input);

                revisions.SelectedIndex.Should().Be(1);
                statusText.Foreground.Should().BeSameAs(errorBrush);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            AppSettings.RevisionGridQuickSearchTimeout = originalTimeout;
        }
    }

    [Test]
    public void Index_watcher_should_report_changed_when_invalid_repository_paths_disable_watching()
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        IGitModule module = Substitute.For<IGitModule>();
        source.UICommands.Returns(commands);
        commands.Module.Returns(module);
        module.WorkingDirGitDir.Returns(Path.Combine(Path.GetTempPath(), "gitextensions-missing-index-watcher"));
        module.IsValidGitWorkingDir().Returns(false);
        using IndexWatcher watcher = new(source);
        List<bool> changes = [];
        watcher.Changed += (_, e) => changes.Add(e.IsIndexChanged);

        watcher.Reset();

        changes.Should().Equal(true);
    }

    [AvaloniaTest]
    public void Menu_caption_and_multiline_indicator_should_use_native_noninteractive_controls()
    {
        MenuItem caption = new();
        MenuItem owner = new();

        MenuUtil.SetAsCaptionMenuItem(caption, owner);

        caption.IsEnabled.Should().BeFalse();
        caption.Focusable.Should().BeFalse();
        caption.IsHitTestVisible.Should().BeFalse();
        caption.Classes.Should().Contain("gitextensions-menu-caption");

        MultilineIndicator indicator = new();
        indicator.Update(Revision('1', "subject", multiline: true));
        indicator.IsVisible.Should().BeTrue();
        indicator.Width.Should().Be(26);
        indicator.Height.Should().Be(11);

        indicator.Update(Revision('2', "subject", multiline: false));
        indicator.IsVisible.Should().BeFalse();
    }

    private static ObjectId Id(char value)
        => ObjectId.Parse(new string(value, 40));

    private static GitRevision Revision(char id, string subject, bool multiline = false)
        => new(Id(id))
        {
            Subject = subject,
            Author = "Author",
            HasMultiLineMessage = multiline,
        };
}
