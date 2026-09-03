using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI.AutoCompletion;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary;
using ResourceManager;
using ContextMenuStrip = GitUI.Compat.WinFormsControls.ContextMenuStrip;
using DrawingColor = System.Drawing.Color;
using Point = Avalonia.Point;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.SpellChecker;

[DefaultEvent("TextChanged")]
public partial class EditNetSpell : GitModuleControl, IDisposable
{
#pragma warning disable SX1309 // Preserve the original Designer field names for port parity.
    private readonly DispatcherTimer SpellCheckTimer;
    private readonly DispatcherTimer AutoCompleteTimer;
    private readonly ToolTip AutoCompleteToolTip = new();
    private readonly DispatcherTimer AutoCompleteToolTipTimer;
#pragma warning restore SX1309

    public event EventHandler? TextAssigned;

    /// <summary>
    ///  Raised after all built-in items have been added to the spell-check context menu,
    ///  allowing consumers to append additional items.
    /// </summary>
    public event EventHandler<ContextMenuStrip>? ContextMenuPopulating;

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
    private SpellCheckAdorner _customUnderlines = null!;
    private readonly Spelling _spelling;

    private static WordDictionary? _wordDictionary;

    private CancellationTokenSource _autoCompleteCancellationTokenSource = new();
    private readonly List<IAutoCompleteProvider> _autoCompleteProviders = [];
    private AsyncLazy<IEnumerable<AutoCompleteWord>?>? _autoCompleteListTask;
    private bool _autoCompleteWasUserActivated;
    private bool _disableAutoCompleteTriggerOnTextUpdate = true; // only popup on key press

    // Avalonia routes navigation directly to the native list instead of sending virtual key strings.
    private readonly HashSet<Key> _keysToSendToAutoComplete =
    [
        Key.Down,
        Key.Up,
        Key.PageUp,
        Key.PageDown,
        Key.End,
        Key.Home,
    ];
    private readonly IWordAtCursorExtractor _wordAtCursorExtractor = new WordAtCursorExtractor();
    private readonly System.Collections.ObjectModel.ObservableCollection<object> _spellCheckContextMenuItems = [];
    private int _contextMenuTextIndex = -1;
    private WinFormsShims.Font _textBoxFont;

    // Avalonia resolves the WinForms-shaped point font at the native TextBox boundary.
    public WinFormsShims.Font TextBoxFont
    {
        get => _textBoxFont;
        set
        {
            _textBoxFont = value;
            TextBox.FontFamily = new FontFamily(value.Name);
            TextBox.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(value.Size);
            TextBox.FontStyle = value.Italic ? Avalonia.Media.FontStyle.Italic : Avalonia.Media.FontStyle.Normal;
            TextBox.FontWeight = value.Bold ? FontWeight.Bold : FontWeight.Normal;
        }
    }

    // Avalonia's spelling and validation adorners do not change the TextBox undo stack.
    public bool IsUndoInProgress;

    public EditNetSpell()
    {
        InitializeComponent();
        _customUnderlines = SpellCheckAdorner;
        SpellCheckContextMenu.Items.Clear();
        SpellCheckContextMenu.ItemsSource = _spellCheckContextMenuItems;

        _textBoxFont = WinFormsShims.SystemFonts.DefaultFont ?? new WinFormsShims.Font("Segoe UI", 9F);
        TextBoxFont = _textBoxFont;
        MistakeFont = new WinFormsShims.Font(
            _textBoxFont.FontFamily,
            _textBoxFont.Size,
            WinFormsShims.FontStyle.Underline);

        _spelling = new Spelling
        {
            ShowDialog = false,
            IgnoreAllCapsWords = true,
            IgnoreWordsWithDigits = true,
            MaxSuggestions = 5,
        };

        SpellCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250),
        };
        SpellCheckTimer.Tick += SpellCheckTimerTick;

        AutoCompleteTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200),
        };
        AutoCompleteTimer.Tick += AutoCompleteTimer_Tick;
        AutoCompleteToolTipTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2),
        };
        AutoCompleteToolTipTimer.Tick += AutoCompleteToolTipTimer_Tick;
        AutoComplete.ItemTemplate = new FuncDataTemplate<AutoCompleteWord>((word, _) =>
            new TextBlock { Text = word?.Word ?? string.Empty });

        TextBox.TextWrapping = AppSettings.MessageEditorWordWrap.Value
            ? TextWrapping.Wrap
            : TextWrapping.NoWrap;
        TextBox.KeyDown += TextBox_KeyDown;
        TextBox.KeyUp += TextBox_KeyUp;
        TextBox.TextInput += TextBox_KeyPress;
        TextBox.GotFocus += TextBox_GotFocus;
        TextBox.LostFocus += TextBoxLeave;
        TextBox.LostFocus += TextBox_LostFocus;
        TextBox.PointerPressed += TextBox_MouseDown;
        TextBox.ContextRequested += TextBox_ContextRequested;
        AutoComplete.PointerReleased += AutoComplete_Click;
        TextBox.LayoutUpdated += (_, _) => SpellCheckAdorner.InvalidateVisual();
        SpellCheckContextMenu.Opening += SpellCheckContextMenuOpening;
        AttachedToVisualTree += EditNetSpellAttachedToVisualTree;
        DetachedFromVisualTree += EditNetSpellDetachedFromVisualTree;

        _customUnderlines.TextBox = TextBox;
        InitializeComplete();
    }

    public override void AddTranslationItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(EditNetSpell), nameof(TextBox), "Text", string.Empty);
    }

    [AllowNull]
    public string Text
    {
        get => TextBox.Text ?? string.Empty;
        set
        {
            HideWatermark();
            EvaluateForecolor();
            TextBox.Text = value ?? string.Empty;
            ShowWatermark();
            OnTextAssigned();
        }
    }

    public event EventHandler? TextChanged;

    public void EvaluateForecolor()
    {
        // In dark mode the background color is set to White, but still reported as SystemColors.Window (or adjusted)
        // The Forecolor must be changed manually
    }

    private void OnTextAssigned()
    {
        TextAssigned?.Invoke(this, EventArgs.Empty);
    }

    public string Line(int line) => GetLines()[line];

    public void ReplaceLine(int line, string withText)
    {
        int caret = SelectionStart + SelectionLength;
        (int start, int length) = GetLineBounds(line);
        ReplaceText(start, length, withText);
        CaretIndex = caret;
    }

    public int LineLength(int line) => line < LineCount() ? Line(line).Length : 0;

    public int CaretIndex
    {
        get => TextBox.CaretIndex;
        set => TextBox.CaretIndex = Math.Clamp(value, 0, Text.Length);
    }

    public int LineCount() => GetLines().Length;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public WinFormsShims.Font MistakeFont { get; set; }

    public int CurrentColumn
    {
        get
        {
            int previousNewLine = CaretIndex == 0 ? -1 : Text.LastIndexOf('\n', CaretIndex - 1);
            return CaretIndex - previousNewLine;
        }
    }

    public int CurrentLine => Text.Take(CaretIndex).Count(character => character == '\n') + 1;

    public event EventHandler? SelectionChanged;

    private void EditNetSpellEnabledChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsEnabledProperty)
        {
            TextBox.IsReadOnly = !IsEnabled;
        }
    }

    private bool _isWatermarkShowing;
    private string _watermarkText = "";

    public string WatermarkText
    {
        get => _watermarkText;
        set
        {
            HideWatermark();
            _watermarkText = value;
            TextBox.PlaceholderText = value;
            ShowWatermark();
        }
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

    public bool Focus() => TextBox.Focus();

    protected DistributedSettings Settings
        => TryGetUICommands(out IGitUICommands? commands)
            ? commands.Module.GetEffectiveSettings() as DistributedSettings ?? AppSettings.SettingsContainer
            : AppSettings.SettingsContainer;

    public void SelectAll() => TextBox.SelectAll();

    // Avalonia controls have no WinForms RuntimeLoad event, so the first visual-tree attachment
    // invokes the original source-named runtime boundary.
    protected virtual void OnRuntimeLoad()
    {
        TextBox.PropertyChanged += TextBox_SelectionChanged;
        TextBox.TextChanged += TextBoxTextChanged;
        TextBox.DoubleTapped += TextBox_DoubleClick;

        PropertyChanged += EditNetSpellEnabledChanged;

        ShowWatermark();

        ToggleAutoCompletion();

        //
        // spelling
        //
        _spelling.ReplacedWord += SpellingReplacedWord;
        _spelling.DeletedWord += SpellingDeletedWord;
        _spelling.MisspelledWord += SpellingMisspelledWord;

        //
        // wordDictionary
        //
        LoadDictionary();

        SpellCheckTimer.Start();
    }

    private static string DictionaryDirectory
    {
        get
        {
            string configuredDirectory = AppSettings.GetDictionaryDir();
            if (Directory.Exists(configuredDirectory))
            {
                return configuredDirectory;
            }

            // Cross-platform test and app-host layouts deploy dictionaries beside the entry assembly.
            return Path.Combine(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory), "Dictionaries");
        }
    }

    private MenuItem AddContextMenuItem(string text, EventHandler<RoutedEventArgs> eventHandler)
    {
        MenuItem menuItem = CreateMenuItem(text, (sender, e) => eventHandler(sender, e));
        _spellCheckContextMenuItems.Add(menuItem);
        return menuItem;
    }

    private void AddContextMenuSeparator()
    {
        _spellCheckContextMenuItems.Add(new Separator());
    }

    private void AddDictionaries()
    {
        try
        {
            string selectedDictionary = Settings.Detached().Dictionary;
            MenuItem dictionaryToolStripMenuItem = new() { Header = _dictionaryText.Text };
            _spellCheckContextMenuItems.Add(dictionaryToolStripMenuItem);

            List<object> dictionaries = [];
            MenuItem noDicToolStripMenuItem = new()
            {
                Header = "None",
                IsChecked = selectedDictionary is "None",
                ToggleType = MenuItemToggleType.CheckBox,
            };
            noDicToolStripMenuItem.Click += DicToolStripMenuItemClick;
            dictionaries.Add(noDicToolStripMenuItem);

            foreach (string fileName in Directory.GetFiles(DictionaryDirectory, "*.dic", SearchOption.TopDirectoryOnly))
            {
                string dictionary = Path.GetFileNameWithoutExtension(fileName);
                MenuItem dicToolStripMenuItem = new()
                {
                    Header = dictionary,
                    IsChecked = selectedDictionary == dictionary,
                    ToggleType = MenuItemToggleType.CheckBox,
                };
                dicToolStripMenuItem.Click += DicToolStripMenuItemClick;
                dictionaries.Add(dicToolStripMenuItem);
            }

            dictionaryToolStripMenuItem.ItemsSource = dictionaries;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private void AddWordSuggestions(int pos)
    {
        if (!AppSettings.ProvideAutocompletion)
        {
            return;
        }

        try
        {
            LoadDictionary();
            if (_spelling.Dictionary is null || !File.Exists(_spelling.Dictionary.DictionaryFile))
            {
                return;
            }

            _spelling.Text = Text;
            _spelling.WordIndex = _spelling.GetWordIndexFromTextIndex(pos);
            if (_spelling.CurrentWord.Length == 0 || _spelling.TestWord())
            {
                return;
            }

            // generate suggestions
            _spelling.Suggest();
            foreach (string suggestion in _spelling.Suggestions)
            {
                MenuItem suggestionItem = AddContextMenuItem(suggestion, SuggestionToolStripItemClick);
                suggestionItem.FontWeight = FontWeight.Bold;
            }

            AddContextMenuItem(_addToDictionaryText.Text, AddToDictionaryClick);
            AddContextMenuItem(_ignoreWordText.Text, IgnoreWordClick);
            AddContextMenuItem(_removeWordText.Text, RemoveWordClick);

            if (_spelling.Suggestions.Count > 0)
            {
                AddContextMenuSeparator();
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private void LoadDictionary()
    {
        // Don`t load a dictionary in Design-time
        if (Design.IsDesignMode)
        {
            return;
        }

        IDetachedSettings detachedSettings = Settings.Detached();
        string dictionaryFile = string.Concat(Path.Join(DictionaryDirectory, detachedSettings.Dictionary), ".dic");

        if (_wordDictionary is null || _wordDictionary.DictionaryFile != dictionaryFile)
        {
            _wordDictionary = new WordDictionary
            {
                DictionaryFile = dictionaryFile,
            };
        }

        _spelling.Dictionary = _wordDictionary;
    }

    private void EditNetSpellAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        OnRuntimeLoad();
    }

    private void EditNetSpellDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        TextBox.PropertyChanged -= TextBox_SelectionChanged;
        TextBox.TextChanged -= TextBoxTextChanged;
        TextBox.DoubleTapped -= TextBox_DoubleClick;
        PropertyChanged -= EditNetSpellEnabledChanged;
        _spelling.ReplacedWord -= SpellingReplacedWord;
        _spelling.DeletedWord -= SpellingDeletedWord;
        _spelling.MisspelledWord -= SpellingMisspelledWord;
        SpellCheckTimer.Stop();
        CloseAutoComplete();
        CancelAutoComplete();
    }

    private void ToggleAutoCompletion()
    {
        if (!AppSettings.ProvideAutocompletion || Design.IsDesignMode)
        {
            CloseAutoComplete();
            CancelAutoComplete();
            return;
        }

        InitializeAutoCompleteWordsTask();
        CancellationToken cancellationToken = _autoCompleteCancellationTokenSource.Token;
        AsyncLazy<IEnumerable<AutoCompleteWord>?> autoCompleteListTask = _autoCompleteListTask!;

        ThreadHelper.FileAndForget(async () =>
        {
            IEnumerable<AutoCompleteWord>? words = await autoCompleteListTask.GetValueAsync(cancellationToken);
            await this.SwitchToMainThreadAsync(cancellationToken);
            if (words is not null)
            {
                _spelling.AddAutoCompleteWords(words.Select(word => word.Word));
            }
        });
    }

    private void SpellingMisspelledWord(object? sender, SpellingEventArgs e)
        => _customUnderlines.MisspelledWords.Add(new TextPos(e.TextIndex, e.TextIndex + e.Word.Length));

    public void CheckSpelling()
    {
        SpellCheckTimer.Stop();
        _customUnderlines.MisspelledWords.Clear();
        _customUnderlines.IllFormedLines.Clear();
        _customUnderlines.MarkFirstLineBlank = false;

        string text = Text;

        // Do not check spelling of watermark text
        if (!_isWatermarkShowing && text.Length < 5000)
        {
            try
            {
                LoadDictionary();
                if (_spelling.Dictionary is not null && File.Exists(_spelling.Dictionary.DictionaryFile))
                {
                    _spelling.Text = text;
                    _spelling.SpellCheck();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        MarkLines();
        _customUnderlines.InvalidateVisual();
    }

    private void MarkLines()
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
                _customUnderlines.IllFormedLines.Add(
                    new TextPos(textIndex + maximumLength, textIndex + lines[line].Length));
            }

            textIndex += lines[line].Length + 1;
        }

        _customUnderlines.MarkFirstLineBlank = Text.Length > 1 && lines.Length > 0 && lines[0].Length == 0;
    }

    private void SpellingDeletedWord(object? sender, SpellingEventArgs e)
    {
        int start = SelectionStart;
        int length = SelectionLength;
        ReplaceText(e.TextIndex, e.Word.Length, string.Empty);
        SelectionStart = Math.Min(start, Text.Length);
        SelectionLength = start + length > Text.Length ? 0 : length;
    }

    private void SpellingReplacedWord(object? sender, ReplaceWordEventArgs e)
    {
        int start = SelectionStart;
        int length = SelectionLength;
        ReplaceText(e.TextIndex, e.Word.Length, e.ReplacementWord);
        SelectionStart = Math.Min(start, Text.Length);
        SelectionLength = start + length > Text.Length ? 0 : length;
    }

    private void SpellCheckContextMenuOpening(object? sender, CancelEventArgs e)
    {
        TextBox.Focus();
        int textIndex = _contextMenuTextIndex >= 0 ? _contextMenuTextIndex : CaretIndex;
        _contextMenuTextIndex = -1;

        _spellCheckContextMenuItems.Clear();
        AddWordSuggestions(textIndex);
        AddContextMenuItem(_cutMenuItemText.Text, CutMenuItemClick);
        AddContextMenuItem(_copyMenuItemText.Text, CopyMenuItemdClick);
        AddContextMenuItem(_pasteMenuItemText.Text, PasteMenuItemClick);
        AddContextMenuItem(_deleteMenuItemText.Text, DeleteMenuItemClick);
        AddContextMenuItem(_selectAllMenuItemText.Text, SelectAllMenuItemClick);

        AddContextMenuSeparator();
        AddDictionaries();
        AddContextMenuSeparator();

        MenuItem mi = new()
        {
            Header = _markIllFormedLinesText.Text,
            IsChecked = AppSettings.MarkIllFormedLinesInCommitMsg,
            ToggleType = MenuItemToggleType.CheckBox,
        };
        mi.Click += MarkIllFormedLinesInCommitMsgClick;
        _spellCheckContextMenuItems.Add(mi);

        mi = new MenuItem
        {
            Header = _autoCompletionText.Text,
            IsChecked = AppSettings.ProvideAutocompletion,
            ToggleType = MenuItemToggleType.CheckBox,
        };
        mi.Click += (_, _) =>
        {
            AppSettings.ProvideAutocompletion = !AppSettings.ProvideAutocompletion;
            ToggleAutoCompletion();
        };
        _spellCheckContextMenuItems.Add(mi);

        WinFormsToolStripMenuSizer.Apply(SpellCheckContextMenu);
        ContextMenuPopulating?.Invoke(this, SpellCheckContextMenu);
    }

    private void RemoveWordClick(object? sender, EventArgs e)
    {
        _spelling.DeleteWord();
        CheckSpelling();
    }

    private void IgnoreWordClick(object? sender, EventArgs e)
    {
        _spelling.IgnoreWord();
        CheckSpelling();
    }

    private void AddToDictionaryClick(object? sender, EventArgs e)
    {
        LoadDictionary();
        if (_spelling.Dictionary is not null)
        {
            _spelling.Dictionary.Add(_spelling.CurrentWord);
        }

        CheckSpelling();
    }

    private void MarkIllFormedLinesInCommitMsgClick(object? sender, EventArgs e)
    {
        AppSettings.MarkIllFormedLinesInCommitMsg = !AppSettings.MarkIllFormedLinesInCommitMsg;
        CheckSpelling();
    }

    private void SuggestionToolStripItemClick(object? sender, EventArgs e)
    {
        _spelling.ReplaceWord(((MenuItem)sender!).Header?.ToString() ?? string.Empty);
        CheckSpelling();
    }

    private void DicToolStripMenuItemClick(object? sender, EventArgs e)
    {
        // if a Module is available, then always change the "repository local" setting
        // it will set a dictionary only for this Module (repository) locally
        DistributedSettings settings = TryGetUICommands(out IGitUICommands? commands)
            ? commands.Module.GetLocalSettings() as DistributedSettings ?? Settings
            : AppSettings.SettingsContainer;
        settings.Detached().Dictionary = ((MenuItem)sender!).Header?.ToString() ?? "None";
        _wordDictionary = null;
        LoadDictionary();
        CheckSpelling();
    }

    private void SpellCheckTimerTick(object? sender, EventArgs e) => CheckSpelling();

    private void TextBoxTextChanged(object? sender, EventArgs e)
    {
        if (!_disableAutoCompleteTriggerOnTextUpdate)
        {
            _disableAutoCompleteTriggerOnTextUpdate = true; // only popup on key press

            // Reset when timer is already running
            AutoCompleteTimer.Stop();
            AutoCompleteTimer.Start();
        }

        _customUnderlines.MisspelledWords.Clear();
        _customUnderlines.IllFormedLines.Clear();
        _customUnderlines.ForegroundRanges.Clear();
        _customUnderlines.InvalidateVisual();
        TextChanged?.Invoke(this, EventArgs.Empty);

        if (Text.Length >= 4 && Settings.Detached().Dictionary is not "None")
        {
            SpellCheckTimer.Stop();
            SpellCheckTimer.Start();
        }
    }

    private void TextBoxLeave(object? sender, RoutedEventArgs e)
    {
        if (!AutoComplete.IsKeyboardFocusWithin)
        {
            CloseAutoComplete();
        }
    }

    private void TextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        // Avalonia key events already bubble from the inner TextBox through this control.
    }

    private bool _skipSelectionUndo;

    private void UndoHighlighting()
    {
        if (!_skipSelectionUndo)
        {
            return;
        }

        // Avalonia renders validation colors in an adorner, so no formatting actions enter the native undo stack.
        _skipSelectionUndo = false;
    }

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (ProcessCmdKey(e.Key, e.KeyModifiers))
        {
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers == KeyModifiers.None
            && _keysToSendToAutoComplete.Contains(e.Key)
            && AutoComplete.IsVisible)
        {
            MoveAutoCompleteSelection(e.Key);
            e.Handled = true;
            return;
        }

        // handle paste from clipboard (Ctrl+V, Shift+Ins)
        if ((e.KeyModifiers == KeyModifiers.Control && e.Key == Key.V)
            || (e.KeyModifiers == KeyModifiers.Shift && e.Key == Key.Insert))
        {
            PasteTextFromClipboard();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Z)
        {
            UndoHighlighting();
        }
        else if (e.KeyModifiers == KeyModifiers.Control
                 && e.Key == Key.Space
                 && AppSettings.ProvideAutocompletion)
        {
            UpdateOrShowAutoComplete(calledByUser: true);
            e.Handled = true;
            return;
        }

        // handle vertical tab (Shift + Enter)
        if (e.KeyModifiers == KeyModifiers.Shift && e.Key == Key.Enter)
        {
            AddNewLine();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Back)
        {
            _disableAutoCompleteTriggerOnTextUpdate = false;
            if (CaretIndex == 0 || Text[CaretIndex - 1].IsSeparator())
            {
                CloseAutoComplete();
            }
        }
    }

    private void PasteTextFromClipboard()
    {
        if (!WinFormsShims.Clipboard.ContainsText())
        {
            return;
        }

        // insert only text with replace vertical tab to line feed
        TextBox.SelectedText = WinFormsShims.Clipboard.GetText().Replace('\v', '\n');
    }

    private void TextBox_KeyPress(object? sender, TextInputEventArgs e)
    {
        // When a character is deleted...
        // Avalonia reports Backspace through KeyDown rather than TextInput, so that branch is handled there.
        if (string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        bool isSeparator = e.Text[^1].IsSeparator();
        _disableAutoCompleteTriggerOnTextUpdate = isSeparator;
        if (isSeparator)
        {
            CloseAutoComplete();
        }
    }

    private void TextBox_SelectionChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextBox.CaretIndexProperty
            || e.Property == TextBox.SelectionStartProperty
            || e.Property == TextBox.SelectionEndProperty)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void TextBox_DoubleClick(object? sender, TappedEventArgs e)
    {
        int textIndex = GetTextIndex(e.GetPosition(TextBox));
        (int start, int length) = _wordAtCursorExtractor.GetWordBounds(Text, textIndex);
        TextBox.SelectionStart = start;
        TextBox.SelectionEnd = start + length;
    }

    private void ShowWatermark()
    {
        _isWatermarkShowing = !TextBox.IsFocused && string.IsNullOrEmpty(TextBox.Text) && _watermarkText.Length > 0;
        TextBox.PlaceholderText = _watermarkText;
    }

    private void HideWatermark()
    {
        _isWatermarkShowing = false;
    }

    private void TextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        ShowWatermark();

        // Avalonia raises LostFocus before the list receives focus, so defer the original ActiveControl check.
        Dispatcher.UIThread.Post(() =>
        {
            if (!AutoComplete.IsKeyboardFocusWithin)
            {
                CloseAutoComplete();
            }
        }, DispatcherPriority.Input);
    }

    private void TextBox_GotFocus(object? sender, RoutedEventArgs e)
    {
        HideWatermark();
    }

    private void CutMenuItemClick(object? sender, RoutedEventArgs e)
    {
        TextBox.Cut();
        CheckSpelling();
    }

    private void CopyMenuItemdClick(object? sender, RoutedEventArgs e)
    {
        TextBox.Copy();
    }

    private void PasteMenuItemClick(object? sender, RoutedEventArgs e)
    {
        if (!WinFormsShims.Clipboard.ContainsText())
        {
            return;
        }

        PasteTextFromClipboard();
        CheckSpelling();
    }

    private void DeleteMenuItemClick(object? sender, RoutedEventArgs e)
    {
        TextBox.SelectedText = string.Empty;
        CheckSpelling();
    }

    private void SelectAllMenuItemClick(object? sender, RoutedEventArgs e)
    {
        TextBox.SelectAll();
    }

    public void ChangeTextColor(int line, int offset, int length, DrawingColor color)
    {
        (int lineStart, int lineLength) = GetLineBounds(line);
        int start = Math.Clamp(lineStart + offset, lineStart, lineStart + lineLength);
        int end = Math.Clamp(start + length, start, lineStart + lineLength);
        _customUnderlines.ForegroundRanges.Add(
            new SpellCheckAdorner.TextColorRange(
                new TextPos(start, end),
                Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B)));
        _customUnderlines.InvalidateVisual();

        // restore old color only if oldPos doesn't intersects with colored selection

        // undoes all recent selections while ctrl-z pressed
        _skipSelectionUndo = true;
    }

    /// <summary>
    /// Make sure this line is empty by inserting a newline at its start.
    /// </summary>
    public void EnsureEmptyLine(bool addBullet, int afterLine)
    {
        int lineLength = LineLength(afterLine);
        if (lineLength > 0)
        {
            string bullet = addBullet ? " - " : string.Empty;
            (int start, _) = GetLineBounds(afterLine);
            string newLine = Environment.NewLine;
            int newCursorPos = start + newLine.Length + bullet.Length + lineLength - 1;
            ReplaceText(start, 0, newLine + bullet);
            CaretIndex = newCursorPos;
        }
    }

    private void ShowAutoCompleteList(IReadOnlyList<AutoCompleteWord> list)
    {
        const double itemHeight = 15;
        const double listBorderWidth = 1;
        const double textRendererOverhang = 7;
        const double verticalScrollBarWidth = 17;

        // TextRenderer includes a seven-pixel glyph overhang in the original ListBox width.
        double width = Math.Max(
            24,
            Math.Ceiling(list.Max(word => WinFormsTextMeasurer.Measure(TextBox, word.Word))) + textRendererOverhang);

        Point cursorPosition = GetCursorPosition();
        double top = cursorPosition.Y;
        double height = (list.Count + 1) * itemHeight;
        if (top + height > Bounds.Height)
        {
            if (Bounds.Height - top > Bounds.Height / 2)
            {
                height = Bounds.Height - top;
            }
            else
            {
                top = Math.Max(0, Bounds.Height - height);

                height = Math.Min(Bounds.Height - top, height);
            }

            width += verticalScrollBarWidth;
        }

        // WinForms ListBox.IntegralHeight reduces SetBounds heights to complete 15-pixel rows.
        double clientHeight = Math.Max(itemHeight, height - (2 * listBorderWidth));
        height = (Math.Floor(clientHeight / itemHeight) * itemHeight) + (2 * listBorderWidth);

        Canvas.SetLeft(AutoComplete, Math.Clamp(cursorPosition.X, 0, Math.Max(0, Bounds.Width - width)));
        Canvas.SetTop(AutoComplete, top);
        AutoComplete.Width = width;
        AutoComplete.Height = Math.Max(itemHeight, height);
        AutoComplete.ItemsSource = list;
        AutoComplete.SelectedIndex = 0;
        AutoComplete.IsVisible = true;
        TextBox.Focus();
    }

    private void MoveAutoCompleteSelection(Key key)
    {
        int count = AutoComplete.ItemCount;
        if (count == 0)
        {
            return;
        }

        int index = Math.Max(0, AutoComplete.SelectedIndex);
        index = key switch
        {
            Key.Up => index == 0 ? count - 1 : index - 1,
            Key.Down => index == count - 1 ? 0 : index + 1,
            Key.PageUp => Math.Max(0, index - 5),
            Key.PageDown => Math.Min(count - 1, index + 5),
            Key.Home => 0,
            Key.End => count - 1,
            _ => index,
        };
        AutoComplete.SelectedIndex = index;
        AutoComplete.ScrollIntoView(index);
        TextBox.Focus();
    }

    public void RefreshAutoCompleteWords()
    {
        if (AppSettings.ProvideAutocompletion)
        {
            InitializeAutoCompleteWordsTask();
        }
    }

    private void InitializeAutoCompleteWordsTask()
    {
        CancelAutoComplete();
        _autoCompleteCancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _autoCompleteCancellationTokenSource.Token;
        _autoCompleteListTask = new AsyncLazy<IEnumerable<AutoCompleteWord>?>(
            async () =>
            {
                await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                Task<IEnumerable<AutoCompleteWord>>[] subTasks =
                    [.. _autoCompleteProviders.Select(provider => provider.GetAutoCompleteWordsAsync(cancellationToken))];
                try
                {
                    IEnumerable<AutoCompleteWord>[] results = await Task.WhenAll(subTasks);
                    return results.SelectMany(result => result).Distinct();
                }
                catch (OperationCanceledException)
                {
                    // WaitAll was cancelled
                    return null;
                }
                catch (Exception)
                {
                    if (subTasks.Any(task => task.IsCanceled))
                    {
                        // At least one task was cancelled
                        return null;
                    }

                    throw;
                }
            },
            ThreadHelper.JoinableTaskFactory);
    }

    public void AddAutoCompleteProvider(IAutoCompleteProvider autoCompleteProvider)
    {
        _autoCompleteProviders.Add(autoCompleteProvider);
    }

    protected bool ProcessCmdKey(Key key, KeyModifiers keyModifiers)
    {
        if (AutoComplete.IsVisible && key is Key.Tab or Key.Enter)
        {
            AcceptAutoComplete();
            return true;
        }

        if (AutoComplete.IsVisible && key == Key.Escape)
        {
            CloseAutoComplete();
            return true;
        }

        return false;
    }

    private string GetWordAtCursor()
    {
        return _wordAtCursorExtractor.Extract(Text, CaretIndex - 1);
    }

    private void CloseAutoComplete()
    {
        AutoComplete.IsVisible = false;
        _autoCompleteWasUserActivated = false;
    }

    private void AcceptAutoComplete(AutoCompleteWord? completionWord = null)
    {
        completionWord ??= AutoComplete.SelectedItem as AutoCompleteWord;
        if (completionWord is null)
        {
            return;
        }

        string word = GetWordAtCursor();
        int start = Math.Max(0, CaretIndex - word.Length);
        TextBox.SelectionStart = start;
        TextBox.SelectionEnd = CaretIndex;
        TextBox.SelectedText = completionWord.Word;
        CaretIndex = start + completionWord.Word.Length;
        CloseAutoComplete();
    }

    private void AddNewLine()
    {
        TextBox.SelectedText = "\n";
    }

    private void UpdateOrShowAutoComplete(bool calledByUser)
    {
        if (TopLevel.GetTopLevel(this) is null && !Design.IsDesignMode)
        {
            return;
        }

        if (_autoCompleteListTask is null || !AppSettings.ProvideAutocompletion)
        {
            return;
        }

        if (!_autoCompleteListTask.IsValueFactoryCompleted)
        {
            _autoCompleteListTask.GetValueAsync(_autoCompleteCancellationTokenSource.Token).Forget();

            if (calledByUser)
            {
                AutoCompleteToolTip.Content = "AutoComplete is not available yet (it is still parsing the changed files).";
                ToolTip.SetTip(TextBox, AutoCompleteToolTip.Content);
                ToolTip.SetIsOpen(TextBox, true);
                AutoCompleteToolTipTimer.Stop();
                AutoCompleteToolTipTimer.Start();
            }

            return;
        }

        AutoCompleteToolTipTimer.Stop();
        ToolTip.SetIsOpen(TextBox, false);

        string word = GetWordAtCursor();
        if (word.Length <= 1 && !calledByUser && !_autoCompleteWasUserActivated)
        {
            CloseAutoComplete();
            return;
        }

        IEnumerable<AutoCompleteWord>? autoCompleteList =
            ThreadHelper.JoinableTaskFactory.Run(_autoCompleteListTask.GetValueAsync);
        IReadOnlyList<AutoCompleteWord> list = autoCompleteList?
            .Where(candidate => candidate.Matches(word))
            .OrderBy(candidate => candidate.Word, StringComparer.CurrentCultureIgnoreCase)
            .ToList()
            ?? [];

        if (list.Count == 0)
        {
            CloseAutoComplete();
            return;
        }

        if (list.Count == 1 && calledByUser)
        {
            AcceptAutoComplete(list[0]);
            return;
        }

        if (calledByUser)
        {
            _autoCompleteWasUserActivated = true;
        }

        // The native list helper applies the source overflow rules:
        // if reduced height is not too small then shrink only
        // if shrinking wasn't acceptable, move higher
        // and reduce height if moving up wasn't enough
        ShowAutoCompleteList(list);
    }

    private Point GetCursorPosition()
    {
        Point position = _customUnderlines.GetTextPosition(CaretIndex);
        int lineStart = CaretIndex == 0 ? 0 : Text.LastIndexOf('\n', CaretIndex - 1) + 1;
        string linePrefix = Text[lineStart..CaretIndex];
        const double richTextBoxTextInset = 1;
        double sourceTextPosition = richTextBoxTextInset
                                    + Math.Ceiling(WinFormsTextMeasurer.Measure(TextBox, linePrefix))
                                    + 2;
        return new Point(sourceTextPosition, position.Y);
    }

    private void AutoComplete_Click(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            AcceptAutoComplete();
        }
    }

    private void AutoCompleteTimer_Tick(object? sender, EventArgs e)
    {
        UpdateOrShowAutoComplete(calledByUser: false);
        AutoCompleteTimer.Stop();
    }

    public void CancelAutoComplete()
    {
        _autoCompleteCancellationTokenSource.Cancel();
        AutoCompleteToolTipTimer.Stop();
        AutoCompleteTimer.Stop();
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

    private void AutoCompleteToolTipTimer_Tick(object? sender, EventArgs e)
    {
        ToolTip.SetIsOpen(TextBox, false);
        AutoCompleteToolTipTimer.Stop();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            CancelAutoComplete();
            SpellCheckTimer.Stop();
            _autoCompleteCancellationTokenSource.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void TextBox_ContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (e.TryGetPosition(TextBox, out Point position))
        {
            _contextMenuTextIndex = GetTextIndex(position);
        }

        SpellCheckContextMenuOpening(SpellCheckContextMenu, new CancelEventArgs());
    }

    private void TextBox_MouseDown(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(TextBox).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            _contextMenuTextIndex = GetTextIndex(e.GetPosition(TextBox));
        }
    }

    private int GetTextIndex(Point point)
    {
        return _customUnderlines.GetTextIndex(point);
    }

    private bool TryGetUICommands([NotNullWhen(true)] out IGitUICommands? commands)
    {
        if (TryGetUICommandsDirect(out commands))
        {
            return true;
        }

        // The Avalonia previewer attaches this control to a command-less design-time form.
        if (Design.IsDesignMode)
        {
            commands = null;
            return false;
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

    // parity-scaffolding: Drives and inspects editor states that the paired capture and headless tests cannot reach through a compositor.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(EditNetSpell control)
    {
        public TextBox TextBox => control.TextBox;

        public IReadOnlyList<TextPos> MisspelledWords => control._customUnderlines.MisspelledWords;

        public IReadOnlyList<TextPos> IllFormedLines => control._customUnderlines.IllFormedLines;

        public IReadOnlyList<SpellCheckAdorner.TextColorRange> ForegroundRanges => control._customUnderlines.ForegroundRanges;

        public ContextMenuStrip ContextMenu => control.SpellCheckContextMenu;

        public ListBox AutoComplete => control.AutoComplete;

        public bool IsAutoCompleteVisible => control.AutoComplete.IsVisible;

        public int AutoCompleteProviderCount => control._autoCompleteProviders.Count;

        public string DictionaryPath => EditNetSpell.DictionaryDirectory;

        public int RenderedMisspellingCount => control._customUnderlines.RenderedMisspellingCount;

        public int RenderedForegroundRangeCount => control._customUnderlines.RenderedForegroundRangeCount;

        public Avalonia.Media.Color IllFormedMarkColor => control._customUnderlines.IllFormedMarkColor;

        public Avalonia.Media.Color SpellingWaveColor => control._customUnderlines.SpellingWaveColor;

        public void OpenContextMenu() => control.SpellCheckContextMenuOpening(control.SpellCheckContextMenu, new CancelEventArgs());

        public void AcceptAutoComplete() => control.AcceptAutoComplete();

        public void MoveAutoCompleteSelection(Key key) => control.MoveAutoCompleteSelection(key);

        public async Task ShowAutoCompleteAsync(bool calledByUser)
        {
            control.InitializeAutoCompleteWordsTask();
            await control._autoCompleteListTask!.GetValueAsync();
            control.UpdateOrShowAutoComplete(calledByUser);
        }

        public void ShowAutoCompleteForCapture(IReadOnlyList<AutoCompleteWord> words)
            => control.ShowAutoCompleteList(words);

        public void CloseAutoComplete() => control.CloseAutoComplete();

        public void ToggleAutoCompletion() => control.ToggleAutoCompletion();

        public bool KeyDown(Key key, KeyModifiers keyModifiers)
        {
            KeyEventArgs e = new()
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = key,
                KeyModifiers = keyModifiers,
            };
            control.TextBox_KeyDown(control.TextBox, e);
            return e.Handled;
        }
    }
}
