using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs;
using GitUI.SpellChecker;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class EditNetSpellTests
{
    private string _originalDictionary = null!;
    private bool _originalMarkIllFormedLines;
    private bool _originalProvideAutocompletion;

    [SetUp]
    public void SetUp()
    {
        _originalDictionary = AppSettings.Dictionary;
        _originalMarkIllFormedLines = AppSettings.MarkIllFormedLinesInCommitMsg;
        _originalProvideAutocompletion = AppSettings.ProvideAutocompletion;
        AppSettings.Dictionary = "en-US";
        AppSettings.MarkIllFormedLinesInCommitMsg = true;
        AppSettings.ProvideAutocompletion = true;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.Dictionary = _originalDictionary;
        AppSettings.MarkIllFormedLinesInCommitMsg = _originalMarkIllFormedLines;
        AppSettings.ProvideAutocompletion = _originalProvideAutocompletion;
    }

    [AvaloniaTest]
    public void EditNetSpell_should_use_the_deployed_dictionary_and_offer_corrections()
    {
        EditNetSpell control = new();
        EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
        control.Text = "This sentnce contains a misspeling.";

        control.CheckSpelling();

        File.Exists(Path.Combine(accessor.DictionaryDirectory, "en-US.dic")).Should().BeTrue();
        accessor.MisspelledWords.Select(range => control.Text[range.Start..range.End])
            .Should().Contain(["sentnce", "misspeling"]);

        control.CaretIndex = control.Text.IndexOf("sentnce", StringComparison.Ordinal) + 2;
        accessor.OpenContextMenu();

        accessor.ContextMenu.Items.OfType<MenuItem>().Select(item => item.Header?.ToString())
            .Should().Contain(["sentence", "Add to dictionary", "Dictionary"]);
    }

    [AvaloniaTest]
    public void EditNetSpell_should_preserve_the_original_editor_boundary_and_line_marks()
    {
        AppSettings.Dictionary = "None";
        EditNetSpell control = new();
        int textAssigned = 0;
        control.TextAssigned += (_, _) => textAssigned++;
        control.Text = new string('a', 51) + "\n\nbody";
        control.CaretIndex = 53;

        control.CheckSpelling();

        textAssigned.Should().Be(1);
        control.LineCount().Should().Be(3);
        control.LineLength(0).Should().Be(51);
        control.CurrentLine.Should().Be(3);
        control.CurrentColumn.Should().Be(1);
        control.GetTestAccessor().IllFormedLines.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new TextPos(50, 51));

        control.Text = "abcdef";
        control.SelectionStart = 1;
        control.SelectionLength = 3;
        control.SelectionStart = 2;
        control.SelectionLength.Should().Be(3);
        control.SelectedText.Should().Be("cde");

        control.SelectionStart = 4;
        control.SelectionLength = 0;
        control.ReplaceLine(0, "uvwxyz");
        control.CaretIndex.Should().Be(4);
        control.Text.Should().Be("uvwxyz");
        control.Text = "\nbody";
        control.CaretIndex = 0;
        control.CurrentColumn.Should().Be(1);
    }

    [AvaloniaTest]
    public void FormCommit_should_host_the_same_name_spell_check_editor()
    {
        FormCommit form = new();

        form.GetTestAccessor().Message.Should().BeOfType<EditNetSpell>();
    }

    [AvaloniaTest]
    public void EditNetSpell_should_retain_the_original_translation_keys()
    {
        EditNetSpell control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);
        control.TranslateItems(translation);

        (string Field, string Text)[] expected =
        [
            ("_addToDictionaryText", "Add to dictionary"),
            ("_autoCompletionText", "Provide auto completion"),
            ("_copyMenuItemText", "Copy"),
            ("_cutMenuItemText", "Cut"),
            ("_deleteMenuItemText", "Delete"),
            ("_dictionaryText", "Dictionary"),
            ("_ignoreWordText", "Ignore word"),
            ("_markIllFormedLinesText", "Mark ill formed lines"),
            ("_pasteMenuItemText", "Paste"),
            ("_removeWordText", "Remove word"),
            ("_selectAllMenuItemText", "Select all"),
        ];
        foreach ((string field, string text) in expected)
        {
            translation.Received(1).AddTranslationItem(nameof(EditNetSpell), field, "Text", text);
        }
    }

    [AvaloniaTest]
    public async Task EditNetSpell_should_render_spell_check_marks_after_the_typing_delay()
    {
        EditNetSpell control = new()
        {
            Text = "This sentnce contains a misspeling.",
        };
        Window window = new()
        {
            Width = 400,
            Height = 200,
            Content = control,
        };

        try
        {
            window.Show();
            control.CheckSpelling();
            await Task.Delay(300);
            Dispatcher.UIThread.RunJobs();
            window.CaptureRenderedFrame().Should().NotBeNull();

            EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
            accessor.MisspelledWords.Should().NotBeEmpty();
            accessor.RenderedMisspellingCount.Should().Be(accessor.MisspelledWords.Count);
        }
        finally
        {
            window.Close();
        }
    }
}
