using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitUI;
using GitUI.AutoCompletion;
using GitUI.SpellChecker;
using Microsoft.VisualStudio.Threading;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class AutoCompletionTests
{
    private string _originalApplicationExecutablePath = null!;
    private bool _originalProvideAutocompletion;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        WinFormsShims.ShimHost.Clipboard = new StubClipboard(string.Empty);
        AppSettings.TestAccessor settingsAccessor = AppSettings.GetTestAccessor();
        _originalApplicationExecutablePath = settingsAccessor.ApplicationExecutablePath;
        settingsAccessor.ApplicationExecutablePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "GitExtensions.Avalonia.exe");
        _originalProvideAutocompletion = AppSettings.ProvideAutocompletion;
        AppSettings.ProvideAutocompletion = true;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.GetTestAccessor().ApplicationExecutablePath = _originalApplicationExecutablePath;
        AppSettings.ProvideAutocompletion = _originalProvideAutocompletion;
    }

    [Test]
    public void AutoCompleteWord_should_match_prefix_and_camel_humps()
    {
        AutoCompleteWord word = new("CommitMessageMetadataProvider");

        word.Matches("commit").Should().BeTrue();
        word.Matches("CMMP").Should().BeTrue();
        word.Matches("cmmp").Should().BeTrue();
        word.Matches("CMMX").Should().BeFalse();
        word.Should().Be(new AutoCompleteWord("CommitMessageMetadataProvider"));
    }

    [Test]
    public async Task CommitMessageMetadataProvider_should_return_the_original_keywords()
    {
        IAutoCompleteProvider provider = new CommitMessageMetadataProvider();

        IEnumerable<AutoCompleteWord> words = await provider.GetAutoCompleteWordsAsync(CancellationToken.None);

        words.Select(word => word.Word).Should().Equal(
            "Co-authored-by: ",
            "Signed-off-by: ",
            "BREAKING CHANGE: ",
            "Reviewed-by: ",
            "Tested-by: ");
    }

    [AvaloniaTest]
    public async Task EditNetSpell_should_show_navigate_and_accept_auto_completion()
    {
        EditNetSpell control = new()
        {
            Height = 336,
        };
        control.AddAutoCompleteProvider(new StaticAutoCompleteProvider("BranchParser", "BranchPolicy"));
        Window window = new()
        {
            Width = 386,
            Height = 336,
            Content = control,
        };

        try
        {
            window.Show();
            control.Text = "Br";
            control.CaretIndex = control.Text.Length;
            EditNetSpell.TestAccessor accessor = control.GetTestAccessor();

            await accessor.ShowAutoCompleteAsync(calledByUser: true);

            accessor.IsAutoCompleteVisible.Should().BeTrue();
            accessor.AutoComplete.ItemCount.Should().Be(2);
            accessor.AutoComplete.SelectedIndex.Should().Be(0);

            accessor.CloseAutoComplete();
            accessor.KeyDown(Key.Space, KeyModifiers.Control).Should().BeTrue();
            accessor.IsAutoCompleteVisible.Should().BeTrue();
            accessor.KeyDown(Key.Down, KeyModifiers.None).Should().BeTrue();
            accessor.KeyDown(Key.Enter, KeyModifiers.None).Should().BeTrue();

            control.Text.Should().Be("BranchPolicy");
            accessor.IsAutoCompleteVisible.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task EditNetSpell_should_close_auto_completion_when_focus_leaves_the_editor()
    {
        EditNetSpell control = new()
        {
            Height = 336,
        };
        control.AddAutoCompleteProvider(new StaticAutoCompleteProvider("BranchParser", "BranchPolicy"));
        Button otherControl = new();
        Window window = new()
        {
            Width = 386,
            Height = 360,
            Content = new StackPanel
            {
                Children =
                {
                    control,
                    otherControl,
                },
            },
        };

        try
        {
            window.Show();
            control.Text = "Br";
            control.CaretIndex = control.Text.Length;
            EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
            await accessor.ShowAutoCompleteAsync(calledByUser: true);

            otherControl.Focus();
            Dispatcher.UIThread.RunJobs();

            accessor.IsAutoCompleteVisible.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task EditNetSpell_should_cancel_a_pending_provider_when_words_are_refreshed()
    {
        CancellationAwareAutoCompleteProvider provider = new();
        EditNetSpell control = new();
        control.AddAutoCompleteProvider(provider);
        Window window = new()
        {
            Width = 386,
            Height = 336,
            Content = control,
        };

        try
        {
            window.Show();
            await provider.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

            control.RefreshAutoCompleteWords();

            await provider.Cancelled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void EditNetSpell_should_close_auto_completion_when_the_setting_is_disabled()
    {
        EditNetSpell control = new();
        Window window = new()
        {
            Width = 386,
            Height = 336,
            Content = control,
        };

        try
        {
            window.Show();
            EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
            accessor.ShowAutoCompleteForCapture([new AutoCompleteWord("BranchParser")]);
            accessor.IsAutoCompleteVisible.Should().BeTrue();

            AppSettings.ProvideAutocompletion = false;
            accessor.ToggleAutoCompletion();

            accessor.IsAutoCompleteVisible.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void EditNetSpell_should_preserve_the_original_paste_and_shift_enter_routes()
    {
        WinFormsShims.ShimHost.Clipboard = new StubClipboard("first\vsecond");
        EditNetSpell control = new()
        {
            Text = "prefix ",
        };
        Window window = new()
        {
            Width = 386,
            Height = 336,
            Content = control,
        };

        try
        {
            window.Show();
            EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
            accessor.TextBox.Focus().Should().BeTrue();
            Dispatcher.UIThread.RunJobs();
            control.CaretIndex = control.Text.Length;

            accessor.KeyDown(Key.V, KeyModifiers.Control).Should().BeTrue();

            control.Text.Should().Be("prefix first\nsecond");
            accessor.KeyDown(Key.Enter, KeyModifiers.Shift).Should().BeTrue();
            control.Text.Should().Be("prefix first\nsecond\n");

            WinFormsShims.ShimHost.Clipboard = new StubClipboard("third\vfourth");
            control.CaretIndex = control.Text.Length;
            accessor.OpenContextMenu();
            MenuItem paste = accessor.ContextMenu.Items
                .OfType<MenuItem>()
                .Single(item => item.Header?.ToString() == "Paste");
            paste.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            control.Text.Should().Be("prefix first\nsecond\nthird\nfourth");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task EditNetSpell_should_keep_the_auto_complete_list_inside_the_editor_at_the_bottom_edge()
    {
        EditNetSpell control = new();
        Window window = new()
        {
            Width = 180,
            Height = 90,
            Content = control,
        };

        try
        {
            window.Show();
            control.Text = "top\nline\nline\nBo";
            control.CaretIndex = control.Text.Length;
            EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
            accessor.ShowAutoCompleteForCapture(
                Enumerable.Range(0, 20).Select(index => new AutoCompleteWord($"BottomEdgeWord{index:00}")).ToList());
            Dispatcher.UIThread.RunJobs();

            Rect bounds = accessor.AutoComplete.Bounds;
            bounds.Left.Should().BeGreaterThanOrEqualTo(0);
            bounds.Top.Should().BeGreaterThanOrEqualTo(0);
            bounds.Right.Should().BeLessThanOrEqualTo(control.Bounds.Width + 0.01);
            bounds.Bottom.Should().BeLessThanOrEqualTo(control.Bounds.Height + 0.01);
            bounds.Height.Should().BeLessThan(20 * 15, "the overflowing list must use its vertical scroller");
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class StaticAutoCompleteProvider(params string[] words) : IAutoCompleteProvider
    {
        public Task<IEnumerable<AutoCompleteWord>> GetAutoCompleteWordsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(words.Select(word => new AutoCompleteWord(word)));
        }
    }

    private sealed class CancellationAwareAutoCompleteProvider : IAutoCompleteProvider
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Cancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<IEnumerable<AutoCompleteWord>> GetAutoCompleteWordsAsync(CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Cancelled.TrySetResult();
                throw;
            }

            return [];
        }
    }

    private sealed class StubClipboard(string text) : WinFormsShims.IClipboard
    {
        private string _text = text;

        public void SetText(string value) => _text = value;

        public string GetText() => _text;

        public bool ContainsText() => _text.Length > 0;
    }
}
