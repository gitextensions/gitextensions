using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitUI;
using GitUI.AutoCompletion;
using GitUI.SpellChecker;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
public sealed class AutoCompletionTests
{
    private bool _originalProvideAutocompletion;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _originalProvideAutocompletion = AppSettings.ProvideAutocompletion;
        AppSettings.ProvideAutocompletion = true;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.ProvideAutocompletion = _originalProvideAutocompletion;
    }

    [Test]
    public void AutoCompleteWord_should_match_prefix_and_camel_humps()
    {
        AutoCompleteWord word = new("CommitMessageMetadataProvider");

        word.Matches("commit").Should().BeTrue();
        word.Matches("CMMP").Should().BeTrue();
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

            accessor.MoveAutoCompleteSelection(Avalonia.Input.Key.Down);
            accessor.AcceptAutoComplete();

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

    private sealed class StaticAutoCompleteProvider(params string[] words) : IAutoCompleteProvider
    {
        public Task<IEnumerable<AutoCompleteWord>> GetAutoCompleteWordsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(words.Select(word => new AutoCompleteWord(word)));
        }
    }
}
