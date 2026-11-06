using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using GitUI.CommandsDialogs;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class SearchControlEventTimingTests
{
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Text_changes_should_notify_the_owner_synchronously_once_after_the_value_changes(bool nativeTextBox)
    {
        SearchControl<string> search = new(_ => [], _ => { });
        using IDisposable lifetime = search;
        List<string> changes = [];
        search.TextChanged += (_, _) => changes.Add(search.Text);
        TextBox textBox = search.FindControl<TextBox>("txtSearchBox")!;

        SetText("first");
        changes.Should().Equal("first");
        SetText("second");
        changes.Should().Equal("first", "second");
        SetText("second");
        changes.Should().Equal(new[] { "first", "second" }, "assigning the same text does not raise a source change");

        Dispatcher.UIThread.RunJobs();
        changes.Should().Equal(new[] { "first", "second" }, "Avalonia's later rendered TextChanged must not duplicate the source notification");

        return;

        void SetText(string text)
        {
            if (nativeTextBox)
            {
                textBox.Text = text;
            }
            else
            {
                search.Text = text;
            }
        }
    }

    [AvaloniaTest]
    public void Selecting_a_candidate_should_notify_changed_text_before_the_original_entered_event()
    {
        SearchControl<string> search = new(_ => [], _ => { });
        using IDisposable lifetime = search;
        List<string> events = [];
        search.TextChanged += (_, _) => events.Add("changed:" + search.Text);
        search.OnTextEntered += () => events.Add("entered:" + search.Text);
        ListBox results = search.FindControl<ListBox>("listBoxSearchResult")!;
        results.ItemsSource = new[] { "candidate" };
        results.SelectedIndex = 0;

        search.FindControl<TextBox>("txtSearchBox")!.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyUpEvent,
            Key = Key.Enter,
        });

        events.Should().Equal("changed:candidate", "entered:candidate");
        results.IsVisible.Should().BeFalse();
        Dispatcher.UIThread.RunJobs();
        events.Should().Equal("changed:candidate", "entered:candidate");
    }
}
