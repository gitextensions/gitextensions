using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitExtensions.ParityCapture;
using GitExtUtils.GitUI.Theming;
using GitUI.AutoCompletion;
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
        Window window = new() { Content = control };
        window.Show();
        control.Text = "This sentnce contains a misspeling.";

        try
        {
            control.CheckSpelling();

            File.Exists(Path.Combine(accessor.DictionaryPath, "en-US.dic")).Should().BeTrue();
            accessor.MisspelledWords.Select(range => control.Text[range.Start..range.End])
                .Should().Contain(["sentnce", "misspeling"]);

            control.CaretIndex = control.Text.IndexOf("sentnce", StringComparison.Ordinal) + 2;
            accessor.OpenContextMenu();

            accessor.ContextMenu.Items.OfType<MenuItem>().Select(item => item.Header?.ToString())
                .Should().Contain(["sentence", "Add to dictionary", "Dictionary"]);
            accessor.ContextMenu.Items.OfType<MenuItem>()
                .Where(item => item.Header?.ToString() is "Cut" or "Copy" or "Paste" or "Delete")
                .Should().OnlyContain(item => item.IsEnabled);

            MenuItem correction = accessor.ContextMenu.Items.OfType<MenuItem>()
                .Single(item => item.Header?.ToString() == "sentence");
            correction.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            control.Text.Should().StartWith("This sentence contains");
            control.SelectionStart.Should().BeLessThanOrEqualTo(control.Text.Length);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void EditNetSpell_should_execute_the_original_dictionary_and_marking_menu_actions()
    {
        EditNetSpell control = new() { Text = "subject\n\nbody" };
        EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
        accessor.OpenContextMenu();

        MenuItem dictionary = accessor.ContextMenu.Items.OfType<MenuItem>()
            .Single(item => item.Header?.ToString() == "Dictionary");
        MenuItem none = dictionary.Items.OfType<MenuItem>()
            .Single(item => item.Header?.ToString() == "None");
        none.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        AppSettings.Dictionary.Should().Be("None");

        MenuItem marking = accessor.ContextMenu.Items.OfType<MenuItem>()
            .Single(item => item.Header?.ToString() == "Mark ill formed lines");
        bool previousMarking = AppSettings.MarkIllFormedLinesInCommitMsg;
        marking.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        AppSettings.MarkIllFormedLinesInCommitMsg.Should().Be(!previousMarking);
    }

    [AvaloniaTest]
    public void EditNetSpell_should_use_the_native_placeholder_for_the_original_watermark_contract()
    {
        EditNetSpell control = new();

        control.WatermarkText = "Commit message";

        control.GetTestAccessor().TextBox.PlaceholderText.Should().Be("Commit message");
        control.Text.Should().BeEmpty();
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
    public void EditNetSpell_should_allow_its_owner_to_extend_the_context_menu()
    {
        EditNetSpell control = new();
        MenuItem ownerItem = new() { Header = "Owner action" };
        control.ContextMenuPopulating += (_, menu) => ((IList<object>)menu.ItemsSource!).Insert(0, ownerItem);

        EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
        accessor.OpenContextMenu();

        accessor.ContextMenu.Items.OfType<object>().First().Should().BeSameAs(ownerItem);
    }

    [AvaloniaTest]
    public void EditNetSpell_should_notify_the_open_popup_when_rebuilding_its_context_menu()
    {
        EditNetSpell control = new() { Text = "sentnce" };
        EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
        System.Collections.Specialized.INotifyCollectionChanged items = accessor.ContextMenu.ItemsSource.Should()
            .BeAssignableTo<System.Collections.Specialized.INotifyCollectionChanged>().Subject;
        int notificationCount = 0;
        items.CollectionChanged += (_, _) => notificationCount++;

        accessor.OpenContextMenu();

        notificationCount.Should().BeGreaterThan(0);
        accessor.ContextMenu.Items.Should().NotBeEmpty();
    }

    [AvaloniaTest]
    public void EditNetSpell_should_materialize_the_rebuilt_context_menu_in_its_popup()
    {
        EditNetSpell control = new() { Text = "sentnce" };
        EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
        Window window = new() { Content = control };
        window.Show();
        accessor.TextBox.Focus();

        accessor.TextBox.RaiseEvent(new Avalonia.Input.ContextRequestedEventArgs());
        accessor.ContextMenu.Open(accessor.TextBox);
        Dispatcher.UIThread.RunJobs();

        accessor.ContextMenu.IsOpen.Should().BeTrue();
        Control overlayHost = window.GetVisualDescendants().OfType<Control>()
            .Single(control => control.GetType().Name == "OverlayPopupHost");
        overlayHost.Bounds.Width.Should().BeGreaterThan(2);
        overlayHost.GetVisualDescendants().OfType<MenuItem>().Should().NotBeEmpty();
        accessor.ContextMenu.Close();
        window.Close();
    }

    [AvaloniaTest]
    public void Capture_driver_should_open_the_spelling_menu_at_the_same_misspelled_word_as_WinForms()
    {
        EditNetSpell control = new() { Text = "sentnce Br" };
        EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
        control.CaretIndex = control.Text.Length;
        int originalCaretIndex = control.CaretIndex;
        Window window = new() { Width = 386, Height = 336, Content = control };
        window.Show();

        try
        {
            using (AvaloniaControlStateDriver.Apply(
                       control,
                       new CaptureStatePlan
                       {
                           Id = "spelling.open",
                           Kind = CaptureStateKind.MenuOpen,
                           TargetField = "SpellCheckContextMenu",
                       }))
            {
                control.CaretIndex.Should().Be(2);
                window.Bounds.Height.Should().BeGreaterThanOrEqualTo(900);
                Control popupHost = window.GetVisualDescendants().OfType<Control>()
                    .Single(candidate => candidate.GetType().Name == "OverlayPopupHost");
                if (OperatingSystem.IsWindows())
                {
                    popupHost.Bounds.Width.Should().Be(206);
                }
                else
                {
                    popupHost.Bounds.Width.Should().BeGreaterThan(2);
                }

                popupHost.Bounds.Height.Should().Be(374);
                CaptureNode textBox = new AvaloniaControlTreeReader(control, renderScale: 1)
                    .ReadPrimary(control, new Avalonia.PixelSize(386, 336))
                    .Root.Children.Single(child => child.FieldName == "TextBox");
                textBox.Focused.Should().BeTrue();
                accessor.ContextMenu.Items.OfType<MenuItem>()
                    .Select(item => item.Header?.ToString())
                    .Should().Contain("sentence");
            }

            control.CaretIndex.Should().Be(originalCaretIndex);
        }
        finally
        {
            accessor.ContextMenu.Close();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Capture_tree_should_project_the_source_editor_and_integral_autocomplete_list()
    {
        EditNetSpell control = new() { Text = "Br" };
        EditNetSpell.TestAccessor accessor = control.GetTestAccessor();
        control.CaretIndex = control.Text.Length;
        Window window = new() { Width = 386, Height = 336, Content = control };
        window.Show();

        try
        {
            accessor.ShowAutoCompleteForCapture(
            [
                new AutoCompleteWord("BranchParser"),
                new AutoCompleteWord("BranchPolicy"),
            ]);
            Dispatcher.UIThread.RunJobs();

            CaptureNode root = new AvaloniaControlTreeReader(control, renderScale: 1)
                .ReadPrimary(control, new Avalonia.PixelSize(386, 336))
                .Root;
            root.BorderStyle.Should().Be("None");
            root.Anchor.Should().Equal("Top", "Left");
            root.Dock.Should().Be("None");
            root.AutoSize.Should().BeFalse();
            root.Alignment.Should().BeNull();
            root.Children.Select(child => child.FieldName).Should().Equal("TextBox", "AutoComplete");

            CaptureNode autoComplete = root.Children.Single(child => child.FieldName == "AutoComplete");
            if (OperatingSystem.IsWindows())
            {
                autoComplete.BoundsDip.Should().Be(new CaptureRectangleF { X = 14, Y = 16, Width = 76, Height = 32 });
                autoComplete.ClientSizeDip.Should().Be(new CaptureSizeF { Width = 74, Height = 30 });
            }
            else
            {
                autoComplete.BoundsDip.X.Should().BeGreaterThanOrEqualTo(0);
                autoComplete.BoundsDip.Y.Should().BeGreaterThanOrEqualTo(0);
                autoComplete.BoundsDip.Width.Should().BeGreaterThan(2);
                autoComplete.BoundsDip.Height.Should().Be(32);
                autoComplete.ClientSizeDip.Should().Be(new CaptureSizeF
                {
                    Width = autoComplete.BoundsDip.Width - 2,
                    Height = 30,
                });
            }

            autoComplete.Text.Should().Be("BranchParser");
            autoComplete.Selected.Should().BeTrue();
        }
        finally
        {
            accessor.CloseAutoComplete();
            window.Close();
        }
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

        translation.Received(1).AddTranslationItem(nameof(EditNetSpell), "TextBox", "Text", string.Empty);
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
