using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary;
using ResourceManager;
using Point = Avalonia.Point;

namespace GitUI.SpellChecker;

[DefaultEvent("TextChanged")]
public sealed partial class EditNetSpell : GitModuleControl
{
    public event EventHandler? TextAssigned;

    private readonly TranslationString _cutMenuItemText = new("Cut");
    private readonly TranslationString _copyMenuItemText = new("Copy");
    private readonly TranslationString _pasteMenuItemText = new("Paste");
    private readonly TranslationString _deleteMenuItemText = new("Delete");
    private readonly TranslationString _selectAllMenuItemText = new("Select all");
    private readonly TranslationString _addToDictionaryText = new("Add to dictionary");
    private readonly TranslationString _ignoreWordText = new("Ignore word");
    private readonly TranslationString _removeWordText = new("Remove word");
    private readonly TranslationString _dictionaryText = new("Dictionary");
    private readonly TranslationString _markIllFormedLinesText = new("Mark ill formed lines");
    private readonly TranslationString _autoCompletionText = new("Provide auto completion");

    private static WordDictionary? _wordDictionary;

    private readonly DispatcherTimer _spellCheckTimer;
    private readonly Spelling _spelling;
    private readonly IWordAtCursorExtractor _wordAtCursorExtractor = new WordAtCursorExtractor();
    private int _contextMenuTextIndex = -1;

    public EditNetSpell()
    {
        InitializeComponent();

        _spelling = new Spelling
        {
            ShowDialog = false,
            IgnoreAllCapsWords = true,
            IgnoreWordsWithDigits = true,
            MaxSuggestions = 5,
        };
        _spelling.MisspelledWord += SpellingMisspelledWord;

        _spellCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250),
        };
        _spellCheckTimer.Tick += SpellCheckTimerTick;

        TextBox.TextWrapping = AppSettings.MessageEditorWordWrap.Value
            ? TextWrapping.Wrap
            : TextWrapping.NoWrap;
        TextBox.TextChanged += TextBoxTextChanged;
        TextBox.PropertyChanged += TextBoxPropertyChanged;
        TextBox.PointerPressed += TextBoxPointerPressed;
        TextBox.LayoutUpdated += (_, _) => SpellCheckAdorner.InvalidateVisual();
        SpellCheckContextMenu.Opening += SpellCheckContextMenuOpening;
        PropertyChanged += EditNetSpellPropertyChanged;
        AttachedToVisualTree += (_, _) => CheckSpelling();
        DetachedFromVisualTree += (_, _) => _spellCheckTimer.Stop();

        SpellCheckAdorner.TextBox = TextBox;
        InitializeComplete();
    }

    public event EventHandler? TextChanged;

    public event EventHandler? SelectionChanged;

    [AllowNull]
    public string Text
    {
        get => TextBox.Text ?? string.Empty;
        set
        {
            TextBox.Text = value ?? string.Empty;
            TextAssigned?.Invoke(this, EventArgs.Empty);
        }
    }

    public string WatermarkText
    {
        get => TextBox.PlaceholderText ?? string.Empty;
        set => TextBox.PlaceholderText = value;
    }

    public int CaretIndex
    {
        get => TextBox.CaretIndex;
        set => TextBox.CaretIndex = Math.Clamp(value, 0, Text.Length);
    }

    public int SelectionStart
    {
        get => Math.Min(TextBox.SelectionStart, TextBox.SelectionEnd);
        set
        {
            int length = SelectionLength;
            int start = Math.Clamp(value, 0, Text.Length);
            TextBox.SelectionStart = start;
            TextBox.SelectionEnd = Math.Clamp(start + length, 0, Text.Length);
        }
    }

    public int SelectionLength
    {
        get => Math.Abs(TextBox.SelectionEnd - TextBox.SelectionStart);
        set => TextBox.SelectionEnd = Math.Clamp(SelectionStart + value, 0, Text.Length);
    }

    public string? SelectedText
    {
        get => TextBox.SelectedText;
        set => TextBox.SelectedText = value ?? string.Empty;
    }

    public int CurrentColumn
    {
        get
        {
            int previousNewLine = CaretIndex == 0 ? -1 : Text.LastIndexOf('\n', CaretIndex - 1);
            return CaretIndex - previousNewLine;
        }
    }

    public int CurrentLine => Text.Take(CaretIndex).Count(character => character == '\n') + 1;

    public string Line(int line) => GetLines()[line];

    public int LineLength(int line) => line < LineCount() ? Line(line).Length : 0;

    public int LineCount() => GetLines().Length;

    public void ReplaceLine(int line, string withText)
    {
        int caret = SelectionStart + SelectionLength;
        (int start, int length) = GetLineBounds(line);
        ReplaceText(start, length, withText);
        CaretIndex = caret;
    }

    public void SelectAll() => TextBox.SelectAll();

    public bool Focus() => TextBox.Focus();

    public void CheckSpelling()
    {
        _spellCheckTimer.Stop();
        SpellCheckAdorner.MisspelledWords.Clear();
        SpellCheckAdorner.IllFormedLines.Clear();
        SpellCheckAdorner.MarkFirstLineBlank = false;

        string text = Text;
        if (text.Length < 5000 && TryLoadDictionary())
        {
            try
            {
                _spelling.Text = text;
                _spelling.SpellCheck();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        MarkIllFormedLines();
        SpellCheckAdorner.InvalidateVisual();
    }

    private static string DictionaryDirectory
        => Path.Combine(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory), "Dictionaries");

    private IDetachedSettings Settings
        => TryGetUICommands(out IGitUICommands? commands)
            ? (commands.Module.GetEffectiveSettings() as DistributedSettings ?? AppSettings.SettingsContainer).Detached()
            : AppSettings.SettingsContainer.Detached();

    private void TextBoxTextChanged(object? sender, EventArgs e)
    {
        SpellCheckAdorner.MisspelledWords.Clear();
        SpellCheckAdorner.IllFormedLines.Clear();
        SpellCheckAdorner.InvalidateVisual();
        TextChanged?.Invoke(this, EventArgs.Empty);

        if (Text.Length >= 4 && Settings.Dictionary is not "None")
        {
            _spellCheckTimer.Stop();
            _spellCheckTimer.Start();
        }
    }

    private void TextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextBox.CaretIndexProperty
            || e.Property == TextBox.SelectionStartProperty
            || e.Property == TextBox.SelectionEndProperty)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void TextBoxPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(TextBox).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            _contextMenuTextIndex = GetTextIndex(e.GetPosition(TextBox));
        }
    }

    private void EditNetSpellPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsEnabledProperty)
        {
            TextBox.IsReadOnly = !IsEnabled;
        }
    }

    private void SpellCheckTimerTick(object? sender, EventArgs e) => CheckSpelling();

    private void SpellingMisspelledWord(object? sender, SpellingEventArgs e)
        => SpellCheckAdorner.MisspelledWords.Add(new TextPos(e.TextIndex, e.TextIndex + e.Word.Length));

    private void SpellCheckContextMenuOpening(object? sender, CancelEventArgs e)
    {
        TextBox.Focus();
        int textIndex = _contextMenuTextIndex >= 0 ? _contextMenuTextIndex : CaretIndex;
        _contextMenuTextIndex = -1;

        List<object> items = [];
        AddWordSuggestions(items, textIndex);
        items.Add(CreateMenuItem(_cutMenuItemText.Text, (_, _) => TextBox.Cut(), SelectionLength > 0 && !TextBox.IsReadOnly));
        items.Add(CreateMenuItem(_copyMenuItemText.Text, (_, _) => TextBox.Copy(), SelectionLength > 0));
        items.Add(CreateMenuItem(_pasteMenuItemText.Text, (_, _) => TextBox.Paste(), !TextBox.IsReadOnly));
        items.Add(CreateMenuItem(_deleteMenuItemText.Text, (_, _) => SelectedText = string.Empty, SelectionLength > 0 && !TextBox.IsReadOnly));
        items.Add(CreateMenuItem(_selectAllMenuItemText.Text, (_, _) => SelectAll()));
        items.Add(new Separator());
        items.Add(CreateDictionaryMenu());
        items.Add(new Separator());
        items.Add(CreateMenuItem(
            _markIllFormedLinesText.Text,
            (_, _) =>
            {
                AppSettings.MarkIllFormedLinesInCommitMsg = !AppSettings.MarkIllFormedLinesInCommitMsg;
                CheckSpelling();
            },
            isChecked: AppSettings.MarkIllFormedLinesInCommitMsg,
            isCheckable: true));

        SpellCheckContextMenu.ItemsSource = items;
    }

    private void AddWordSuggestions(List<object> items, int textIndex)
    {
        if (!AppSettings.ProvideAutocompletion || !TryLoadDictionary())
        {
            return;
        }

        try
        {
            _spelling.Text = Text;
            _spelling.WordIndex = _spelling.GetWordIndexFromTextIndex(textIndex);
            if (_spelling.CurrentWord.Length == 0 || _spelling.TestWord())
            {
                return;
            }

            _spelling.Suggest();
            (int start, int length) = _wordAtCursorExtractor.GetWordBounds(Text, textIndex);
            string word = _spelling.CurrentWord;
            foreach (string suggestion in _spelling.Suggestions)
            {
                items.Add(CreateMenuItem(suggestion, (_, _) => ReplaceText(start, length, suggestion), fontWeight: FontWeight.Bold));
            }

            items.Add(CreateMenuItem(_addToDictionaryText.Text, (_, _) => AddToDictionary(word)));
            items.Add(CreateMenuItem(_ignoreWordText.Text, (_, _) => IgnoreWord(word)));
            items.Add(CreateMenuItem(_removeWordText.Text, (_, _) => ReplaceText(start, length, string.Empty)));
            items.Add(new Separator());
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private MenuItem CreateDictionaryMenu()
    {
        string selectedDictionary = Settings.Dictionary;
        MenuItem dictionaryMenu = new() { Header = _dictionaryText.Text };
        List<object> dictionaries =
        [
            CreateMenuItem("None", (_, _) => SelectDictionary("None"), isChecked: selectedDictionary is "None", isCheckable: true),
        ];

        try
        {
            dictionaries.AddRange(Directory
                .EnumerateFiles(DictionaryDirectory, "*.dic", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => name is not null)
                .Select(name => (object)CreateMenuItem(
                    name!,
                    (_, _) => SelectDictionary(name!),
                    isChecked: selectedDictionary == name,
                    isCheckable: true)));
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }

        dictionaryMenu.ItemsSource = dictionaries;
        return dictionaryMenu;
    }

    private void SelectDictionary(string dictionary)
    {
        DistributedSettings settings = TryGetUICommands(out IGitUICommands? commands)
            ? commands.Module.GetLocalSettings() as DistributedSettings ?? AppSettings.SettingsContainer
            : AppSettings.SettingsContainer;
        settings.Detached().Dictionary = dictionary;
        _wordDictionary = null;
        CheckSpelling();
    }

    private bool TryLoadDictionary()
    {
        string dictionary = Settings.Dictionary;
        if (dictionary is "None")
        {
            return false;
        }

        string dictionaryFile = Path.Combine(DictionaryDirectory, dictionary + ".dic");
        if (!File.Exists(dictionaryFile))
        {
            return false;
        }

        if (_wordDictionary is null || _wordDictionary.DictionaryFile != dictionaryFile)
        {
            _wordDictionary = new WordDictionary
            {
                DictionaryFile = dictionaryFile,
            };
        }

        _spelling.Dictionary = _wordDictionary;
        return true;
    }

    private void AddToDictionary(string word)
    {
        if (TryLoadDictionary())
        {
            _spelling.Dictionary.Add(word);
            CheckSpelling();
        }
    }

    private void IgnoreWord(string word)
    {
        if (!_spelling.IgnoreList.Contains(word))
        {
            _spelling.IgnoreList.Add(word);
        }

        CheckSpelling();
    }

    private void ReplaceText(int start, int length, string replacement)
    {
        string text = Text;
        if (start < 0 || start > text.Length)
        {
            return;
        }

        length = Math.Clamp(length, 0, text.Length - start);
        Text = string.Concat(text.AsSpan(0, start), replacement, text.AsSpan(start + length));
        CaretIndex = start + replacement.Length;
        CheckSpelling();
    }

    private void MarkIllFormedLines()
    {
        if (!AppSettings.MarkIllFormedLinesInCommitMsg)
        {
            return;
        }

        string[] lines = GetLines();
        int textIndex = 0;
        for (int line = 0; line < lines.Length; line++)
        {
            int maximumLength = line switch
            {
                0 => 50,
                1 => 0,
                _ => 72,
            };
            if (lines[line].Length > maximumLength)
            {
                SpellCheckAdorner.IllFormedLines.Add(
                    new TextPos(textIndex + maximumLength, textIndex + lines[line].Length));
            }

            textIndex += lines[line].Length + 1;
        }

        SpellCheckAdorner.MarkFirstLineBlank = Text.Length > 1 && lines.Length > 0 && lines[0].Length == 0;
    }

    private int GetTextIndex(Point point)
    {
        return SpellCheckAdorner.GetTextIndex(point);
    }

    private bool TryGetUICommands([NotNullWhen(true)] out IGitUICommands? commands)
    {
        if (TryGetUICommandsDirect(out commands))
        {
            return true;
        }

        commands = this.GetLogicalAncestors().OfType<IGitUICommandsSource>().FirstOrDefault()?.UICommands;
        return commands is not null;
    }

    private string[] GetLines() => Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    private (int start, int length) GetLineBounds(int line)
    {
        string text = Text;
        int start = 0;
        for (int index = 0; index < line; index++)
        {
            int nextLine = text.IndexOf('\n', start);
            if (nextLine < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            start = nextLine + 1;
        }

        int end = text.IndexOf('\n', start);
        if (end < 0)
        {
            end = text.Length;
        }

        int length = end - start;
        if (length > 0 && text[start + length - 1] == '\r')
        {
            length--;
        }

        return (start, length);
    }

    private static MenuItem CreateMenuItem(
        string text,
        EventHandler<Avalonia.Interactivity.RoutedEventArgs> click,
        bool isEnabled = true,
        bool isChecked = false,
        bool isCheckable = false,
        FontWeight? fontWeight = null)
    {
        MenuItem item = new()
        {
            Header = text,
            IsEnabled = isEnabled,
            ToggleType = isCheckable ? MenuItemToggleType.CheckBox : MenuItemToggleType.None,
            IsChecked = isChecked,
            FontWeight = fontWeight ?? FontWeight.Normal,
        };
        item.Click += click;
        return item;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(EditNetSpell control)
    {
        public TextBox TextBox => control.TextBox;

        public IReadOnlyList<TextPos> MisspelledWords => control.SpellCheckAdorner.MisspelledWords;

        public IReadOnlyList<TextPos> IllFormedLines => control.SpellCheckAdorner.IllFormedLines;

        public ContextMenu ContextMenu => control.SpellCheckContextMenu;

        public string DictionaryDirectory => EditNetSpell.DictionaryDirectory;

        public int RenderedMisspellingCount => control.SpellCheckAdorner.RenderedMisspellingCount;

        public void OpenContextMenu() => control.SpellCheckContextMenuOpening(control.SpellCheckContextMenu, new CancelEventArgs());
    }
}
