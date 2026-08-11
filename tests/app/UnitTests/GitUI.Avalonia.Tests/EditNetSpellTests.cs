using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.SpellChecker;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class EditNetSpellTests
{
    private string _originalApplicationExecutablePath = null!;
    private string _originalDictionary = null!;
    private bool _originalMarkIllFormedLines;
    private bool _originalProvideAutocompletion;

    [SetUp]
    public void SetUp()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new Microsoft.VisualStudio.Threading.JoinableTaskContext();
        AppSettings.TestAccessor settingsAccessor = AppSettings.GetTestAccessor();
        _originalApplicationExecutablePath = settingsAccessor.ApplicationExecutablePath;
        settingsAccessor.ApplicationExecutablePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "GitExtensions.Avalonia.exe");
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
        AppSettings.GetTestAccessor().ApplicationExecutablePath = _originalApplicationExecutablePath;
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

        File.Exists(Path.Combine(accessor.DictionaryPath, "en-US.dic")).Should().BeTrue();
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

        control.Text = "subject\n\nbody";
        control.ChangeTextColor(2, 0, 4, System.Drawing.Color.Red);
        control.GetTestAccessor().ForegroundRanges.Should().ContainSingle()
            .Which.Should().Be(new SpellCheckAdorner.TextColorRange(new TextPos(9, 13), Colors.Red));

        control.EnsureEmptyLine(addBullet: true, afterLine: 2);
        control.Text.Should().Be($"subject\n\n{Environment.NewLine} - body");
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
            await Task.Delay(300);
            Dispatcher.UIThread.RunJobs();
            control.CheckSpelling();
            control.ChangeTextColor(0, 0, 4, System.Drawing.Color.Red);
            control.GetTestAccessor().ForegroundRanges.Should().ContainSingle();
            Dispatcher.UIThread.RunJobs();
            window.CaptureRenderedFrame().Should().NotBeNull();

            EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
            accessor.ForegroundRanges.Should().ContainSingle();
            accessor.MisspelledWords.Should().NotBeEmpty();
            accessor.RenderedMisspellingCount.Should().Be(accessor.MisspelledWords.Count);
            accessor.RenderedForegroundRangeCount.Should().Be(1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void EditNetSpell_should_use_the_original_adapted_spell_check_colors()
    {
        EditNetSpell control = new();
        Window window = new()
        {
            Width = 400,
            Height = 200,
            Content = control,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
            Color background = accessor.TextBox.Background
                .Should().BeAssignableTo<ISolidColorBrush>().Which.Color;
            System.Drawing.Color drawingBackground = System.Drawing.Color.FromArgb(
                background.A,
                background.R,
                background.G,
                background.B);
            System.Drawing.Color expectedMark =
                System.Drawing.Color.FromArgb(120, 255, 255, 0).AdaptBackColor();
            System.Drawing.Color expectedWave =
                System.Drawing.Color.Red.AdaptForeColor(drawingBackground);

            accessor.IllFormedMarkColor.Should().Be(ToMediaColor(expectedMark));
            accessor.SpellingWaveColor.Should().Be(ToMediaColor(expectedWave));
        }
        finally
        {
            window.Close();
        }
    }

    private static Color ToMediaColor(System.Drawing.Color color)
        => Color.FromArgb(color.A, color.R, color.G, color.B);
}
