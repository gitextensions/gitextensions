using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using GitUI.AutoCompletion;
using Microsoft.VisualStudio.Threading;
using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary;
using ResourceManager;

namespace GitUI.SpellChecker
{
    [DefaultEvent("TextChanged")]
    public partial class EditNetSpell : GitModuleControl
    {
        public event EventHandler TextAssigned;

        private readonly TranslationString _cutMenuItemText = new TranslationString("Cut");
        private readonly TranslationString _copyMenuItemText = new TranslationString("Copy");
        private readonly TranslationString _pasteMenuItemText = new TranslationString("Paste");
        private readonly TranslationString _deleteMenuItemText = new TranslationString("Delete");
        private readonly TranslationString _selectAllMenuItemText = new TranslationString("Select all");

        private readonly TranslationString _addToDictionaryText = new TranslationString("Add to dictionary");
        private readonly TranslationString _ignoreWordText = new TranslationString("Ignore word");
        private readonly TranslationString _removeWordText = new TranslationString("Remove word");
        private readonly TranslationString _dictionaryText = new TranslationString("Dictionary");
        private readonly TranslationString _markIllFormedLinesText = new TranslationString("Mark ill formed lines");
        private readonly TranslationString _autoCompletionText = new TranslationString("Provide auto completion");

        private SpellCheckEditControl _customUnderlines;
        private Spelling _spelling;
        private static WordDictionary _wordDictionary;

        private CancellationTokenSource _autoCompleteCancellationTokenSource = new CancellationTokenSource();
        private readonly List<IAutoCompleteProvider> _autoCompleteProviders = new List<IAutoCompleteProvider>();
        private AsyncLazy<IEnumerable<AutoCompleteWord>> _autoCompleteListTask;
        private bool _autoCompleteWasUserActivated;
        private bool _disableAutoCompleteTriggerOnTextUpdate = true; // only popup on key press
        private readonly Dictionary<Keys, string> _keysToSendToAutoComplete = new Dictionary<Keys, string>
                                                                     {
                                                                             { Keys.Down, "{DOWN}" },
                                                                             { Keys.Up, "{UP}" },
                                                                             { Keys.PageUp, "{PGUP}" },
                                                                             { Keys.PageDown, "{PGDN}" },
                                                                             { Keys.End, "{END}" },
                                                                             { Keys.Home, "{HOME}" }
                                                                     };

        private readonly IWordAtCursorExtractor _wordAtCursorExtractor;

        public Font TextBoxFont { get; set; }

        public bool IsUndoInProgress;

        public EditNetSpell()
        {
            InitializeComponent();
            InitializeComplete();

            MistakeFont = new Font(TextBox.Font, FontStyle.Underline);
            TextBoxFont = TextBox.Font;

            AutoComplete.DisplayMember = nameof(AutoCompleteWord.Word);

            _wordAtCursorExtractor = new WordAtCursorExtractor();
        }

        public override string Text
        {
            get
            {
                if (TextBox == null)
                {
                    return string.Empty;
                }

                return _isWatermarkShowing ? string.Empty : TextBox.Text;
            }
            set
            {
                HideWatermark();
                TextBox.Text = value;
                ShowWatermark();
                OnTextAssigned();
            }
        }

        private void OnTextAssigned()
        {
            TextAssigned?.Invoke(this, EventArgs.Empty);
        }

        public string Line(int line)
        {
            return TextBox.Lines[line];
        }

        public void ReplaceLine(int line, string withText)
        {
            var oldPos = TextBox.SelectionStart + TextBox.SelectionLength;
            var startIdx = TextBox.GetFirstCharIndexFromLine(line);
            TextBox.SelectionLength = 0;
            TextBox.SelectionStart = startIdx;
            TextBox.SelectionLength = Line(line).Length;
            TextBox.SelectedText = withText;
            TextBox.SelectionLength = 0;
            TextBox.SelectionStart = oldPos;
        }

        public int LineLength(int line)
        {
            return LineCount() <= line ? 0 : TextBox.Lines[line].Length;
        }

        public int LineCount()
        {
            return TextBox.Lines.Length;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Font MistakeFont { get; set; }

        [Browsable(false)]
        public int CurrentColumn => TextBox.SelectionStart - TextBox.GetFirstCharIndexOfCurrentLine() + 1;

        [Browsable(false)]
        public int CurrentLine => TextBox.GetLineFromCharIndex(TextBox.SelectionStart) + 1;

        public event EventHandler SelectionChanged;

        private void EditNetSpellEnabledChanged(object sender, EventArgs e)
        {
            if (Enabled)
            {
                TextBox.ReadOnly = false;
                ShowWatermark();
            }
            else
            {
                TextBox.ReadOnly = true;
                HideWatermark();
            }
        }

        private bool _isWatermarkShowing;
        private string _watermarkText = "";
        [Category("Appearance")]
        [DefaultValue("")]
        public string WatermarkText
        {
            get { return _watermarkText; }

            set
            {
                HideWatermark();
                _watermarkText = value;
                ShowWatermark();
            }
        }

        [Category("Appearance")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get => TextBox.SelectionStart;
            set => TextBox.SelectionStart = value;
        }

        [Category("Appearance")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int SelectionLength
        {
            get => TextBox.SelectionLength;

            set => TextBox.SelectionLength = value;
        }

        [Category("Appearance")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string SelectedText
        {
            get => TextBox.SelectedText;
            set => TextBox.SelectedText = value;
        }

        protected RepoDistSettings Settings => Module.EffectiveSettings ?? AppSettings.SettingsContainer;

        public void SelectAll()
        {
            TextBox.SelectAll();
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            _customUnderlines = new SpellCheckEditControl(TextBox);
            TextBox.SelectionChanged += TextBox_SelectionChanged;
            TextBox.TextChanged += TextBoxTextChanged;
            TextBox.DoubleClick += TextBox_DoubleClick;

            EnabledChanged += EditNetSpellEnabledChanged;

            ShowWatermark();

            components = new Container();
            _spelling = new Spelling(components)
            {
                ShowDialog = false,
                IgnoreAllCapsWords = true,
                IgnoreWordsWithDigits = true
            };

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

            SpellCheckTimer.Enabled = true;
        }

        private ToolStripMenuItem AddContextMenuItem(string text, EventHandler eventHandler)
        {
            var menuItem = new ToolStripMenuItem(text, null, eventHandler);
            SpellCheckContextMenu.Items.Add(menuItem);
            return menuItem;
        }

        private void AddContextMenuSeparator()
        {
            SpellCheckContextMenu.Items.Add(new ToolStripSeparator());
        }

        private void AddDictionaries()
        {
            try
            {
                var dictionaryToolStripMenuItem = new ToolStripMenuItem(_dictionaryText.Text);
                SpellCheckContextMenu.Items.Add(dictionaryToolStripMenuItem);

                var toolStripDropDown = new ContextMenuStrip();

                var noDicToolStripMenuItem = new ToolStripMenuItem("None");
                noDicToolStripMenuItem.Click += DicToolStripMenuItemClick;
                if (Settings.Dictionary == "None")
                {
                    noDicToolStripMenuItem.Checked = true;
                }

                toolStripDropDown.Items.Add(noDicToolStripMenuItem);

                foreach (var fileName in Directory.GetFiles(AppSettings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    var file = new FileInfo(fileName);

                    var dic = file.Name.Replace(".dic", "");

                    var dicToolStripMenuItem = new ToolStripMenuItem(dic);
                    dicToolStripMenuItem.Click += DicToolStripMenuItemClick;

                    if (Settings.Dictionary == dic)
                    {
                        dicToolStripMenuItem.Checked = true;
                    }

                    toolStripDropDown.Items.Add(dicToolStripMenuItem);
                }

                dictionaryToolStripMenuItem.DropDown = toolStripDropDown;
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
                _spelling.Text = TextBox.Text;
                _spelling.WordIndex = _spelling.GetWordIndexFromTextIndex(pos);

                if (_spelling.CurrentWord.Length == 0 || _spelling.TestWord())
                {
                    return;
                }

                _spelling.ShowDialog = false;
                _spelling.MaxSuggestions = 5;

                // generate suggestions
                _spelling.Suggest();

                foreach (var suggestion in _spelling.Suggestions)
                {
                    var si = AddContextMenuItem(suggestion, SuggestionToolStripItemClick);
                    si.Font = new Font(si.Font, FontStyle.Bold);
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
            if (Site != null && Site.DesignMode)
            {
                return;
            }

            string dictionaryFile = string.Concat(Path.Combine(AppSettings.GetDictionaryDir(), Settings.Dictionary), ".dic");

            if (_wordDictionary == null || _wordDictionary.DictionaryFile != dictionaryFile)
            {
                _wordDictionary =
                    new WordDictionary(components)
                    {
                        DictionaryFile = dictionaryFile
                    };
            }

            _spelling.Dictionary = _wordDictionary;
        }

        private void ToggleAutoCompletion()
        {
            if (!AppSettings.ProvideAutocompletion)
            {
                CloseAutoComplete();
                CancelAutoComplete();
                return;
            }

            InitializeAutoCompleteWordsTask();

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    var words = await _autoCompleteListTask.GetValueAsync();
                    await this.SwitchToMainThreadAsync(_autoCompleteCancellationTokenSource.Token);

                    _spelling.AddAutoCompleteWords(words.Select(x => x.Word));
                });
        }

        private void SpellingMisspelledWord(object sender, SpellingEventArgs e)
        {
            _customUnderlines.Lines.Add(new TextPos(e.TextIndex, e.TextIndex + e.Word.Length));
        }

        public void CheckSpelling()
        {
            _customUnderlines.IllFormedLines.Clear();
            _customUnderlines.Lines.Clear();

            // Do not check spelling of watermark text
            if (!_isWatermarkShowing)
            {
                try
                {
                    if (_spelling != null && TextBox.Text.Length < 5000)
                    {
                        _spelling.Text = TextBox.Text;
                        _spelling.ShowDialog = false;

                        if (File.Exists(_spelling.Dictionary.DictionaryFile))
                        {
                            _spelling.SpellCheck();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }

                MarkLines();
            }

            TextBox.Refresh();
        }

        private void MarkLines()
        {
            if (!AppSettings.MarkIllFormedLinesInCommitMsg)
            {
                return;
            }

            var numLines = TextBox.Lines.Length;
            var chars = 0;
            for (var curLine = 0; curLine < numLines; ++curLine)
            {
                var curLength = TextBox.Lines[curLine].Length;
                var curMaxLength = 72;
                if (curLine == 0)
                {
                    curMaxLength = 50;
                }

                if (curLine == 1)
                {
                    curMaxLength = 0;
                }

                if (curLength > curMaxLength)
                {
                    _customUnderlines.IllFormedLines.Add(new TextPos(chars + curMaxLength, chars + curLength));
                }

                chars += curLength + 1;
            }
        }

        private void SpellingDeletedWord(object sender, SpellingEventArgs e)
        {
            var start = TextBox.SelectionStart;
            var length = TextBox.SelectionLength;

            TextBox.Select(e.TextIndex, e.Word.Length);
            TextBox.SelectedText = "";

            if (start > TextBox.Text.Length)
            {
                start = TextBox.Text.Length;
            }

            if ((start + length) > TextBox.Text.Length)
            {
                length = 0;
            }

            TextBox.Select(start, length);
        }

        private void SpellingReplacedWord(object sender, ReplaceWordEventArgs e)
        {
            var start = TextBox.SelectionStart;
            var length = TextBox.SelectionLength;

            TextBox.Select(e.TextIndex, e.Word.Length);
            TextBox.SelectedText = e.ReplacementWord;

            if (start > TextBox.Text.Length)
            {
                start = TextBox.Text.Length;
            }

            if ((start + length) > TextBox.Text.Length)
            {
                length = 0;
            }

            TextBox.Select(start, length);
        }

        private void SpellCheckContextMenuOpening(object sender, CancelEventArgs e)
        {
            TextBox.Focus();
            var pos = TextBox.GetCharIndexFromPosition(TextBox.PointToClient(MousePosition));
            if (pos < 0)
            {
                e.Cancel = true;
                return;
            }

            SpellCheckContextMenu.Items.Clear();

            AddWordSuggestions(pos);
            AddContextMenuItem(_cutMenuItemText.Text, CutMenuItemClick);
            AddContextMenuItem(_copyMenuItemText.Text, CopyMenuItemdClick);
            AddContextMenuItem(_pasteMenuItemText.Text, PasteMenuItemClick);
            AddContextMenuItem(_deleteMenuItemText.Text, DeleteMenuItemClick);
            AddContextMenuItem(_selectAllMenuItemText.Text, SelectAllMenuItemClick);

            AddContextMenuSeparator();

            AddDictionaries();

            AddContextMenuSeparator();

            var mi = new ToolStripMenuItem(_markIllFormedLinesText.Text)
            {
                Checked = AppSettings.MarkIllFormedLinesInCommitMsg
            };
            mi.Click += MarkIllFormedLinesInCommitMsgClick;
            SpellCheckContextMenu.Items.Add(mi);

            mi = new ToolStripMenuItem(_autoCompletionText.Text)
            {
                Checked = AppSettings.ProvideAutocompletion
            };
            mi.Click += (s, _) =>
            {
                AppSettings.ProvideAutocompletion = !AppSettings.ProvideAutocompletion;
                ToggleAutoCompletion();
            };
            SpellCheckContextMenu.Items.Add(mi);
        }

        private void RemoveWordClick(object sender, EventArgs e)
        {
            _spelling.DeleteWord();
            CheckSpelling();
        }

        private void IgnoreWordClick(object sender, EventArgs e)
        {
            _spelling.IgnoreWord();
            CheckSpelling();
        }

        private void AddToDictionaryClick(object sender, EventArgs e)
        {
            _spelling.Dictionary.Add(_spelling.CurrentWord);
            CheckSpelling();
        }

        private void MarkIllFormedLinesInCommitMsgClick(object sender, EventArgs e)
        {
            AppSettings.MarkIllFormedLinesInCommitMsg = !AppSettings.MarkIllFormedLinesInCommitMsg;
            CheckSpelling();
        }

        private void SuggestionToolStripItemClick(object sender, EventArgs e)
        {
            _spelling.ReplaceWord(((ToolStripItem)sender).Text);
            CheckSpelling();
        }

        private void DicToolStripMenuItemClick(object sender, EventArgs e)
        {
            // if a Module is available, then always change the "repository local" setting
            // it will set a dictionary only for this Module (repository) locally

            var settings = Module.LocalSettings ?? Settings;

            settings.Dictionary = ((ToolStripItem)sender).Text;
            LoadDictionary();
            CheckSpelling();
        }

        private void SpellCheckTimerTick(object sender, EventArgs e)
        {
            if (!_customUnderlines.IsImeStartingComposition)
            {
                CheckSpelling();

                SpellCheckTimer.Enabled = false;
                SpellCheckTimer.Interval = 250;
            }
        }

        private void TextBoxTextChanged(object sender, EventArgs e)
        {
            if (_customUnderlines.IsImeStartingComposition)
            {
                return;
            }

            if (!_disableAutoCompleteTriggerOnTextUpdate)
            {
                _disableAutoCompleteTriggerOnTextUpdate = true; // only popup on key press

                // Reset when timer is already running
                if (AutoCompleteTimer.Enabled)
                {
                    AutoCompleteTimer.Stop();
                }

                AutoCompleteTimer.Start();
            }

            _customUnderlines.Lines.Clear();
            _customUnderlines.IllFormedLines.Clear();

            if (!_isWatermarkShowing)
            {
                OnTextChanged(e);

                if (Settings.Dictionary == "None" || TextBox.Text.Length < 4)
                {
                    return;
                }

                SpellCheckTimer.Enabled = false;
                SpellCheckTimer.Interval = 250;
                SpellCheckTimer.Enabled = true;
            }
        }

        private void TextBoxLeave(object sender, EventArgs e)
        {
            if (ActiveControl != AutoComplete)
            {
                CloseAutoComplete();
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private bool _skipSelectionUndo;

        private void UndoHighlighting()
        {
            if (!_skipSelectionUndo)
            {
                return;
            }

            IsUndoInProgress = true;
            while (TextBox.UndoActionName == "Unknown")
            {
                TextBox.Undo();
            }

            TextBox.Undo();
            _skipSelectionUndo = false;
            IsUndoInProgress = false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Alt && !e.Control && !e.Shift && _keysToSendToAutoComplete.ContainsKey(e.KeyCode) && AutoComplete.Visible)
            {
                if (e.KeyCode == Keys.Up && AutoComplete.SelectedIndex == 0)
                {
                    AutoComplete.SelectedIndex = AutoComplete.Items.Count - 1;
                }
                else if (e.KeyCode == Keys.Down && AutoComplete.SelectedIndex == AutoComplete.Items.Count - 1)
                {
                    AutoComplete.SelectedIndex = 0;
                }
                else
                {
                    AutoComplete.Focus();
                    SendKeys.SendWait(_keysToSendToAutoComplete[e.KeyCode]);
                    TextBox.Focus();
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            // handle paste from clipboard (Ctrl+V, Shift+Ins)
            if ((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert))
            {
                PasteTextFromClipboard();
                e.Handled = true;
                return;
            }

            if (e.Control && !e.Alt && e.KeyCode == Keys.Z)
            {
                UndoHighlighting();
            }
            else if (e.Control && !e.Alt && e.KeyCode == Keys.Space && AppSettings.ProvideAutocompletion)
            {
                UpdateOrShowAutoComplete(true);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            OnKeyDown(e);
        }

        private void PasteTextFromClipboard()
        {
            // insert only text
            TextBox.Paste(DataFormats.GetFormat(DataFormats.UnicodeText));
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            _disableAutoCompleteTriggerOnTextUpdate = e.KeyChar.IsSeparator();

            OnKeyPress(e);
        }

        private void TextBox_SelectionChanged(object sender, EventArgs e)
        {
            SelectionChanged?.Invoke(sender, e);
        }

        private void TextBox_DoubleClick(object sender, EventArgs e)
        {
            int charIndexAtMousePosition = TextBox.GetCharIndexFromPosition(TextBox.PointToClient(MousePosition));
            (int start, int length) = _wordAtCursorExtractor.GetWordBounds(TextBox.Text, charIndexAtMousePosition);
            TextBox.Select(start, length);
        }

        private void ShowWatermark()
        {
            if (!ContainsFocus && string.IsNullOrEmpty(TextBox.Text) && TextBoxFont != null)
            {
                _isWatermarkShowing = true;
                TextBox.Font = new Font(TextBox.Font, FontStyle.Italic);
                TextBox.ForeColor = SystemColors.InactiveCaption;
                TextBox.Text = WatermarkText;
            }
        }

        private void HideWatermark()
        {
            if (_isWatermarkShowing && TextBoxFont != null)
            {
                TextBox.Font = TextBoxFont;
                _isWatermarkShowing = false;
                TextBox.Text = string.Empty;
                TextBox.ForeColor = SystemColors.WindowText;
            }
        }

        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            ShowWatermark();
        }

        private void TextBox_GotFocus(object sender, EventArgs e)
        {
            HideWatermark();
        }

        private void CutMenuItemClick(object sender, EventArgs e)
        {
            TextBox.Cut();
            CheckSpelling();
        }

        private void CopyMenuItemdClick(object sender, EventArgs e)
        {
            TextBox.Copy();
        }

        private void PasteMenuItemClick(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }

            PasteTextFromClipboard();
            CheckSpelling();
        }

        private void DeleteMenuItemClick(object sender, EventArgs e)
        {
            TextBox.SelectedText = string.Empty;
            CheckSpelling();
        }

        private void SelectAllMenuItemClick(object sender, EventArgs e)
        {
            TextBox.SelectAll();
        }

        public void ChangeTextColor(int line, int offset, int length, Color color)
        {
            var oldPos = TextBox.SelectionStart;
            var oldColor = TextBox.SelectionColor;
            var lineIndex = TextBox.GetFirstCharIndexFromLine(line);
            TextBox.SelectionStart = Math.Max(lineIndex + offset, 0);
            TextBox.SelectionLength = length;
            TextBox.SelectionColor = color;
            var restoreColor = oldPos < TextBox.SelectionStart || oldPos > TextBox.SelectionStart + TextBox.SelectionLength;

            TextBox.SelectionLength = 0;
            TextBox.SelectionStart = oldPos;

            // restore old color only if oldPos doesn't intersects with colored selection
            if (restoreColor)
            {
                TextBox.SelectionColor = oldColor;
            }

            // undoes all recent selections while ctrl-z pressed
            _skipSelectionUndo = true;
        }

        /// <summary>
        /// Make sure this line is empty by inserting a newline at its start.
        /// </summary>
        public void EnsureEmptyLine(bool addBullet, int afterLine)
        {
            var lineLength = LineLength(afterLine);
            if (lineLength > 0)
            {
                var bullet = addBullet ? " - " : string.Empty;
                var indexOfLine = TextBox.GetFirstCharIndexFromLine(afterLine);
                var newLine = Environment.NewLine;
                var newCursorPos = indexOfLine + newLine.Length + bullet.Length + lineLength - 1;
                TextBox.SelectionLength = 0;
                TextBox.SelectionStart = indexOfLine;
                TextBox.SelectedText = newLine + bullet;
                TextBox.SelectionLength = 0;
                TextBox.SelectionStart = newCursorPos;
            }
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
            _autoCompleteListTask = new AsyncLazy<IEnumerable<AutoCompleteWord>>(
                async () =>
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                    var subTasks = _autoCompleteProviders.Select(p => p.GetAutoCompleteWordsAsync(_autoCompleteCancellationTokenSource.Token)).ToArray();
                    try
                    {
                        var results = await Task.WhenAll(subTasks);
                        return results.SelectMany(result => result).Distinct().ToList();
                    }
                    catch (OperationCanceledException)
                    {
                        // WaitAll was cancelled
                        return null;
                    }
                    catch (Exception)
                    {
                        if (subTasks.Any(t => t.IsCanceled))
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (AutoComplete.Visible)
            {
                if (keyData == Keys.Tab || keyData == Keys.Enter)
                {
                    AcceptAutoComplete();
                    return true;
                }

                if (keyData == Keys.Escape)
                {
                    CloseAutoComplete();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private string GetWordAtCursor()
        {
            return _wordAtCursorExtractor.Extract(TextBox.Text, TextBox.SelectionStart - 1);
        }

        private void CloseAutoComplete()
        {
            AutoComplete.Hide();
            _autoCompleteWasUserActivated = false;
        }

        private void AcceptAutoComplete(AutoCompleteWord completionWord = null)
        {
            completionWord = completionWord ?? (AutoCompleteWord)AutoComplete.SelectedItem;
            var word = GetWordAtCursor();
            TextBox.Select(TextBox.SelectionStart - word.Length, word.Length);
            TextBox.SelectedText = completionWord.Word;
            CloseAutoComplete();
        }

        private void UpdateOrShowAutoComplete(bool calledByUser)
        {
            if (IsDisposed)
            {
                return;
            }

            if (_customUnderlines.IsImeStartingComposition)
            {
                return;
            }

            if (_autoCompleteListTask == null || !AppSettings.ProvideAutocompletion)
            {
                return;
            }

            if (!_autoCompleteListTask.IsValueFactoryCompleted)
            {
                _autoCompleteListTask.GetValueAsync().Forget();

                if (calledByUser)
                {
                    AutoCompleteToolTip.Show(
                            "AutoComplete is not available yet (it is still parsing the changed files).",
                            TextBox,
                            GetCursorPosition());
                    AutoCompleteToolTipTimer.Start();
                }

                return;
            }

            AutoCompleteToolTipTimer.Stop();
            AutoCompleteToolTip.Hide(TextBox);

            var word = GetWordAtCursor();

            if (word == null || (word.Length <= 1 && !calledByUser && !_autoCompleteWasUserActivated))
            {
                if (AutoComplete.Visible)
                {
                    CloseAutoComplete();
                }

                return;
            }

            var autoCompleteList = ThreadHelper.JoinableTaskFactory.Run(() => _autoCompleteListTask.GetValueAsync());
            var list = autoCompleteList.Where(x => x.Matches(word)).Distinct().ToList();

            if (list.Count == 0)
            {
                if (AutoComplete.Visible)
                {
                    CloseAutoComplete();
                }

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

            var sizes = list.Select(x => TextRenderer.MeasureText(x.Word, TextBox.Font)).ToList();

            var cursorPos = GetCursorPosition();

            var top = cursorPos.Y;
            var height = (sizes.Count + 1) * AutoComplete.ItemHeight;
            var width = sizes.Max(x => x.Width);
            if (top + height > TextBox.Height)
            {
                // if reduced height is not too small then shrink only
                if (TextBox.Height - top > TextBox.Height / 2)
                {
                    height = TextBox.Height - top;
                }
                else
                {
                    // if shrinking wasn't acceptable, move higher
                    top = Math.Max(0, TextBox.Height - height);

                    // and reduce height if moving up wasn't enough
                    height = Math.Min(TextBox.Height - top, height);
                }

                width += SystemInformation.VerticalScrollBarWidth;
            }

            AutoComplete.SetBounds(cursorPos.X, top, width, height);

            AutoComplete.DataSource = list.ToList();
            AutoComplete.Show();
            TextBox.Focus();
        }

        private Point GetCursorPosition()
        {
            var cursorPos = TextBox.GetPositionFromCharIndex(TextBox.SelectionStart);
            cursorPos.Y += (int)Math.Ceiling(TextBox.Font.GetHeight());
            cursorPos.X += 2;
            return cursorPos;
        }

        private void AutoComplete_Click(object sender, EventArgs e)
        {
            AcceptAutoComplete();
        }

        private void AutoCompleteTimer_Tick(object sender, EventArgs e)
        {
            if (!_customUnderlines.IsImeStartingComposition)
            {
                UpdateOrShowAutoComplete(false);
                AutoCompleteTimer.Stop();
            }
        }

        public void CancelAutoComplete()
        {
            _autoCompleteCancellationTokenSource.Cancel();
            AutoCompleteToolTipTimer.Stop();
            AutoCompleteTimer.Stop();
        }

        private void AutoCompleteToolTipTimer_Tick(object sender, EventArgs e)
        {
            AutoCompleteToolTip.Hide(TextBox);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelAutoComplete();
                SpellCheckTimer.Stop();
                _autoCompleteCancellationTokenSource.Dispose();
                _customUnderlines?.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
