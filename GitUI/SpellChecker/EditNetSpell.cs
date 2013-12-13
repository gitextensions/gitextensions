using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary;
using ResourceManager.Translation;

namespace GitUI.SpellChecker
{
    [DefaultEvent("TextChanged")]
    public partial class EditNetSpell : GitExtensionsControl
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

        public Font TextBoxFont { get; set; }
        public EventHandler TextAssigned;

        public EditNetSpell()
        {
            InitializeComponent();
            Translate();

            _customUnderlines = new SpellCheckEditControl(TextBox);

            SpellCheckTimer.Enabled = false;

            EnabledChanged += EditNetSpellEnabledChanged;
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
            string dictionaryFile = string.Concat(Path.Combine(AppSettings.GetDictionaryDir(), AppSettings.Dictionary), ".dic");

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
                if (AppSettings.Dictionary == "None")
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

                    if (AppSettings.Dictionary == dic)
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

        void translateText_Click(object sender, EventArgs e)
        {
            try
            {
                TextBox.Text = Google.TranslateText(TextBox.Text, "", new CultureInfo(AppSettings.Dictionary.Replace('_', '-')).TwoLetterISOLanguageName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString());
            }
        }

        void translate_Click(object sender, EventArgs e)
        {
            try
            {
                _spelling.ReplaceWord(Google.TranslateText(_spelling.CurrentWord, "", new CultureInfo(AppSettings.Dictionary.Replace('_', '-')).TwoLetterISOLanguageName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString());
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
            AppSettings.Dictionary = ((ToolStripItem)sender).Text;
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
            _customUnderlines.Lines.Clear();
            _customUnderlines.IllFormedLines.Clear();

            if (!IsWatermarkShowing)
            {
                OnTextChanged(e);
            }

            if (AppSettings.Dictionary == "None" || TextBox.Text.Length < 4)
                return;

            SpellCheckTimer.Enabled = false;
            SpellCheckTimer.Interval = 250;
            SpellCheckTimer.Enabled = true;
        }

        private void TextBoxSizeChanged(object sender, EventArgs e)
        {
            SpellCheckTimer.Enabled = true;
        }

        private void TextBoxLeave(object sender, EventArgs e)
        {
            ShowWatermark();
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

            while (TextBox.UndoActionName.Equals("Unknown"))
            {
                TextBox.Undo();
            }
            TextBox.Undo();
            skipSelectionUndo = false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (!Clipboard.ContainsText())
                {
                    e.Handled = true;
                    return;
                }
                // remove image data from clipboard
                string text = Clipboard.GetText();
                Clipboard.SetText(text);
            }
            else if (e.Control && !e.Alt && e.KeyCode == Keys.Z)
            {
                UndoHighlighting();
            }
            OnKeyDown(e);
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void TextBox_SelectionChanged(object sender, EventArgs e)
        {
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
                TextBox.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Italic);
                TextBox.ForeColor = SystemColors.InactiveCaption;
                IsWatermarkShowing = true;
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
    }
}
