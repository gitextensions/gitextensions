using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using GitUI.AutoCompletion;
using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary;
using ResourceManager;

namespace GitUI.SpellChecker
{
    [DefaultEvent("TextChanged")]
    public partial class EditNetSpell : GitModuleControl
    {
        private readonly TranslationString cutMenuItemText = new TranslationString("Cut");
        private readonly TranslationString copyMenuItemText = new TranslationString("Copy");
        private readonly TranslationString pasteMenuItemText = new TranslationString("Paste");
        private readonly TranslationString deleteMenuItemText = new TranslationString("Delete");
        private readonly TranslationString selectAllMenuItemText = new TranslationString("Select all");

        private readonly TranslationString addToDictionaryText = new TranslationString("Add to dictionary");
        private readonly TranslationString ignoreWordText = new TranslationString("Ignore word");
        private readonly TranslationString removeWordText = new TranslationString("Remove word");
        private readonly TranslationString dictionaryText = new TranslationString("Dictionary");
        private readonly TranslationString markIllFormedLinesText = new TranslationString("Mark ill formed lines");

        private readonly SpellCheckEditControl _customUnderlines;
        private Spelling _spelling;
        private static WordDictionary _wordDictionary;

        private readonly CancellationTokenSource _autoCompleteCancellationTokenSource = new CancellationTokenSource();
        private readonly List<IAutoCompleteProvider> _autoCompleteProviders = new List<IAutoCompleteProvider>();
        private Task<IEnumerable<AutoCompleteWord>> _autoCompleteListTask;
        private bool _autoCompleteWasUserActivated;
        private bool _disableAutoCompleteTriggerOnTextUpdate;
        private readonly Dictionary<Keys, string> _keysToSendToAutoComplete = new Dictionary<Keys, string>
                                                                     {
                                                                             { Keys.Down, "{DOWN}" },
                                                                             { Keys.Up, "{UP}" },
                                                                             { Keys.PageUp, "{PGUP}" },
                                                                             { Keys.PageDown, "{PGDN}" },
                                                                             { Keys.End, "{END}" },
                                                                             { Keys.Home, "{HOME}" }
                                                                     };

        public Font TextBoxFont { get; set; }

        public EventHandler TextAssigned;
        public bool IsUndoInProgress = false;

        public EditNetSpell()
        {
            InitializeComponent();
            Translate();

            _customUnderlines = new SpellCheckEditControl(TextBox);

            SpellCheckTimer.Enabled = false;

            EnabledChanged += EditNetSpellEnabledChanged;

            InitializeAutoCompleteWordsTask();
        }

        public override string Text
        {
            get
            {
                if (TextBox == null)
                    return string.Empty;

                return IsWatermarkShowing ? string.Empty : TextBox.Text;
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
            if (TextAssigned != null)
            {
                TextAssigned(this, EventArgs.Empty);
            }
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
        public int CurrentColumn
        {
            get { return TextBox.SelectionStart - TextBox.GetFirstCharIndexOfCurrentLine() + 1; }
        }
        [Browsable(false)]
        public int CurrentLine
        {
            get { return TextBox.GetLineFromCharIndex(TextBox.SelectionStart) + 1; }
        }
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

        private bool IsWatermarkShowing;
        private string _WatermarkText = "";
        [Category("Appearance")]
        [DefaultValue("")]
        public string WatermarkText
        {
            get { return _WatermarkText; }

            set
            {
                HideWatermark();
                _WatermarkText = value;
                ShowWatermark();
            }
        }

        [Category("Appearance")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get
            {
                return TextBox.SelectionStart;
            }
            set
            {
                TextBox.SelectionStart = value;
            }
        }

        [Category("Appearance")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int SelectionLength
        {
            get
            {
                return TextBox.SelectionLength;
            }

            set
            {
                TextBox.SelectionLength = value;
            }
        }

        [Category("Appearance")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string SelectedText
        {
            get
            {
                return TextBox.SelectedText;
            }
            set
            {
               TextBox.SelectedText = value;
            }
        }

        protected RepoDistSettings Settings
        {
            get
            {
                return IsUICommandsInitialized ?
                    Module.EffectiveSettings:
                    AppSettings.SettingsContainer;
            }
        }

        public void SelectAll()
        {
            TextBox.SelectAll();
        }

        private void EditNetSpellLoad(object sender, EventArgs e)
        {
            MistakeFont = new Font(TextBox.Font, FontStyle.Underline);
            TextBoxFont = TextBox.Font;
            ShowWatermark();

            components = new Container();
            _spelling =
                new Spelling(components)
                    {
                        ShowDialog = false,
                        IgnoreAllCapsWords = true,
                        IgnoreWordsWithDigits = true
                    };

            _autoCompleteListTask.ContinueWith(
                w => _spelling.AddAutoCompleteWords(w.Result.Select(x => x.Word)),
                _autoCompleteCancellationTokenSource.Token,
                TaskContinuationOptions.NotOnCanceled,
                TaskScheduler.FromCurrentSynchronizationContext()
            );
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
        }

        private void LoadDictionary()
        {
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

        private void SpellingMisspelledWord(object sender, SpellingEventArgs e)
        {
            _customUnderlines.Lines.Add(new TextPos(e.TextIndex, e.TextIndex + e.Word.Length));
        }

        public void CheckSpelling()
        {
            _customUnderlines.IllFormedLines.Clear();
            _customUnderlines.Lines.Clear();

            //Do not check spelling of watermark text
            if (!IsWatermarkShowing)
            {
                try
                {
                    if (_spelling != null && TextBox.Text.Length < 5000)
                    {
                        _spelling.Text = TextBox.Text;
                        _spelling.ShowDialog = false;

                        if (File.Exists(_spelling.Dictionary.DictionaryFile))
                            _spelling.SpellCheck();
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
                return;
            var numLines = TextBox.Lines.Length;
            var chars = 0;
            for (var curLine = 0; curLine < numLines; ++curLine)
            {
                var curLength = TextBox.Lines[curLine].Length;
                var curMaxLength = 72;
                if (curLine == 0)
                    curMaxLength = 50;
                if (curLine == 1)
                    curMaxLength = 0;
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
                start = TextBox.Text.Length;

            if ((start + length) > TextBox.Text.Length)
                length = 0;

            TextBox.Select(start, length);
        }

        private void SpellingReplacedWord(object sender, ReplaceWordEventArgs e)
        {
            var start = TextBox.SelectionStart;
            var length = TextBox.SelectionLength;

            TextBox.Select(e.TextIndex, e.Word.Length);
            TextBox.SelectedText = e.ReplacementWord;

            if (start > TextBox.Text.Length)
                start = TextBox.Text.Length;

            if ((start + length) > TextBox.Text.Length)
                length = 0;

            TextBox.Select(start, length);
        }

        private ToolStripMenuItem AddContextMenuItem(String text, EventHandler eventHandler)
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem(text, null, eventHandler);
            SpellCheckContextMenu.Items.Add(menuItem);
            return menuItem;
        }

        private void AddContextMenuSeparator()
        {
            SpellCheckContextMenu.Items.Add(new ToolStripSeparator());
        }

        private void SpellCheckContextMenuOpening(object sender, CancelEventArgs e)
        {
            TextBox.Focus();

            SpellCheckContextMenu.Items.Clear();

            try
            {
                var pos = TextBox.GetCharIndexFromPosition(TextBox.PointToClient(MousePosition));

                if (pos < 0)
                {
                    e.Cancel = true;
                    return;
                }

                _spelling.Text = TextBox.Text;
                _spelling.WordIndex = _spelling.GetWordIndexFromTextIndex(pos);

                if (_spelling.CurrentWord.Length != 0 && !_spelling.TestWord())
                {
                    _spelling.ShowDialog = false;
                    _spelling.MaxSuggestions = 5;

                    //generate suggestions
                    _spelling.Suggest();

                    foreach (var suggestion in _spelling.Suggestions)
                    {
                        var si = AddContextMenuItem(suggestion, SuggestionToolStripItemClick);
                        si.Font = new System.Drawing.Font(si.Font, FontStyle.Bold);
                    }

                    AddContextMenuItem(addToDictionaryText.Text, AddToDictionaryClick)
                        .Enabled = (_spelling.CurrentWord.Length > 0);
                    AddContextMenuItem(ignoreWordText.Text, IgnoreWordClick)
                        .Enabled = (_spelling.CurrentWord.Length > 0);
                    AddContextMenuItem(removeWordText.Text, RemoveWordClick)
                        .Enabled = (_spelling.CurrentWord.Length > 0);

                    if (_spelling.Suggestions.Count > 0)
                        AddContextMenuSeparator();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            AddContextMenuItem(cutMenuItemText.Text, CutMenuItemClick)
                .Enabled = (TextBox.SelectedText.Length > 0);
            AddContextMenuItem(copyMenuItemText.Text, CopyMenuItemdClick)
                .Enabled = (TextBox.SelectedText.Length > 0);
            AddContextMenuItem(pasteMenuItemText.Text, PasteMenuItemClick)
                .Enabled = Clipboard.ContainsText();
            AddContextMenuItem(deleteMenuItemText.Text, DeleteMenuItemClick)
                .Enabled = (TextBox.SelectedText.Length > 0);
            AddContextMenuItem(selectAllMenuItemText.Text, SelectAllMenuItemClick);

            /*AddContextMenuSeparator();

            if (!string.IsNullOrEmpty(_spelling.CurrentWord))
            {
                string text = string.Format(translateCurrentWord.Text, _spelling.CurrentWord, CultureCodeToString(Settings.Dictionary));
                AddContextMenuItem(text, translate_Click);
            }

            string entireText = string.Format(translateEntireText.Text, CultureCodeToString(Settings.Dictionary));
            AddContextMenuItem(entireText, translateText_Click);*/

            AddContextMenuSeparator();

            try
            {
                var dictionaryToolStripMenuItem = new ToolStripMenuItem(dictionaryText.Text);
                SpellCheckContextMenu.Items.Add(dictionaryToolStripMenuItem);

                var toolStripDropDown = new ContextMenuStrip();

                var noDicToolStripMenuItem = new ToolStripMenuItem("None");
                noDicToolStripMenuItem.Click += DicToolStripMenuItemClick;
                if (Settings.Dictionary == "None")
                    noDicToolStripMenuItem.Checked = true;


                toolStripDropDown.Items.Add(noDicToolStripMenuItem);

                foreach (
                    var fileName in
                        Directory.GetFiles(AppSettings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    var file = new FileInfo(fileName);

                    var dic = file.Name.Replace(".dic", "");

                    var dicToolStripMenuItem = new ToolStripMenuItem(dic);
                    dicToolStripMenuItem.Click += DicToolStripMenuItemClick;

                    if (Settings.Dictionary == dic)
                        dicToolStripMenuItem.Checked = true;

                    toolStripDropDown.Items.Add(dicToolStripMenuItem);
                }

                dictionaryToolStripMenuItem.DropDown = toolStripDropDown;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            AddContextMenuSeparator();

            var mi =
                new ToolStripMenuItem(markIllFormedLinesText.Text)
                    {
                        Checked = AppSettings.MarkIllFormedLinesInCommitMsg
                    };
            mi.Click += MarkIllFormedLinesInCommitMsgClick;
            SpellCheckContextMenu.Items.Add(mi);
        }

        private static string CultureCodeToString(string cultureCode)
        {
            try
            {
                return new CultureInfo(new CultureInfo(cultureCode.Replace('_', '-')).TwoLetterISOLanguageName).NativeName;
            }
            catch
            {
                return cultureCode;
            }
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
            RepoDistSettings settings;
            //if a Module is available, then always change the "repository local" setting
            //it will set a dictionary only for this Module (repository) localy
            if (IsUICommandsInitialized)
                settings = Module.LocalSettings;
            else
                settings = Settings;

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
            if (!_disableAutoCompleteTriggerOnTextUpdate)
            {
                // Reset when timer is already running
                if (AutoCompleteTimer.Enabled)
                    AutoCompleteTimer.Stop();

                AutoCompleteTimer.Start();
            }

            _customUnderlines.Lines.Clear();
            _customUnderlines.IllFormedLines.Clear();

            if (!IsWatermarkShowing)
            {
                OnTextChanged(e);

                if (Settings.Dictionary == "None" || TextBox.Text.Length < 4)
                    return;

                SpellCheckTimer.Enabled = false;
                SpellCheckTimer.Interval = 250;
                SpellCheckTimer.Enabled = true;
            }
        }

        private void TextBoxSizeChanged(object sender, EventArgs e)
        {
            SpellCheckTimer.Enabled = true;
        }

        private void TextBoxLeave(object sender, EventArgs e)
        {
            ShowWatermark();
            if (ActiveControl != AutoComplete)
                CloseAutoComplete();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private bool skipSelectionUndo = false;
        private void UndoHighlighting()
        {
            if (!skipSelectionUndo)
                return;

            IsUndoInProgress = true;
            while (TextBox.UndoActionName.Equals("Unknown"))
            {
                TextBox.Undo();
            }
            TextBox.Undo();
            skipSelectionUndo = false;
            IsUndoInProgress = false;
        }



        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Alt && !e.Control && !e.Shift && _keysToSendToAutoComplete.ContainsKey (e.KeyCode) && AutoComplete.Visible)
            {

                if (e.KeyCode == Keys.Up && AutoComplete.SelectedIndex == 0)
                    AutoComplete.SelectedIndex = AutoComplete.Items.Count - 1;
                else if (e.KeyCode == Keys.Down && AutoComplete.SelectedIndex == AutoComplete.Items.Count - 1)
                    AutoComplete.SelectedIndex = 0;
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

            if (e.Control && e.KeyCode == Keys.V)
            {
                if (!Clipboard.ContainsText())
                {
                    e.Handled = true;
                    return;
                }
                // remove image data from clipboard
                var text = Clipboard.GetText();
                // Clipboard.SetText throws exception when text is null or empty. See https://msdn.microsoft.com/en-us/library/ydby206k.aspx
                if (!string.IsNullOrEmpty(text))
                {
                    Clipboard.SetText(text);
                }
            }
            else if (e.Control && !e.Alt && e.KeyCode == Keys.Z)
            {
                UndoHighlighting();
            }
            else if (e.Control && !e.Alt && e.KeyCode == Keys.Space)
            {
                UpdateOrShowAutoComplete(true);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            OnKeyDown(e);
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void TextBox_SelectionChanged(object sender, EventArgs e)
        {
            UpdateOrShowAutoComplete(false);

            if (SelectionChanged != null)
                SelectionChanged(sender, e);
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            HideWatermark();
        }

        private void ShowWatermark()
        {
            if (!ContainsFocus && string.IsNullOrEmpty(TextBox.Text) && TextBoxFont != null)
            {
                IsWatermarkShowing = true;
                TextBox.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Italic);
                TextBox.ForeColor = SystemColors.InactiveCaption;
                TextBox.Text = WatermarkText;
            }
        }

        private void HideWatermark()
        {
            if (IsWatermarkShowing && TextBoxFont != null)
            {
                TextBox.Font = TextBoxFont;
                IsWatermarkShowing = false;
                TextBox.Text = string.Empty;
                TextBox.ForeColor = SystemColors.WindowText;
            }
        }

        public new bool Focus()
        {
            HideWatermark();
            return base.Focus();
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
            if (!Clipboard.ContainsText()) return;
            // remove image data from clipboard
            string text = Clipboard.GetText();
            Clipboard.SetText(text);

            TextBox.Paste();
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
            //restore old color only if oldPos doesn't intersects with colored selection
            if(restoreColor)
                TextBox.SelectionColor = oldColor;
            //undoes all recent selections while ctrl-z pressed
            skipSelectionUndo = true;
        }

        /// <summary>
        /// Make sure this line is empty by inserting a newline at its start.
        /// </summary>
        public void EnsureEmptyLine(bool addBullet, int afterLine)
        {
            var lineLength = LineLength(afterLine);
            if (lineLength > 0)
            {
                var bullet = addBullet ? " - " : String.Empty;
                var indexOfLine = TextBox.GetFirstCharIndexFromLine(afterLine);
                var newLine = (lineLength > 0) ? Environment.NewLine : String.Empty;
                var newCursorPos = indexOfLine + newLine.Length + bullet.Length + lineLength - 1;
                TextBox.SelectionLength = 0;
                TextBox.SelectionStart = indexOfLine;
                TextBox.SelectedText = newLine + bullet;
                TextBox.SelectionLength = 0;
                TextBox.SelectionStart = newCursorPos;
            }
        }

        private void InitializeAutoCompleteWordsTask ()
        {
            _autoCompleteListTask = new Task<IEnumerable<AutoCompleteWord>>(
                    () =>
                    {
                        var subTasks = _autoCompleteProviders.Select(p => p.GetAutoCompleteWords(_autoCompleteCancellationTokenSource)).ToArray();
                        try
                        {
                            Task.WaitAll(subTasks, _autoCompleteCancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            // WaitAll was cancelled
                            return null;
                        }
                        catch (AggregateException ex)
                        {
                            if (ex.InnerExceptions.OfType<OperationCanceledException>().Any())
                            {
                                // At least one task was cancelled
                                return null;
                            }

                            throw;
                        }

                        return subTasks.SelectMany(t => t.Result).Distinct().ToList();
                    });
        }

        public void AddAutoCompleteProvider (IAutoCompleteProvider autoCompleteProvider)
        {
            _autoCompleteProviders.Add(autoCompleteProvider);
        }

        protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
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

        private string GetWordAtCursor ()
        {
            if (TextBox.Text.Length > TextBox.SelectionStart && IsSeparatorChar(TextBox.Text[TextBox.SelectionStart]))
                return null;

            var sb = new StringBuilder();

            int i = TextBox.SelectionStart - 1;

            while (i >= 0 && !IsSeparatorChar(TextBox.Text[i]))
                sb.Insert(0, TextBox.Text[i--]);

            return sb.ToString();
        }

        private bool IsSeparatorChar (char c)
        {
            return char.IsWhiteSpace(c) || c == '.';
        }

        private void CloseAutoComplete ()
        {
            AutoComplete.Hide();
            _autoCompleteWasUserActivated = false;
        }

        private void AcceptAutoComplete (AutoCompleteWord completionWord = null)
        {
            completionWord = completionWord ?? (AutoCompleteWord) AutoComplete.SelectedItem;

            var word = GetWordAtCursor();

            var pos = TextBox.SelectionStart;

            _disableAutoCompleteTriggerOnTextUpdate = true;
            Text = Text.Remove(pos - word.Length, word.Length);
            Text = Text.Insert(pos - word.Length, completionWord.Word);
            _disableAutoCompleteTriggerOnTextUpdate = false;
            TextBox.SelectionStart = pos + completionWord.Word.Length - word.Length;
            CloseAutoComplete();
        }

        private void UpdateOrShowAutoComplete (bool calledByUser)
        {
            if (_autoCompleteListTask == null)
                return;

            if (!_autoCompleteListTask.IsCompleted)
            {
                if (_autoCompleteListTask.Status == TaskStatus.Created)
                    _autoCompleteListTask.Start();

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
                    CloseAutoComplete();

                return;
            }

            var list = _autoCompleteListTask.Result.Where(x => x.Matches(word)).Distinct().ToList();

            if (list.Count == 0)
            {
                if (AutoComplete.Visible)
                    CloseAutoComplete();

                return;
            }

            if (list.Count == 1 && calledByUser)
            {
                AcceptAutoComplete(list[0]);
                return;
            }

            if (calledByUser)
                _autoCompleteWasUserActivated = true;

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

        private Point GetCursorPosition ()
        {
            var cursorPos = TextBox.GetPositionFromCharIndex(TextBox.SelectionStart);
            cursorPos.Y += (int) Math.Ceiling(TextBox.Font.GetHeight());
            cursorPos.X += 2;
            return cursorPos;
        }

        private void AutoComplete_Click (object sender, EventArgs e)
        {
            AcceptAutoComplete();
        }

        private void AutoCompleteTimer_Tick (object sender, EventArgs e)
        {
            if (!_customUnderlines.IsImeStartingComposition)
            {
                UpdateOrShowAutoComplete(false);
                AutoCompleteTimer.Stop();
            }
        }

        public void CancelAutoComplete ()
        {
            _autoCompleteCancellationTokenSource.Cancel();
            AutoCompleteToolTipTimer.Stop();
            AutoCompleteTimer.Stop();
        }

        private void AutoCompleteToolTipTimer_Tick (object sender, EventArgs e)
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
                _autoCompleteCancellationTokenSource.Dispose();
                _customUnderlines.Dispose();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
